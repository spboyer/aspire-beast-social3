namespace SocialContentCreator.Web.Services;

public interface ISocialMediaContentService
{
    Task<List<SocialMediaPostResponse>> GenerateContentForPlatformsAsync(int contentId, string[] platforms, string? customInstructions = null);
    Task<List<SocialMediaPostResponse>> GenerateOptimizedPostsAsync(int contentId, List<string> platforms, string tone, bool includeHashtags, bool includeEmoji);
    Task<List<string>> GenerateHashtagsAsync(string content, string platform);
    string[] GetSupportedPlatforms();
    int GetCharacterLimit(string platform);
    string GetPlatformIcon(string platform);
    string GetPlatformColor(string platform);
}

public class SocialMediaContentService : ISocialMediaContentService
{
    private readonly ApiService _apiService;
    private readonly ILogger<SocialMediaContentService> _logger;

    private readonly Dictionary<string, PlatformInfo> _platformInfo = new()
    {
        ["Twitter"] = new("Twitter", 280, "🐦", "bg-blue-500", "text-white"),
        ["LinkedIn"] = new("LinkedIn", 3000, "💼", "bg-blue-700", "text-white"),
        ["Instagram"] = new("Instagram", 2200, "📷", "bg-gradient-to-br from-purple-500 to-pink-500", "text-white"),
        ["Facebook"] = new("Facebook", 63206, "👥", "bg-blue-600", "text-white"),
        ["TikTok"] = new("TikTok", 150, "🎵", "bg-black", "text-white")
    };

    public SocialMediaContentService(ApiService apiService, ILogger<SocialMediaContentService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public Task<List<SocialMediaPostResponse>> GenerateOptimizedPostsAsync(int contentId, List<string> platforms, string tone, bool includeHashtags, bool includeEmoji)
    {
        _logger.LogInformation("Generating optimized posts for content {ContentId} with tone {Tone}", contentId, tone);
        
        if (contentId <= 0 || platforms == null || platforms.Count == 0)
        {
            _logger.LogWarning("Invalid parameters provided");
            return Task.FromResult(new List<SocialMediaPostResponse>());
        }

        // Validate platforms
        var supportedPlatforms = GetSupportedPlatforms();
        var validPlatforms = platforms.Where(p => supportedPlatforms.Contains(p)).ToList();
        
        if (validPlatforms.Count == 0)
        {
            _logger.LogWarning("No valid platforms provided");
            return Task.FromResult(new List<SocialMediaPostResponse>());
        }

        // For now, return mock data with the specified parameters
        // In a real implementation, this would call the API service with the tone and options
        var result = validPlatforms.Select(platform => new SocialMediaPostResponse
        {
            Id = Random.Shared.Next(1000, 9999),
            Platform = platform,
            Content = GenerateMockContent(platform, tone, includeEmoji),
            Hashtags = includeHashtags ? GenerateMockHashtags(platform) : "",
            CharacterCount = GetMockCharacterCount(platform),
            CreatedAt = DateTime.Now
        }).ToList();
        
        return Task.FromResult(result);
    }

    private string GenerateMockContent(string platform, string tone, bool includeEmoji)
    {
        var emoji = includeEmoji ? GetPlatformEmoji(platform) : "";
        var baseContent = tone switch
        {
            "professional" => "Discover the latest insights and trends in technology that are shaping our digital future.",
            "casual" => "Hey everyone! Just found this amazing article about tech trends - definitely worth a read!",
            "enthusiastic" => "🚀 WOW! This content about technology trends is absolutely mind-blowing! You NEED to see this!",
            "informative" => "Here are the key technology trends you should know about to stay ahead in 2024.",
            "conversational" => "What do you think about these new technology trends? I'd love to hear your thoughts in the comments!",
            _ => "Check out this interesting content about technology trends and innovations."
        };
        
        return $"{baseContent} {emoji}".Trim();
    }

    private string GenerateMockHashtags(string platform)
    {
        return platform switch
        {
            "Twitter" => "#Tech #Innovation #TechTrends #AI #DigitalTransformation",
            "LinkedIn" => "#Technology #Business #Innovation #DigitalStrategy #ProfessionalDevelopment",
            "Instagram" => "#TechLife #Innovation #DigitalWorld #TechTrends #Future",
            "Facebook" => "#Technology #Innovation #TechNews #DigitalLife",
            _ => "#Tech #Innovation #TechTrends"
        };
    }

    private int GetMockCharacterCount(string platform)
    {
        return platform switch
        {
            "Twitter" => Random.Shared.Next(100, 280),
            "LinkedIn" => Random.Shared.Next(300, 1000),
            "Instagram" => Random.Shared.Next(150, 500),
            "Facebook" => Random.Shared.Next(200, 800),
            _ => Random.Shared.Next(100, 300)
        };
    }

    private string GetPlatformEmoji(string platform)
    {
        return platform switch
        {
            "Twitter" => "🐦",
            "LinkedIn" => "💼",
            "Instagram" => "📸",
            "Facebook" => "👥",
            _ => "🚀"
        };
    }

    public async Task<List<SocialMediaPostResponse>> GenerateContentForPlatformsAsync(int contentId, string[] platforms, string? customInstructions = null)
    {
        _logger.LogInformation("Generating content for platforms {Platforms} for content {ContentId}", string.Join(", ", platforms), contentId);
        
        if (contentId <= 0 || platforms == null || platforms.Length == 0)
        {
            _logger.LogWarning("Invalid parameters provided");
            return new List<SocialMediaPostResponse>();
        }

        // Validate platforms
        var supportedPlatforms = GetSupportedPlatforms();
        var validPlatforms = platforms.Where(p => supportedPlatforms.Contains(p)).ToArray();
        
        if (validPlatforms.Length == 0)
        {
            _logger.LogWarning("No valid platforms provided");
            return new List<SocialMediaPostResponse>();
        }

        return await _apiService.GenerateContentForPlatformsAsync(contentId, validPlatforms, customInstructions);
    }

    public async Task<List<string>> GenerateHashtagsAsync(string content, string platform)
    {
        _logger.LogInformation("Generating hashtags for platform {Platform}", platform);
        
        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(platform))
        {
            _logger.LogWarning("Invalid content or platform provided");
            return new List<string>();
        }

        if (!GetSupportedPlatforms().Contains(platform))
        {
            _logger.LogWarning("Unsupported platform: {Platform}", platform);
            return new List<string>();
        }

        return await _apiService.GenerateHashtagsAsync(content, platform);
    }

    public string[] GetSupportedPlatforms()
    {
        return _platformInfo.Keys.ToArray();
    }

    public int GetCharacterLimit(string platform)
    {
        return _platformInfo.TryGetValue(platform, out var info) ? info.CharacterLimit : 280;
    }

    public string GetPlatformIcon(string platform)
    {
        return _platformInfo.TryGetValue(platform, out var info) ? info.Icon : "📱";
    }

    public string GetPlatformColor(string platform)
    {
        return _platformInfo.TryGetValue(platform, out var info) ? info.ColorClass : "bg-gray-500";
    }

    public string GetPlatformTextColor(string platform)
    {
        return _platformInfo.TryGetValue(platform, out var info) ? info.TextColorClass : "text-white";
    }
}

public record PlatformInfo(string Name, int CharacterLimit, string Icon, string ColorClass, string TextColorClass);
