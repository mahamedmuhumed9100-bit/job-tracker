using JobTracker.Models;
using JobTracker.Services;

namespace JobTracker.Tests;

public class JobApplicationServiceTests
{
    private static JobApplication NewApplication(ApplicationStatus status = ApplicationStatus.Applied, DateTime? createdAt = null) => new()
    {
        Id = 1,
        UserId = "user-1",
        CompanyName = "Acme Corp",
        RoleTitle = "Software Engineer Intern",
        DateApplied = new DateTime(2026, 1, 1),
        CurrentStatus = status,
        CreatedAt = createdAt ?? new DateTime(2026, 1, 1),
    };

    [Fact]
    public void ChangeStatus_UpdatesCurrentStatus_WhenStatusDiffers()
    {
        var service = new JobApplicationService();
        var app = NewApplication();

        var changed = service.ChangeStatus(app, ApplicationStatus.InterviewScheduled, "Phone screen booked");

        Assert.True(changed);
        Assert.Equal(ApplicationStatus.InterviewScheduled, app.CurrentStatus);
    }

    [Fact]
    public void ChangeStatus_AppendsHistoryEntry_WhenStatusDiffers()
    {
        var service = new JobApplicationService();
        var app = NewApplication();
        var when = new DateTime(2026, 1, 15);

        service.ChangeStatus(app, ApplicationStatus.Interviewed, "Went well", when);

        var entry = Assert.Single(app.StatusHistory);
        Assert.Equal(ApplicationStatus.Interviewed, entry.Status);
        Assert.Equal("Went well", entry.Note);
        Assert.Equal(when, entry.ChangedAt);
    }

    [Fact]
    public void ChangeStatus_DoesNothing_WhenStatusIsUnchanged()
    {
        var service = new JobApplicationService();
        var app = NewApplication(ApplicationStatus.Applied);

        var changed = service.ChangeStatus(app, ApplicationStatus.Applied, "duplicate click");

        Assert.False(changed);
        Assert.Empty(app.StatusHistory);
    }

    [Fact]
    public void ChangeStatus_CanRecordMultipleTransitionsInOrder()
    {
        var service = new JobApplicationService();
        var app = NewApplication();

        service.ChangeStatus(app, ApplicationStatus.InterviewScheduled, null, new DateTime(2026, 1, 5));
        service.ChangeStatus(app, ApplicationStatus.Interviewed, null, new DateTime(2026, 1, 12));
        service.ChangeStatus(app, ApplicationStatus.OfferReceived, null, new DateTime(2026, 1, 20));

        Assert.Equal(3, app.StatusHistory.Count);
        Assert.Equal(
            [ApplicationStatus.InterviewScheduled, ApplicationStatus.Interviewed, ApplicationStatus.OfferReceived],
            app.StatusHistory.Select(h => h.Status));
    }

    [Theory]
    [InlineData(ApplicationStatus.OfferReceived, true)]
    [InlineData(ApplicationStatus.Rejected, true)]
    [InlineData(ApplicationStatus.Withdrawn, true)]
    [InlineData(ApplicationStatus.Applied, false)]
    [InlineData(ApplicationStatus.InterviewScheduled, false)]
    [InlineData(ApplicationStatus.Interviewed, false)]
    public void IsFinal_ReflectsWhetherStatusEndsTheProcess(ApplicationStatus status, bool expected)
    {
        var service = new JobApplicationService();

        Assert.Equal(expected, service.IsFinal(status));
    }

    [Fact]
    public void DaysSinceApplied_ComputesWholeDaysBetweenDates()
    {
        var service = new JobApplicationService();
        var app = NewApplication();
        app.DateApplied = new DateTime(2026, 1, 1);

        var days = service.DaysSinceApplied(app, today: new DateTime(2026, 1, 11));

        Assert.Equal(10, days);
    }

    [Fact]
    public void DaysSinceApplied_NeverReturnsNegative_ForFutureDates()
    {
        var service = new JobApplicationService();
        var app = NewApplication();
        app.DateApplied = new DateTime(2026, 2, 1);

        var days = service.DaysSinceApplied(app, today: new DateTime(2026, 1, 1));

        Assert.Equal(0, days);
    }

    [Fact]
    public void GetStale_ReturnsApplications_WithNoRecentActivity()
    {
        var service = new JobApplicationService();
        var today = new DateTime(2026, 2, 1);

        var stale = NewApplication(createdAt: new DateTime(2026, 1, 1));
        stale.Id = 1;
        var fresh = NewApplication(createdAt: new DateTime(2026, 1, 28));
        fresh.Id = 2;

        var result = service.GetStale([stale, fresh], staleAfterDays: 14, today: today).ToList();

        Assert.Contains(stale, result);
        Assert.DoesNotContain(fresh, result);
    }

    [Fact]
    public void GetStale_ExcludesApplicationsInAFinalState()
    {
        var service = new JobApplicationService();
        var today = new DateTime(2026, 2, 1);
        var oldButDone = NewApplication(ApplicationStatus.Rejected, createdAt: new DateTime(2026, 1, 1));

        var stale = service.GetStale([oldButDone], staleAfterDays: 14, today: today).ToList();

        Assert.Empty(stale);
    }

    [Fact]
    public void GetStale_UsesMostRecentHistoryEntry_NotJustCreatedDate()
    {
        var service = new JobApplicationService();
        var today = new DateTime(2026, 2, 1);
        var app = NewApplication(createdAt: new DateTime(2026, 1, 1));
        // Created a month ago, but updated 5 days ago — should NOT count as stale.
        app.StatusHistory.Add(new StatusHistoryEntry { Status = ApplicationStatus.InterviewScheduled, ChangedAt = new DateTime(2026, 1, 27) });

        var stale = service.GetStale([app], staleAfterDays: 14, today: today).ToList();

        Assert.Empty(stale);
    }

    [Fact]
    public void ChangeStatus_ThrowsOnNullApplication()
    {
        var service = new JobApplicationService();

        Assert.Throws<ArgumentNullException>(() => service.ChangeStatus(null!, ApplicationStatus.Rejected, null));
    }
}
