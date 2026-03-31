using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job responsible for match lifecycle transitions:
/// - J-1: excludes unpaid participants and transitions private matches to public
/// - Lock: locks matches when their start time is reached
/// - Complete: completes matches when their end time is reached
/// </summary>
public sealed class MatchLifecycleJob : BackgroundService
{
    private static readonly Action<ILogger, Exception?> LogJobStarted =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(LogJobStarted)), "MatchLifecycleJob started");

    private static readonly Action<ILogger, Guid, Exception?> LogTransitionedToPublic =
        LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(2, nameof(LogTransitionedToPublic)), "Match {MatchId} transitioned to public at J-1");

    private static readonly Action<ILogger, Guid, string, Exception?> LogTransitionFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Warning, new EventId(3, nameof(LogTransitionFailed)), "Match {MatchId} J-1 transition failed: {Error}");

    private static readonly Action<ILogger, Exception?> LogDayBeforeError =
        LoggerMessage.Define(LogLevel.Error, new EventId(4, nameof(LogDayBeforeError)), "Error during J-1 match processing");

    private static readonly Action<ILogger, Guid, Exception?> LogMatchLocked =
        LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(5, nameof(LogMatchLocked)), "Match {MatchId} locked at start time");

    private static readonly Action<ILogger, Guid, string, Exception?> LogLockFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Warning, new EventId(6, nameof(LogLockFailed)), "Match {MatchId} lock failed: {Error}");

    private static readonly Action<ILogger, Exception?> LogLockError =
        LoggerMessage.Define(LogLevel.Error, new EventId(7, nameof(LogLockError)), "Error during match locking");

    private static readonly Action<ILogger, Guid, Exception?> LogMatchCompleted =
        LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(8, nameof(LogMatchCompleted)), "Match {MatchId} completed");

    private static readonly Action<ILogger, Guid, string, Exception?> LogCompleteFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Warning, new EventId(9, nameof(LogCompleteFailed)), "Match {MatchId} completion failed: {Error}");

    private static readonly Action<ILogger, Exception?> LogCompleteError =
        LoggerMessage.Define(LogLevel.Error, new EventId(10, nameof(LogCompleteError)), "Error during match completion");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MatchLifecycleJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public MatchLifecycleJob(IServiceScopeFactory scopeFactory, ILogger<MatchLifecycleJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogJobStarted(_logger, null);

        // Run immediately on startup, then on each tick
        await RunAllAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunAllAsync(stoppingToken);
        }
    }

    private async Task RunAllAsync(CancellationToken cancellationToken)
    {
        await ProcessDayBeforeTransitionsAsync(cancellationToken);
        await LockStartedMatchesAsync(cancellationToken);
        await CompleteFinishedMatchesAsync(cancellationToken);
    }

    /// <summary>
    /// J-1 processing: for private matches occurring tomorrow,
    /// exclude unpaid participants then transition to public.
    /// </summary>
    private async Task ProcessDayBeforeTransitionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var matchRepository = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var tomorrow = dateTimeProvider.UtcNow.Date.AddDays(1);
            var matches = await matchRepository.GetMatchesForDayBeforeProcessingAsync(tomorrow, cancellationToken);

            if (matches.Count == 0) return;

            var utcNow = dateTimeProvider.UtcNow;
            var processed = 0;

            foreach (var match in matches.Where(m => m.Status == MatchStatus.Private))
            {
                match.ExcludeUnpaidParticipants(utcNow);

                var result = match.TransitionToPublicAtDeadline(utcNow);
                if (result.IsSuccess)
                {
                    LogTransitionedToPublic(_logger, match.Id, null);
                    processed++;
                }
                else
                {
                    LogTransitionFailed(_logger, match.Id, result.PadTimeError.ToString(), null);
                }
            }

            if (processed > 0)
                await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogDayBeforeError(_logger, ex);
        }
    }

    /// <summary>
    /// Lock matches whose start time has been reached.
    /// Raises MatchIncompleteEvent for matches with fewer than 4 paid participants,
    /// which will create organizer debt.
    /// </summary>
    private async Task LockStartedMatchesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var matchRepository = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var utcNow = dateTimeProvider.UtcNow;
            var matches = await matchRepository.GetMatchesToLockAsync(utcNow, cancellationToken);

            if (matches.Count == 0) return;

            var locked = 0;

            foreach (var match in matches)
            {
                var result = match.Lock(utcNow);
                if (result.IsSuccess)
                {
                    LogMatchLocked(_logger, match.Id, null);
                    locked++;
                }
                else
                {
                    LogLockFailed(_logger, match.Id, result.PadTimeError.ToString(), null);
                }
            }

            if (locked > 0)
                await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogLockError(_logger, ex);
        }
    }

    /// <summary>
    /// Complete matches whose end time has been reached.
    /// </summary>
    private async Task CompleteFinishedMatchesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var matchRepository = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var utcNow = dateTimeProvider.UtcNow;
            var matches = await matchRepository.GetMatchesToCompleteAsync(utcNow, cancellationToken);

            if (matches.Count == 0) return;

            var completed = 0;

            foreach (var match in matches)
            {
                var result = match.Complete(utcNow);
                if (result.IsSuccess)
                {
                    LogMatchCompleted(_logger, match.Id, null);
                    completed++;
                }
                else
                {
                    LogCompleteFailed(_logger, match.Id, result.PadTimeError.ToString(), null);
                }
            }

            if (completed > 0)
                await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCompleteError(_logger, ex);
        }
    }
}
