namespace JobTracker.Models;

public static class ApplicationStatusExtensions
{
    public static string DisplayName(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Applied => "Applied",
        ApplicationStatus.InterviewScheduled => "Interview scheduled",
        ApplicationStatus.Interviewed => "Interviewed",
        ApplicationStatus.OfferReceived => "Offer received",
        ApplicationStatus.Rejected => "Rejected",
        ApplicationStatus.Withdrawn => "Withdrawn",
        _ => status.ToString(),
    };

    public static string BadgeClass(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Applied => "bg-secondary",
        ApplicationStatus.InterviewScheduled => "bg-info text-dark",
        ApplicationStatus.Interviewed => "bg-primary",
        ApplicationStatus.OfferReceived => "bg-success",
        ApplicationStatus.Rejected => "bg-danger",
        ApplicationStatus.Withdrawn => "bg-dark",
        _ => "bg-secondary",
    };
}
