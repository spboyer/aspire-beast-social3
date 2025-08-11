namespace SocialContentCreator.Web.Services;

public interface IContentManagementService
{
    Task<ContentResponse?> ProcessUrlAsync(string url, string userId);
    Task<ContentResponse?> ProcessTextAsync(string content, string userId);
    Task<List<ContentResponse>> GetUserContentAsync(string userId);
    Task<List<ContentResponse>> GetRecentContentAsync(int count);
    Task<bool> DeleteContentAsync(int contentId);
}

public class ContentManagementService : IContentManagementService
{
    private readonly ApiService _apiService;
    private readonly ILogger<ContentManagementService> _logger;

    public ContentManagementService(ApiService apiService, ILogger<ContentManagementService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<ContentResponse?> ProcessUrlAsync(string url, string userId)
    {
        _logger.LogInformation("Processing URL for user {UserId}: {Url}", userId, url);
        
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Invalid URL or UserId provided");
            return null;
        }

        // Validate URL format
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            _logger.LogWarning("Invalid URL format: {Url}", url);
            return null;
        }

        return await _apiService.ProcessUrlContentAsync(url, userId);
    }

    public async Task<ContentResponse?> ProcessTextAsync(string content, string userId)
    {
        _logger.LogInformation("Processing text content for user {UserId}", userId);
        
        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Invalid content or UserId provided");
            return null;
        }

        // Validate content length
        if (content.Length < 10)
        {
            _logger.LogWarning("Content too short: {Length} characters", content.Length);
            return null;
        }

        return await _apiService.ProcessTextContentAsync(content, userId);
    }

    public async Task<List<ContentResponse>> GetUserContentAsync(string userId)
    {
        _logger.LogInformation("Getting content for user {UserId}", userId);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Invalid UserId provided");
            return new List<ContentResponse>();
        }

        return await _apiService.GetUserContentAsync(userId);
    }

    public Task<List<ContentResponse>> GetRecentContentAsync(int count)
    {
        _logger.LogInformation("Getting recent content, count: {Count}", count);
        
        if (count <= 0)
        {
            _logger.LogWarning("Invalid count provided: {Count}", count);
            return Task.FromResult(new List<ContentResponse>());
        }

        try
        {
            // For now, return mock data since we don't have a specific recent content endpoint
            // In a real implementation, this would call an API endpoint that orders by CreatedAt descending
            var mockData = new List<ContentResponse>
            {
                new ContentResponse
                {
                    Id = 1,
                    Title = "AI Trends in 2024",
                    Summary = "Latest developments in artificial intelligence and machine learning.",
                    Source = "Tech Blog",
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new ContentResponse
                {
                    Id = 2,
                    Title = "Social Media Marketing Best Practices",
                    Summary = "How to optimize your social media presence for maximum engagement.",
                    Source = "Marketing Article",
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new ContentResponse
                {
                    Id = 3,
                    Title = "Remote Work Productivity Tips",
                    Summary = "Strategies for maintaining productivity while working from home.",
                    Source = "Business Guide",
                    CreatedAt = DateTime.Now.AddDays(-3)
                }
            }.Take(count).ToList();
            
            return Task.FromResult(mockData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent content");
            return Task.FromResult(new List<ContentResponse>());
        }
    }

    public Task<bool> DeleteContentAsync(int contentId)
    {
        _logger.LogInformation("Deleting content {ContentId}", contentId);
        
        if (contentId <= 0)
        {
            _logger.LogWarning("Invalid ContentId provided: {ContentId}", contentId);
            return Task.FromResult(false);
        }

        // Note: This would need to be implemented in the API service
        // For now, return false as placeholder
        _logger.LogWarning("Delete functionality not yet implemented in API service");
        return Task.FromResult(false);
    }
}
