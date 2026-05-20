using FluentAssertions;
using PadTime.Application.Booking.Commands.CreateMatch;
using PadTime.Application.Booking.Commands.JoinMatch;
using PadTime.Application.Sites.Commands.ActivateSite;
using PadTime.Application.Sites.Commands.AddSiteClosure;
using PadTime.Application.Sites.Commands.CreateCourt;
using PadTime.Application.Sites.Commands.CreateSite;
using PadTime.Application.Sites.Commands.CreateSiteSchedule;
using PadTime.Application.Sites.Commands.DeleteCourt;
using PadTime.Application.Sites.Commands.DeleteSite;
using PadTime.Application.Sites.Commands.DeleteSiteSchedule;
using PadTime.Application.Sites.Commands.RemoveSiteClosure;
using PadTime.Application.Sites.Commands.UpdateCourt;
using PadTime.Application.Sites.Commands.UpdateSite;
using PadTime.Application.Sites.Commands.UpdateSiteSchedule;
using PadTime.Application.Sites.Queries.GetCourtById;
using PadTime.Domain.Booking;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Tests.Application.Validators;

public sealed class ValidatorTests
{
    [Fact]
    public void CreateMatchCommandValidator_WhenTooManyPrivateParticipants_HasError() =>
        new CreateMatchCommandValidator()
            .Validate(new CreateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), PadMatchType.Private, ["A", "B", "C", "D"]))
            .IsValid.Should().BeFalse();

    [Fact]
    public void JoinMatchCommandValidator_WhenIdempotencyKeyIsTooLong_HasError() =>
        new JoinMatchCommandValidator()
            .Validate(new JoinMatchCommand(Guid.NewGuid(), new string('a', 101)))
            .IsValid.Should().BeFalse();

    [Fact]
    public void ActivateSiteCommandValidator_WhenSiteIdIsEmpty_HasError() =>
        new ActivateSiteCommandValidator().Validate(new ActivateSiteCommand(Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void AddSiteClosureCommandValidator_WhenReducedHoursAreInvalid_HasError() =>
        new AddSiteClosureCommandValidator()
            .Validate(new AddSiteClosureCommand(Guid.NewGuid(), ClosureType.ReducedHours, ClosureReason.Maintenance, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), null, new TimeOnly(10, 0), new TimeOnly(9, 0), null))
            .IsValid.Should().BeFalse();

    [Fact]
    public void CreateCourtCommandValidator_WhenLabelMissing_HasError() =>
        new CreateCourtCommandValidator().Validate(new CreateCourtCommand(Guid.NewGuid(), string.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void CreateSiteCommandValidator_WhenTimezoneIsInvalid_HasError() =>
        new CreateSiteCommandValidator().Validate(new CreateSiteCommand("Main", "1", "Street", "1000", "City", "Country", "Invalid/Timezone")).IsValid.Should().BeFalse();

    [Fact]
    public void CreateSiteScheduleCommandValidator_WhenClosingBeforeOpening_HasError() =>
        new CreateSiteScheduleCommandValidator().Validate(new CreateSiteScheduleCommand(Guid.NewGuid(), "Regular", new DateOnly(2026, 1, 1), null, new TimeOnly(10, 0), new TimeOnly(9, 0), null, 1)).IsValid.Should().BeFalse();

    [Fact]
    public void DeleteCourtCommandValidator_WhenIdsMissing_HasError() =>
        new DeleteCourtCommandValidator().Validate(new DeleteCourtCommand(Guid.Empty, Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void DeleteSiteCommandValidator_WhenIdMissing_HasError() =>
        new DeleteSiteCommandValidator().Validate(new DeleteSiteCommand(Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void DeleteSiteScheduleCommandValidator_WhenIdsMissing_HasError() =>
        new DeleteSiteScheduleCommandValidator().Validate(new DeleteSiteScheduleCommand(Guid.Empty, Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void RemoveSiteClosureCommandValidator_WhenIdsMissing_HasError() =>
        new RemoveSiteClosureCommandValidator().Validate(new RemoveSiteClosureCommand(Guid.Empty, Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void UpdateCourtCommandValidator_WhenLabelTooLong_HasError() =>
        new UpdateCourtCommandValidator().Validate(new UpdateCourtCommand(Guid.NewGuid(), Guid.NewGuid(), new string('a', 101))).IsValid.Should().BeFalse();

    [Fact]
    public void UpdateSiteCommandValidator_WhenTimezoneInvalid_HasError() =>
        new UpdateSiteCommandValidator().Validate(new UpdateSiteCommand(Guid.NewGuid(), "Main", "1", "Street", "1000", "City", "Country", "Bad/Timezone")).IsValid.Should().BeFalse();

    [Fact]
    public void UpdateSiteScheduleCommandValidator_WhenOperatingHoursTooShort_HasError() =>
        new UpdateSiteScheduleCommandValidator().Validate(new UpdateSiteScheduleCommand(Guid.NewGuid(), Guid.NewGuid(), "Regular", DateOnly.FromDateTime(DateTime.UtcNow), null, new TimeOnly(10, 0), new TimeOnly(10, 30), null, 1)).IsValid.Should().BeFalse();

    [Fact]
    public void GetCourtByIdQueryValidator_WhenCourtIdMissing_HasError() =>
        new GetCourtByIdQueryValidator().Validate(new GetCourtByIdQuery(Guid.Empty)).IsValid.Should().BeFalse();
}
