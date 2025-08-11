using System.Net.Http.Json;
using System.Text.Json;

namespace SocialContentCreator.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Content Management
    public async Task<ContentResponse?> ProcessUrlContentAsync(string url, string userId)
    {
        try
        {
            var request = new { Url = url, UserId = userId };
            var response = await _httpClient.PostAsJsonAsync("/api/content/url", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ContentResponse>();
            }
            
            _logger.LogWarning("Failed to process URL content: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing URL content");
            return null;
        }
    }

    public async Task<ContentResponse?> ProcessTextContentAsync(string content, string userId)
    {
        try
        {
            var request = new { Content = content, UserId = userId };
            var response = await _httpClient.PostAsJsonAsync("/api/content/text", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ContentResponse>();
            }
            
            _logger.LogWarning("Failed to process text content: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text content");
            return null;
        }
    }

    public async Task<List<ContentResponse>> GetUserContentAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/content/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ContentResponse>>() ?? new List<ContentResponse>();
            }
            
            _logger.LogWarning("Failed to get user content: {StatusCode}", response.StatusCode);
            return new List<ContentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user content");
            return new List<ContentResponse>();
        }
    }

    // Social Media Generation
    public async Task<List<SocialMediaPostResponse>> GenerateContentForPlatformsAsync(int contentId, string[] platforms, string? customInstructions = null)
    {
        try
        {
            var request = new { ContentId = contentId, Platforms = platforms, CustomInstructions = customInstructions };
            var response = await _httpClient.PostAsJsonAsync("/api/social/generate", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SocialMediaPostResponse>>() ?? new List<SocialMediaPostResponse>();
            }
            
            _logger.LogWarning("Failed to generate social content: {StatusCode}", response.StatusCode);
            return new List<SocialMediaPostResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating social content");
            return new List<SocialMediaPostResponse>();
        }
    }

    public async Task<List<string>> GenerateHashtagsAsync(string content, string platform)
    {
        try
        {
            var request = new { Content = content, Platform = platform };
            var response = await _httpClient.PostAsJsonAsync("/api/social/hashtags", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
            }
            
            _logger.LogWarning("Failed to generate hashtags: {StatusCode}", response.StatusCode);
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hashtags");
            return new List<string>();
        }
    }

    // Brand Voice Management
    public async Task<BrandVoiceResponse?> AnalyzeBrandVoiceAsync(string userId, string[] sampleTexts)
    {
        try
        {
            var request = new { UserId = userId, SampleTexts = sampleTexts };
            var response = await _httpClient.PostAsJsonAsync("/api/brand/analyze", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BrandVoiceResponse>();
            }
            
            _logger.LogWarning("Failed to analyze brand voice: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing brand voice");
            return null;
        }
    }

    public async Task<BrandVoiceResponse?> GetBrandVoiceAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/brand/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BrandVoiceResponse>();
            }
            
            _logger.LogWarning("Failed to get brand voice: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand voice");
            return null;
        }
    }
}

// Response Models
public class ContentResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SocialMediaPostResponse> SocialMediaPosts { get; set; } = new();
}

public class SocialMediaPostResponse
{
    public int Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Hashtags { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public string? ImageSuggestion { get; set; }
    public bool IsPosted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BrandVoiceResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? ToneDescription { get; set; }
    public string? VoiceCharacteristics { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TargetAudience { get; set; }
    public string? Industry { get; set; }
    public string? KeyMessages { get; set; }
    public string? ProhibitedTerms { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
