using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Members;

namespace PadTime.Application.Common.Behaviors;

/// <summary>
/// Auto-provisions a Member record on first authenticated API access.
/// </summary>
public sealed class EnsureMemberExistsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnsureMemberExistsBehavior(
        ICurrentUser currentUser,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_currentUser.IsAuthenticated && !string.IsNullOrEmpty(_currentUser.Subject))
        {
            Console.WriteLine("Matricule: " + _currentUser.Matricule);
            var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);

            if (member is null)
            {
                var result = Member.Create(
                    _currentUser.Subject,
                    _currentUser.Matricule,
                    _currentUser.SiteId,
                    DateTime.UtcNow);

                if (result.IsSuccess)
                {
                    await _memberRepository.AddAsync(result.Value, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        return await next();
    }
}
