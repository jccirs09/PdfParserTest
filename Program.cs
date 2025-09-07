using Microsoft.AspNetCore.Http;
using PdfParserTest.Components;
using PdfParserTest.Models;
using PdfParserTest.Parsing;
using PdfParserTest.Parsing.Strategies;
using System.IO;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ITextParser, PlainTextPickingListParser>();
builder.Services.AddSingleton<IPdfParseStrategy, PdfPigStrategy>();
builder.Services.AddSingleton<IPdfParseStrategy, PopplerStrategy>();
builder.Services.AddSingleton<IPdfParseStrategy, OcrStrategy>();
builder.Services.AddSingleton<ParsingEngine>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/pickinglists/parse", async (HttpRequest req, ParsingEngine engine) =>
{
    if (!req.HasFormContentType) return Results.BadRequest("multipart/form-data required");
    var form = await req.ReadFormAsync();
    var file = form.Files.GetFile("pdf");
    if (file is null || file.Length == 0) return Results.BadRequest("No file uploaded.");
    await using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    ms.Position = 0;
    var dto = engine.Parse(ms);
    return Results.Ok(dto);
});

app.Run();
