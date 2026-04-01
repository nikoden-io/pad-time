using FluentAssertions;
using MediatR;
using NSubstitute;
using PadTime.Application.Common.Behaviors;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Common.Behaviors;

public sealed class EnsureMemberExistsBehaviorTests
{
    [Fact]
    public async Task Handle_WhenMemberDoesNotExist_AutoProvisionsMemberBeforeNext()
    {
        var currentUser = CreateCurrentUser("subject-1", "G1234", null);
        var memberRepository = Substitute.For<IMemberRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new EnsureMemberExistsBehavior<TestRequest, string>(currentUser, memberRepository, unitOfWork);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        memberRepository.GetBySubjectAsync("subject-1", Arg.Any<CancellationToken>())
            .Returns((Member?)null);
        memberRepository.GetByMatriculeAsync("G1234", Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be("ok");
        await memberRepository.Received(1).AddAsync(
            Arg.Is<Member>(m => m.Subject == "subject-1" && m.Matricule.Value == "G1234"),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDemoMemberExists_AdoptsDemoMemberWithoutCreatingNewMember()
    {
        var siteId = Guid.NewGuid();
        var currentUser = CreateCurrentUser("subject-1", "S12345", siteId);
        var demoMember = Member.Create("demo-subject", "S12345", siteId, DateTime.UtcNow).Value;
        var memberRepository = Substitute.For<IMemberRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new EnsureMemberExistsBehavior<TestRequest, string>(currentUser, memberRepository, unitOfWork);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        memberRepository.GetBySubjectAsync("subject-1", Arg.Any<CancellationToken>())
            .Returns((Member?)null);
        memberRepository.GetByMatriculeAsync("S12345", Arg.Any<CancellationToken>())
            .Returns(demoMember);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be("ok");
        demoMember.Subject.Should().Be("subject-1");
        await memberRepository.DidNotReceive().AddAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ICurrentUser CreateCurrentUser(string subject, string matricule, Guid? siteId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.Subject.Returns(subject);
        currentUser.Matricule.Returns(matricule);
        currentUser.SiteId.Returns(siteId);
        return currentUser;
    }

    private sealed record TestRequest;
}
