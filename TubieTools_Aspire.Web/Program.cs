using TubieTools_Aspire.Web;
using TubieTools_Aspire.Web.Components;
using TubieTools_Aspire.Web.Services;
using TubieTools_Aspire.Security.Extensions;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

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

// Register IHttpContextAccessor for resolving the current HTTP context
builder.Services.AddHttpContextAccessor();

// Register the token forwarding handler for dependency injection
// This handler automatically forwards the user's Entra ID token to all downstream API calls
// Registered as Transient to avoid lifetime mismatch with the scoped IHttpContextAccessor
builder.Services.AddTransient<TokenForwardingDelegatingHandler>();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://publicapi");
    })
    .AddHttpMessageHandler<TokenForwardingDelegatingHandler>();

// Public API Client
builder.Services.AddHttpClient<PublicApiClient>(client =>
    {
        client.BaseAddress = new("https+http://publicapi");
    })
    .AddHttpMessageHandler<TokenForwardingDelegatingHandler>();

// Sentiment Analysis API Client
builder.Services.AddHttpClient<SentimentApiClient>(client =>
    {
        client.BaseAddress = new("https+http://sentimentapi");
    })
    .AddHttpMessageHandler<TokenForwardingDelegatingHandler>();

// Forecasting API Client
builder.Services.AddHttpClient<ForecastingApiClient>(client =>
    {
        client.BaseAddress = new("https+http://forecastingapi");
    })
    .AddHttpMessageHandler<TokenForwardingDelegatingHandler>();

// Logistics / OSRM API Client
builder.Services.AddHttpClient<LogisticsApiClient>(client =>
    {
        client.BaseAddress = new("https+http://logisticsOSRM");
    })
    .AddHttpMessageHandler<TokenForwardingDelegatingHandler>();

// Enterprise Automation API Client
builder.Services.AddHttpClient<EnterpriseAutomationClient>(client =>
    {
        client.BaseAddress = new("https+http://enterpriseautomation");
    })
    .AddHttpMessageHandler<TokenForwardingDelegatingHandler>();

// Load configuration from Azure Key Vault
var keyVaultUrl = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    var credential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        credential);
}

// Then add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd--TenantId"]}/v2.0";
    options.ClientId = builder.Configuration["AzureAd--ClientId"];
    options.ClientSecret = builder.Configuration["AzureAd--ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
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
