using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobTracker.Data;

/// <summary>
/// Used only by `dotnet ef` tooling (migrations add/update) at design time.
/// Without this, EF's tooling falls back to partially running Program.cs to
/// discover the DbContext — which would also run this app's startup
/// Database.Migrate() call and try to connect to a real database just to
/// scaffold a migration. This factory sidesteps all of that: it never
/// connects, it just needs a valid-looking Npgsql connection string for EF
/// to generate provider-correct migration code against.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=jobtracker;Username=jobtracker;Password=jobtracker");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
