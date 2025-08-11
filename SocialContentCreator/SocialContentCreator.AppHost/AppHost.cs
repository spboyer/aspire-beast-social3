using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Azure infrastructure services
var storage = builder.AddAzureStorage("storage");
var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

var sqlServer = builder.AddAzureSqlServer("sql");
var database = sqlServer.AddDatabase("socialdb");

var cache = builder.AddRedis("cache");

// PostgreSQL for analytics and content metadata
var postgres = builder.AddPostgres("postgres");
if (builder.ExecutionContext.IsRunMode)
{
    postgres.WithDataVolume();
    storage.RunAsEmulator();
    cache.WithRedisCommander();
}

var analyticsDb = postgres.AddDatabase("analyticsdb");

// Core API Service - Central hub for content management
var apiService = builder.AddProject<Projects.SocialContentCreator_ApiService>("api")
    .WithReference(database)
    .WithReference(cache)
    .WithReference(blobs);

// Content Processing Service - Handles URL scraping, document processing, AI generation
var contentProcessor = builder.AddProject<Projects.SocialContentCreator_ContentProcessor>("content-processor")
    .WithReference(database)
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(apiService)
    .WaitFor(apiService);

// Platform Integration Service - Social media platform APIs
var platformIntegration = builder.AddProject<Projects.SocialContentCreator_PlatformIntegration>("platform-integration")
    .WithReference(database)
    .WithReference(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

// Analytics Service - Performance tracking and insights
var analyticsService = builder.AddProject<Projects.SocialContentCreator_AnalyticsService>("analytics")
    .WithReference(analyticsDb)
    .WithReference(cache)
    .WithReference(apiService)
    .WaitFor(postgres);

// Main Web Application - Blazor Server with ShadCN/Tailwind
var webapp = builder.AddProject<Projects.SocialContentCreator_Web>("webapp")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(contentProcessor)
    .WithReference(platformIntegration)
    .WithReference(analyticsService)
    .WithReference(cache)
    .WaitFor(apiService);

// Production scaling configuration
if (builder.ExecutionContext.IsPublishMode)
{
    apiService.WithReplicas(3);
    contentProcessor.WithReplicas(2);
    platformIntegration.WithReplicas(2);
    webapp.WithReplicas(2);
}

builder.Build().Run();
