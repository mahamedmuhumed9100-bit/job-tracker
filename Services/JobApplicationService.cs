using JobTracker.Models;

namespace JobTracker.Services;

/// <summary>
/// Pure business logic for job applications, kept separate from EF Core and
/// the controllers so it can be unit tested without a database.
/// </summary>
public class JobApplicationService
{
    /// <summary>
    /// Applies a status change to an application. If the status actually
    /// changed, appends a StatusHistoryEntry recording it — this is what
    /// keeps the history table meaningful instead of just a mirror of edits.
    /// Returns true if a change (and history entry) was made.
    /// </summary>
    public bool ChangeStatus(JobApplication application, ApplicationStatus newStatus, string? note, DateTime? at = null)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (application.CurrentStatus == newStatus)
            return false;

        application.CurrentStatus = newStatus;
        application.StatusHistory.Add(new StatusHistoryEntry
        {
            JobApplicationId = application.Id,
            Status = newStatus,
            Note = note,
            ChangedAt = at ?? DateTime.UtcNow,
        });

        return true;
    }

    /// <summary>
    /// True once an application has reached a state that won't change again
    /// through the normal application process.
    /// </summary>
    public bool IsFinal(ApplicationStatus status) =>
        status is ApplicationStatus.OfferReceived or ApplicationStatus.Rejected or ApplicationStatus.Withdrawn;

    /// <summary>
    /// Days between application and today (or the final status, if the
    /// application has already reached one) — used on the dashboard to
    /// surface applications that have gone quiet.
    /// </summary>
    public int DaysSinceApplied(JobApplication application, DateTime? today = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        var reference = today ?? DateTime.UtcNow.Date;
        return Math.Max(0, (reference.Date - application.DateApplied.Date).Days);
    }

    /// <summary>
    /// Applications with no update in the given number of days that also
    /// aren't in a final state — the "needs a follow-up email" list.
    /// </summary>
    public IEnumerable<JobApplication> GetStale(IEnumerable<JobApplication> applications, int staleAfterDays, DateTime? today = null)
    {
        ArgumentNullException.ThrowIfNull(applications);
        var reference = today ?? DateTime.UtcNow.Date;

        foreach (var app in applications)
        {
            if (IsFinal(app.CurrentStatus)) continue;

            var lastChange = app.StatusHistory.Count > 0
                ? app.StatusHistory.Max(h => h.ChangedAt)
                : app.CreatedAt;

            if ((reference - lastChange.Date).Days >= staleAfterDays)
                yield return app;
        }
    }
}
