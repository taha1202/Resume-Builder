using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ResumeBuilder.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- NEW: Add Application Insights Telemetry ---
// This automatically reads "ApplicationInsights:ConnectionString" from appsettings.json
builder.Services.AddApplicationInsightsTelemetry();

// Register custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<AzureBlobService>();

// Configure HttpClient for API calls
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7234/api/";
builder.Services.AddHttpClient<ResumeService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<PdfService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
// Add HttpClient for AuthService
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();