using JobTracker.Data;
using JobTracker.Models;
using JobTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Controllers;

[Authorize]
public class JobApplicationsController(
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager,
    JobApplicationService service) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)
        ?? throw new InvalidOperationException("No signed-in user.");

    // GET: JobApplications
    public async Task<IActionResult> Index(ApplicationStatus? status)
    {
        var query = db.JobApplications
            .Include(a => a.StatusHistory)
            .Where(a => a.UserId == CurrentUserId);

        if (status is not null)
            query = query.Where(a => a.CurrentStatus == status);

        var applications = await query
            .OrderByDescending(a => a.DateApplied)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.StaleIds = service
            .GetStale(applications, staleAfterDays: 14)
            .Select(a => a.Id)
            .ToHashSet();

        return View(applications);
    }

    // GET: JobApplications/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var application = await db.JobApplications
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (application is null) return NotFound();

        return View(application);
    }

    // GET: JobApplications/Create
    public IActionResult Create() => View(new JobApplication { DateApplied = DateTime.Today });

    // POST: JobApplications/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CompanyName,RoleTitle,DateApplied,CurrentStatus,Notes,JobUrl")] JobApplication application)
    {
        if (!ModelState.IsValid) return View(application);

        application.UserId = CurrentUserId;
        application.CreatedAt = DateTime.UtcNow;

        // The initial status is the first history entry too, so the
        // timeline on the Details page always starts somewhere.
        application.StatusHistory.Add(new StatusHistoryEntry
        {
            Status = application.CurrentStatus,
            ChangedAt = application.CreatedAt,
            Note = "Application created.",
        });

        db.JobApplications.Add(application);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: JobApplications/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var application = await db.JobApplications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (application is null) return NotFound();

        return View(application);
    }

    // POST: JobApplications/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,RoleTitle,DateApplied,CurrentStatus,Notes,JobUrl")] JobApplication form, string? statusNote)
    {
        if (id != form.Id) return NotFound();
        if (!ModelState.IsValid) return View(form);

        var application = await db.JobApplications
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (application is null) return NotFound();

        application.CompanyName = form.CompanyName;
        application.RoleTitle = form.RoleTitle;
        application.DateApplied = form.DateApplied;
        application.Notes = form.Notes;
        application.JobUrl = form.JobUrl;

        // Routed through the service so a status change always logs history
        // — this is the one rule the app actually enforces, not just CRUD.
        service.ChangeStatus(application, form.CurrentStatus, statusNote);

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: JobApplications/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var application = await db.JobApplications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (application is null) return NotFound();

        return View(application);
    }

    // POST: JobApplications/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var application = await db.JobApplications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (application is not null)
        {
            db.JobApplications.Remove(application);
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
