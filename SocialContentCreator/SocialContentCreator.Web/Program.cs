using SocialContentCreator.Web.Components;
using SocialContentCreator.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add output caching with Redis
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP clients for API communication
var apiServiceUri = new Uri("https+http://api");
builder.Services.AddHttpClient<ApiService>(client => 
    client.BaseAddress = apiServiceUri);

// Add application services
builder.Services.AddScoped<IContentManagementService, ContentManagementService>();
builder.Services.AddScoped<ISocialMediaContentService, SocialMediaContentService>();
builder.Services.AddScoped<IBrandVoiceManagementService, BrandVoiceManagementService>();

var app = builder.Build();

// Map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseOutputCache();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
