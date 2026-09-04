using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobTracker.Data;
using JobTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = ResolveConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// RequireConfirmedAccount is off because this demo has no email sender wired
// up — in a real deployment this would be true, backed by an actual mail
// provider (e.g. SendGrid) to confirm the address before first sign-in.
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JobApplicationService>();

var app = builder.Build();

// Apply any pending migrations on startup. Fine for a small single-instance
// app like this one; a larger production setup would run migrations as a
// separate release step instead of on every app boot.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Render (and Heroku-style platforms) inject a single DATABASE_URL env var in
// the "postgres://user:pass@host:port/dbname" URI form. Npgsql wants a
// keyword-value string instead, so this converts one to the other. Falls
// back to appsettings/user-secrets ConnectionStrings:DefaultConnection for
// local development, where a plain keyword-value string is simplest.
static string ResolveConnectionString(IConfiguration configuration)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No DATABASE_URL env var and no ConnectionStrings:DefaultConnection configured. " +
                "Set one of them — see README.md for local setup with docker compose.");
    }

    var uri = new Uri(databaseUrl);
    var userInfoParts = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfoParts[0]);
    var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');

    return new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = username,
        Password = password,
        Database = database,
        SslMode = Npgsql.SslMode.Require,
    }.ConnectionString;
}
