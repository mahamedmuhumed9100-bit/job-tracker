using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobTracker.Models;

public class StatusHistoryEntry
{
    public int Id { get; set; }

    [Required]
    public int JobApplicationId { get; set; }

    [ForeignKey(nameof(JobApplicationId))]
    public JobApplication? JobApplication { get; set; }

    [Required]
    public ApplicationStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Note { get; set; }
}
