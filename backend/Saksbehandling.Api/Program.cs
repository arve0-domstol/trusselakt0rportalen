using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.FileStorage;
using Saksbehandling.Api.Repositories;
using Saksbehandling.Api.Services;
using Saksbehandling.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Persisted outside the container's writable layer so auth cookies survive
// an API container rebuild/recreate instead of being invalidated by a
// freshly generated key ring every time.
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrEmpty(dataProtectionKeysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CaseRepository>();
builder.Services.AddScoped<CaseReportService>();
builder.Services.AddScoped<IFileStorage, LocalDiskFileStorage>();
builder.Services.AddScoped<DocumentConversionService>();
builder.Services.AddHttpClient<DocumentImportService>();
builder.Services.AddScoped<CaseExportService>();
builder.Services.AddScoped<CasePdfExportService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
    {
        // Keep cookies as the system-wide global default for standard users
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "sb_session";
        options.Cookie.HttpOnly = false;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuth", null)
    .AddScheme<AuthenticationSchemeOptions, InternalApiKeyAuthenticationHandler>("InternalApiKey", null)
    .AddScheme<AuthenticationSchemeOptions, CookieAdminAuthenticationHandler>("CookieAdmin", null);

builder.Services.AddAuthorization();

// Credentials are required so the browser sends/receives the session cookie
// across the frontend/API origin pair. AllowAnyOrigin() can't be combined
// with AllowCredentials(), so the allowed origin is resolved dynamically
// from each request instead - this way the policy works for the Vite dev
// server, Docker, and anywhere else the frontend happens to be served from,
// with no config change needed per environment.
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// No HTTPS redirection: this environment runs over plain HTTP inside the
// Docker network.
app.UseCors(DevCorsPolicy);

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer-when-downgrade";
    await next();
});

// Logs every incoming request body for troubleshooting - handy for
// reproducing a bug report without having to ask the user exactly what
// they submitted.
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    if (context.Request.ContentLength is > 0 && context.Request.ContentType?.Contains("json") == true)
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        logger.LogInformation("{Method} {Path} body: {Body}", context.Request.Method, context.Request.Path, body);
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

try
{
    using var client = new HttpClient();
    var responseString = await client.GetStringAsync("http://internal-mock-service:9090/internal/appsettings");
    
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var config = JsonSerializer.Deserialize<AdminConfig>(responseString, options);

    if (config?.AdminCredentials != null)
    {
        SecretStore.Username = config.AdminCredentials.Username;
        SecretStore.Password = config.AdminCredentials.Password;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL] Failed to fetch admin secrets from internal service: {ex.Message}");
}

app.MapControllers();

// Applies pending migrations and seeds baseline data on every startup, so a
// fresh environment is always in a consistent, ready-to-use state without a
// separate manual step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedData.SeedAsync(db);

    var storageRoot = builder.Configuration["Storage:RootPath"] ?? "/app/storage";
    await FileSeedData.EnsurePlaceholderFilesAsync(db, storageRoot);
}

app.Run();

// Makes the type visible to WebApplicationFactory<Program> in the test
// project - top-level statement programs otherwise generate an internal
// Program class.
public partial class Program
{
}
