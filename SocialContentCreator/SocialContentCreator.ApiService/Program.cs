using Microsoft.EntityFrameworkCore;
using SocialContentCreator.ApiService.Data;
using SocialContentCreator.ApiService.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (health checks, telemetry, service discovery, resilience)
builder.AddServiceDefaults();

// Add Azure integrations
builder.AddAzureBlobServiceClient("blobs");
builder.AddRedisClient("cache");

// Add Entity Framework with connection string
var connectionString = builder.Configuration.GetConnectionString("socialdb") ?? 
                      "Server=(localdb)\\mssqllocaldb;Database=SocialContentDb;Trusted_Connection=true;MultipleActiveResultSets=true";
builder.Services.AddDbContext<SocialContentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add application services
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<ISocialMediaService, SocialMediaService>();
builder.Services.AddScoped<IUrlScrapingService, UrlScrapingService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IAIContentGenerationService, AIContentGenerationService>();
builder.Services.AddScoped<IBrandVoiceService, BrandVoiceService>();

// Add HTTP clients
builder.Services.AddHttpClient<IUrlScrapingService, UrlScrapingService>();

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Map default endpoints first (health checks and telemetry)
app.MapDefaultEndpoints();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SocialContentDbContext>();
    context.Database.EnsureCreated();
}

// Content Management APIs
app.MapPost("/api/content/url", async (UrlContentRequest request, IContentService contentService) =>
{
    var result = await contentService.ProcessUrlContentAsync(request.Url, request.UserId);
    return Results.Ok(result);
})
.WithName("ProcessUrlContent")
.WithSummary("Process content from a URL")
.WithOpenApi();

app.MapPost("/api/content/document", async (IFormFile file, string userId, IContentService contentService) =>
{
    var result = await contentService.ProcessDocumentAsync(file, userId);
    return Results.Ok(result);
})
.WithName("ProcessDocument")
.WithSummary("Process uploaded document")
.WithOpenApi();

app.MapPost("/api/content/text", async (TextContentRequest request, IContentService contentService) =>
{
    var result = await contentService.ProcessTextContentAsync(request.Content, request.UserId);
    return Results.Ok(result);
})
.WithName("ProcessTextContent")
.WithSummary("Process text content directly")
.WithOpenApi();

// Social Media Generation APIs
app.MapPost("/api/social/generate", async (GenerateSocialContentRequest request, ISocialMediaService socialMediaService) =>
{
    var result = await socialMediaService.GenerateContentForPlatformsAsync(request);
    return Results.Ok(result);
})
.WithName("GenerateSocialContent")
.WithSummary("Generate optimized content for social media platforms")
.WithOpenApi();

app.MapPost("/api/social/hashtags", async (HashtagRequest request, ISocialMediaService socialMediaService) =>
{
    var result = await socialMediaService.GenerateHashtagsAsync(request.Content, request.Platform);
    return Results.Ok(result);
})
.WithName("GenerateHashtags")
.WithSummary("Generate trending hashtags for content")
.WithOpenApi();

// Content Management APIs
app.MapGet("/api/content/user/{userId}", async (string userId, IContentService contentService) =>
{
    var result = await contentService.GetUserContentAsync(userId);
    return Results.Ok(result);
})
.WithName("GetUserContent")
.WithSummary("Get all content for a user")
.WithOpenApi();

app.MapGet("/api/content/{id}", async (int id, IContentService contentService) =>
{
    var result = await contentService.GetContentByIdAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetContentById")
.WithSummary("Get content by ID")
.WithOpenApi();

app.MapDelete("/api/content/{id}", async (int id, IContentService contentService) =>
{
    var result = await contentService.DeleteContentAsync(id);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteContent")
.WithSummary("Delete content")
.WithOpenApi();

// Brand Voice APIs
app.MapPost("/api/brand/analyze", async (BrandAnalysisRequest request, IBrandVoiceService brandVoiceService) =>
{
    var result = await brandVoiceService.AnalyzeBrandVoiceAsync(request.UserId, request.SampleTexts);
    return Results.Ok(result);
})
.WithName("AnalyzeBrandVoice")
.WithSummary("Analyze brand voice from sample texts")
.WithOpenApi();

app.MapGet("/api/brand/{userId}", async (string userId, IBrandVoiceService brandVoiceService) =>
{
    var result = await brandVoiceService.GetBrandVoiceAsync(userId);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetBrandVoice")
.WithSummary("Get brand voice for user")
.WithOpenApi();

app.Run();

// Request/Response models
public record UrlContentRequest(string Url, string UserId);
public record TextContentRequest(string Content, string UserId);
public record GenerateSocialContentRequest(int ContentId, string[] Platforms, string? CustomInstructions = null);
public record HashtagRequest(string Content, string Platform);
public record BrandAnalysisRequest(string UserId, string[] SampleTexts);
