using MediatR;
using Microsoft.Extensions.Logging;

namespace PadTime.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request handling.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Action<ILogger, string, Exception?> LogHandling =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(LogHandling)),
            "Handling {RequestName}");

    private readonly Action<ILogger, string, Exception?> LogHandled =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(LogHandled)),
            "Handled {RequestName}");

    private readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, nameof(LogError)),
            "Error handling {RequestName}");

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        LogHandling(_logger, requestName, null);

        try
        {
            var response = await next();

            LogHandled(_logger, requestName, null);

            return response;
        }
        catch (Exception ex)
        {
            LogError(_logger, requestName, ex);
            throw;
        }
    }
}
