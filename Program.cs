using PickingListApp.Components;
using PickingListApp.Data;
using PickingListApp.Endpoints;
using PickingListApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Add EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=pickinglist.db"));

// Add custom app services
builder.Services.AddScoped<IPickingListParser, PdfPigPickingListParser>();
builder.Services.AddScoped<PickingListService>();

// Add API Explorer for OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Needed for API calls from Blazor components
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API endpoints
app.MapPickingListEndpoints();

app.Run();
