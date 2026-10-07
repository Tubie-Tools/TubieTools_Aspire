//using Serilog; 
using DataAccessLayer.Data.Contexts;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ServiceLayer;
using TubieTools_Map.Components;
using TubieTools_Map.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    //.WriteTo.Console()
//    .WriteTo.File("logs/tubietools-map-.txt", rollingInterval: RollingInterval.Day)
//    .Enrich.FromLogContext()
//    .CreateLogger();

//builder.Host.UseSerilog();

// Add services
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddMicrosoftIdentityConsentHandler();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=MapApp.db";

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MapAppDbContext>(options =>
        options.UseInMemoryDatabase("MapAppDb"));
}
else
{
    builder.Services.AddDbContext<MapAppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
 
// Add HTTP clients
builder.Services
    .AddHttpClient<LogisticsOSRMClient>()
    .ConfigureHttpClient(client =>
    {
        var baseUrl = builder.Configuration["LogisticsOSRM:BaseUrl"] ?? "http://logisticsosrm:8080";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("User-Agent", "TubieTools.Map/1.0");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Add services
builder.Services.AddScoped<RouteService>();
//builder.Services.AddScoped<AccountService>();

var app = builder.Build();

// Configure HTTP middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();