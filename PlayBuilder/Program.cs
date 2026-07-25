using Microsoft.EntityFrameworkCore;
using PlayBuilder.Components;
using PlayBuilder.Data;
using PlayBuilder.Services;
using PlayBuilder.Services.Atlas;
using PlayBuilder.Services.Atlas.Rules;

var builder = WebApplication.CreateBuilder(args);

// PlayBuilder application services.
builder.Services.AddSingleton<ISettingsService, JsonSettingsService>();
builder.Services.AddSingleton<IScanReportService, JsonScanReportService>();
builder.Services.AddSingleton<IAtlasProfileService, JsonAtlasProfileService>();
builder.Services.AddSingleton<CollectionRuleService>();
builder.Services.AddSingleton<ICollectionRuleService, AtlasCollectionRuleService>();
builder.Services.AddSingleton<IAtlasComparisonService, AtlasComparisonService>();
builder.Services.AddSingleton<ICollectionService, CollectionService>();

// Atlas is registered as a parallel decision engine during its staged migration.
builder.Services.AddSingleton<FilenameTokenizer>();
builder.Services.AddSingleton<FilenameMetadataParser>();
builder.Services.AddSingleton<AtlasCandidateFactory>();
builder.Services.AddSingleton<IAtlasRule, LanguageRule>();
builder.Services.AddSingleton<IAtlasRule, RegionRule>();
builder.Services.AddSingleton<IAtlasRule, RevisionRule>();
builder.Services.AddSingleton<IAtlasRule, VersionRule>();
builder.Services.AddSingleton<IAtlasRule, SpecialReleaseRule>();
builder.Services.AddSingleton<IAtlasRule, DumpQualityRule>();
builder.Services.AddSingleton<AtlasDecisionEngine>();

// A factory is safe to use from long-lived Blazor components and singleton services.
builder.Services.AddDbContextFactory<PlayBuilderDbContext>(options =>
    options.UseSqlite("Data Source=playbuilder.db"));

builder.Services.AddSingleton<IArchiveScanner, ArchiveScanner>();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Create the SQLite database and current tables on first launch.
await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<PlayBuilderDbContext>>();

    await using var database = await factory.CreateDbContextAsync();
    await DatabaseInitializer.InitializeAsync(database);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
