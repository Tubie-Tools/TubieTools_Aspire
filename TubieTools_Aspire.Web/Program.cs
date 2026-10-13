    using TubieTools_Aspire.Web;
using TubieTools_Aspire.Web.Components;
using TubieTools_Aspire.Web.Services;
using TubieTools_Aspire.Security.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Entra ID authentication (OIDC flow for interactive Blazor app)
builder.AddEntraIdAuthentication();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();
builder.Services.AddScoped<CartService>();
builder.Services.AddTransient<IPaymentService, PaymentService>();

// Register all microservice HTTP clients
builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://publicapi");
    });

// Public API Client
builder.Services.AddHttpClient<PublicApiClient>(client =>
    {
        client.BaseAddress = new("https+http://publicapi");
    });

// Sentiment Analysis API Client
builder.Services.AddHttpClient<SentimentApiClient>(client =>
    {
        client.BaseAddress = new("https+http://sentimentapi");
    });

// Forecasting API Client
builder.Services.AddHttpClient<ForecastingApiClient>(client =>
    {
        client.BaseAddress = new("https+http://forecastingapi");
    });

// Logistics / OSRM API Client
builder.Services.AddHttpClient<LogisticsApiClient>(client =>
    {
        client.BaseAddress = new("https+http://logisticsOSRM");
    });

// Enterprise Automation API Client
builder.Services.AddHttpClient<EnterpriseAutomationClient>(client =>
    {
        client.BaseAddress = new("https+http://enterpriseautomation");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
