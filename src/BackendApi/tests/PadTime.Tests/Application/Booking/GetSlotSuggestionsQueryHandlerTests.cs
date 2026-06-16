using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PadTime.Application.Booking.Queries.GetSlotSuggestions;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Booking;

public sealed class GetSlotSuggestionsQueryHandlerTests
{
    private static readonly DateTime Clock = new(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

    private static GetSlotSuggestionsQueryHandler Build(IMemberRepository members)
    {
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(Clock);
        return new GetSlotSuggestionsQueryHandler(
            Substitute.For<ICurrentUser>(),
            members,
            Substitute.For<IMatchRepository>(),
            Substitute.For<ISiteRepository>(),
            Substitute.For<ISiteStatisticsRepository>(),
            Substitute.For<ISlotSuggestionService>(),
            Substitute.For<IMediator>(),
            clock,
            Substitute.For<ILogger<GetSlotSuggestionsQueryHandler>>());
    }

    [Fact]
    public async Task Handle_WhenMemberMissing_ReturnsFallbackResponse()
    {
        var members = Substitute.For<IMemberRepository>();
        members.GetBySubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Member?)null);

        var result = await Build(members).Handle(new GetSlotSuggestionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FallbackUsed.Should().BeTrue();
        result.Value.Suggestions.Should().BeEmpty();
        result.Value.GeneratedAtUtc.Should().Be(Clock);
    }

    [Fact]
    public async Task Handle_WhenDependencyThrows_ReturnsFallbackResponse()
    {
        var members = Substitute.For<IMemberRepository>();
        members.GetBySubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Member?>(_ => throw new InvalidOperationException("boom"));

        var result = await Build(members).Handle(new GetSlotSuggestionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FallbackUsed.Should().BeTrue();
        result.Value.Suggestions.Should().BeEmpty();
    }
}
