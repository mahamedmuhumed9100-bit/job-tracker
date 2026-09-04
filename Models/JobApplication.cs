using System.ComponentModel.DataAnnotations;

namespace JobTracker.Models;

public class JobApplication
{
    public int Id { get; set; }

    // Owner of this record — set from the signed-in user in the controller,
    // after model binding, never from form input. Deliberately NOT [Required]:
    // it's empty at the point ModelState is validated (binding runs before
    // the controller assigns it), so requiring it here would fail every
    // Create/Edit post before the controller ever gets a chance to set it.
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    [Display(Name = "Company")]
    public string CompanyName { get; set; } = string.Empty;

    [Required, StringLength(120)]
    [Display(Name = "Role")]
    public string RoleTitle { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date applied")]
    public DateTime DateApplied { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Status")]
    public ApplicationStatus CurrentStatus { get; set; } = ApplicationStatus.Applied;

    [StringLength(2000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    [Display(Name = "Job listing URL")]
    [Url]
    public string? JobUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // One application has many status-history entries — the relationship
    // this project exists to demonstrate.
    public List<StatusHistoryEntry> StatusHistory { get; set; } = [];
}
