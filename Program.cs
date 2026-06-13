using BullethellPrototype.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PatternCatalog>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/patterns", (PatternCatalog catalog) => Results.Ok(catalog.GetSummaries()));

app.MapGet("/api/patterns/{id}", (string id, PatternCatalog catalog) =>
{
    var sample = catalog.TryBuildSample(id);
    return sample is null ? Results.NotFound() : Results.Ok(sample);
});

app.MapFallbackToFile("index.html");

app.Run();
