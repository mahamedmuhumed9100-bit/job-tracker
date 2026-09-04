using JobTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<StatusHistoryEntry> StatusHistoryEntries => Set<StatusHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<JobApplication>()
            .HasMany(a => a.StatusHistory)
            .WithOne(h => h.JobApplication)
            .HasForeignKey(h => h.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobApplication>()
            .Property(a => a.CurrentStatus)
            .HasConversion<string>();

        builder.Entity<StatusHistoryEntry>()
            .Property(h => h.Status)
            .HasConversion<string>();
    }
}
