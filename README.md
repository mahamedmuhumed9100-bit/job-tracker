# JobTracker

An ASP.NET Core MVC app for tracking job & internship applications — company, role,
status, and a full history of how each one progressed. Built to actually use during
my own job search, and to back up the C# / ASP.NET MVC / SQL skills on my CV with a
real project, the same way [algo-visualizer](https://github.com/mahamedmuhumed9100-bit/algo-visualizer)
backs up React.

## What it does

- Register / sign in (ASP.NET Core Identity)
- Add, edit, and delete job applications: company, role, date applied, status, notes, listing URL
- Every status change (Applied → Interview Scheduled → Interviewed → Offer/Rejected/Withdrawn)
  is logged to a status-history table with a timestamp and an optional note — the
  one-to-many relationship this project exists to demonstrate
- Dashboard filterable by status, with applications flagged if they've had no update
  in 14+ days ("follow up?")

## Tech stack

- ASP.NET Core 10 MVC (Controllers + Razor views)
- ASP.NET Core Identity for auth
- Entity Framework Core + PostgreSQL (via Npgsql)
- xUnit for unit tests
- Docker + [Render](https://render.com) for deployment (`render.yaml` provisions both
  the web service and a free Postgres database in one go)

## Project structure

The core business rule — a status change always logs a history entry — lives in
[`Services/JobApplicationService.cs`](Services/JobApplicationService.cs), kept
deliberately free of EF Core or ASP.NET Core dependencies so it can be unit tested
in isolation. [`JobTracker.Tests`](JobTracker.Tests) covers it with 16 tests: status
transitions, history ordering, the "stale application" detection, and edge cases
(no-op status changes, future dates, final states).

## Running locally

Needs a local Postgres — `docker-compose.yml` starts one:

```bash
docker compose up -d      # starts a local Postgres on :5432
dotnet restore
dotnet ef database update
dotnet run
```

Then open the URL `dotnet run` prints (Identity's email-confirmation requirement is
switched off for this demo — there's no mail sender wired up locally — so you can
register and sign straight in).

## Tests

```bash
dotnet test
```

## Deployment

Deployed via [Render](https://render.com) as a Docker web service, with a free
managed Postgres database. `render.yaml` defines both — from Render's dashboard:
**New → Blueprint → connect this repo → Apply**, and it provisions the database,
wires its connection string to the web service automatically (`DATABASE_URL`), and
deploys. `Program.cs` parses that URL into the keyword-value format Npgsql expects,
and runs any pending migrations on startup.
