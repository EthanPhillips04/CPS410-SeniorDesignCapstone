using SportsAnalytics.Web.Services;
using Microsoft.EntityFrameworkCore;
using SportsAnalytics.Web.Components;
using SportsAnalytics.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Register PostgreSQL database
builder.Services.AddDbContextFactory<SportsDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


// Register NBA import service with HTTP client. Using BALLDONTLIE API key
builder.Services.AddHttpClient<NbaImportService>(
    (serviceProvider, client) =>
    {
        var config = serviceProvider
            .GetRequiredService<IConfiguration>();

        string apiKey = config["BallDontLie:ApiKey"]
            ?? throw new InvalidOperationException(
                "BALLDONTLIE API key is missing.");

        client.BaseAddress =
            new Uri("https://api.balldontlie.io/");

        client.DefaultRequestHeaders.Add(
            "Authorization", apiKey);

        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Register Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
