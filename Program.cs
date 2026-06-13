using BullethellPrototype.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PatternCatalog>();
builder.Services.AddSingleton<GameContentExporter>();
builder.Services.AddSingleton<GameStageCatalog>();

var app = builder.Build();

var stageCatalog = app.Services.GetRequiredService<GameStageCatalog>();
var contentExporter = app.Services.GetRequiredService<GameContentExporter>();
contentExporter.ExportSharedContent();
await stageCatalog.ExportUnityJsonAsync(app.Environment.ContentRootPath);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/patterns", (PatternCatalog catalog) => Results.Ok(catalog.GetSummaries()));

app.MapGet("/api/patterns/{id}", (string id, PatternCatalog catalog) =>
{
    var sample = catalog.TryBuildSample(id);
    return sample is null ? Results.NotFound() : Results.Ok(sample);
});

app.MapGet("/api/game/stages", (GameStageCatalog catalog) => Results.Ok(catalog.GetSummaries()));

app.MapGet("/api/game/stages/{id}", (string id, GameStageCatalog catalog) =>
{
    var stage = catalog.TryGet(id);
    return stage is null ? Results.NotFound() : Results.Ok(stage);
});

app.MapGet("/api/game/stages/validate", (GameStageCatalog catalog) =>
{
    catalog.ValidateOrThrow();
    return Results.Ok(new { valid = true });
});

app.MapFallbackToFile("index.html");

app.Run();
