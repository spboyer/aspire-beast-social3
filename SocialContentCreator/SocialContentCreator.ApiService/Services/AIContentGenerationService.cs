using Azure.AI.OpenAI;
using System.Text.Json;

namespace SocialContentCreator.ApiService.Services;

public interface IAIContentGenerationService
{
    Task<ContentAnalysis> AnalyzeContentAsync(string content);
    Task<SocialMediaContentResult> GenerateContentForPlatformAsync(string sourceContent, string platform, string? customInstructions = null, string? brandVoice = null);
    Task<List<string>> GenerateHashtagsAsync(string content, string platform);
    Task<string> GenerateImageSuggestionAsync(string content);
}

public class AIContentGenerationService : IAIContentGenerationService
{
    private readonly ILogger<AIContentGenerationService> _logger;
    private readonly IConfiguration _configuration;

    public AIContentGenerationService(ILogger<AIContentGenerationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ContentAnalysis> AnalyzeContentAsync(string content)
    {
        _logger.LogInformation("Analyzing content with AI");

        try
        {
            // For now, return mock analysis. In a real implementation, this would call Azure OpenAI
            // to analyze the content for tone, key points, industry, etc.
            
            var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var summary = string.Join(" ", words.Take(Math.Min(50, words.Length))) + 
                         (words.Length > 50 ? "..." : "");

            var keyPoints = ExtractKeyPoints(content);
            var tone = AnalyzeTone(content);
            var industry = AnalyzeIndustry(content);

            return new ContentAnalysis
            {
                ProcessedContent = content,
                Summary = summary,
                KeyPoints = string.Join("; ", keyPoints),
                Tone = tone,
                Industry = industry
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing content with AI");
            throw;
        }
    }

    public async Task<SocialMediaContentResult> GenerateContentForPlatformAsync(
        string sourceContent, 
        string platform, 
        string? customInstructions = null, 
        string? brandVoice = null)
    {
        _logger.LogInformation("Generating {Platform} content", platform);

        try
        {
            var platformConfig = GetPlatformConfiguration(platform);
            var generatedContent = await GenerateOptimizedContent(sourceContent, platformConfig, customInstructions, brandVoice);
            var hashtags = await GenerateHashtagsAsync(sourceContent, platform);
            var imageSuggestion = await GenerateImageSuggestionAsync(sourceContent);

            return new SocialMediaContentResult
            {
                Platform = platform,
                Content = generatedContent,
                Hashtags = hashtags,
                CharacterCount = generatedContent.Length,
                ImageSuggestion = imageSuggestion,
                CallToAction = GenerateCallToAction(platform, sourceContent)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {Platform} content", platform);
            throw;
        }
    }

    public async Task<List<string>> GenerateHashtagsAsync(string content, string platform)
    {
        _logger.LogInformation("Generating hashtags for {Platform}", platform);

        // Mock implementation - in reality, this would use AI to generate relevant hashtags
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 4)
            .Take(5)
            .Select(w => "#" + w.Replace(",", "").Replace(".", "").Replace("!", "").Replace("?", ""))
            .ToList();

        // Add platform-specific hashtags
        words.AddRange(platform.ToLower() switch
        {
            "twitter" => new[] { "#ContentMarketing", "#SocialMedia", "#Engagement" },
            "linkedin" => new[] { "#ProfessionalDevelopment", "#Business", "#Networking" },
            "instagram" => new[] { "#Visual", "#Inspiration", "#Community" },
            "facebook" => new[] { "#Community", "#Sharing", "#Connection" },
            "tiktok" => new[] { "#Trending", "#Creative", "#ForYou" },
            _ => new[] { "#Content", "#Social", "#Marketing" }
        });

        return words.Take(10).ToList();
    }

    public async Task<string> GenerateImageSuggestionAsync(string content)
    {
        _logger.LogInformation("Generating image suggestion");

        // Mock implementation - in reality, this would use AI to suggest relevant images
        var suggestions = new[]
        {
            "Professional business setting with modern technology",
            "Inspiring quote overlay on aesthetic background",
            "Infographic with key statistics and data points",
            "Behind-the-scenes workspace or team collaboration",
            "Product showcase with clean, minimal styling",
            "Lifestyle image that resonates with target audience",
            "Abstract design that complements the content theme"
        };

        return suggestions[Random.Shared.Next(suggestions.Length)];
    }

    private async Task<string> GenerateOptimizedContent(
        string sourceContent, 
        PlatformConfiguration config, 
        string? customInstructions, 
        string? brandVoice)
    {
        // Mock implementation - in reality, this would use Azure OpenAI
        var words = sourceContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var targetWords = Math.Min(config.MaxCharacters / 5, words.Length); // Rough estimate
        
        var optimizedContent = string.Join(" ", words.Take(targetWords));
        
        // Ensure it fits within character limits
        if (optimizedContent.Length > config.MaxCharacters)
        {
            optimizedContent = optimizedContent[..(config.MaxCharacters - 3)] + "...";
        }

        return optimizedContent;
    }

    private PlatformConfiguration GetPlatformConfiguration(string platform)
    {
        return platform.ToLower() switch
        {
            "twitter" => new PlatformConfiguration { MaxCharacters = 280, Style = "concise, engaging" },
            "linkedin" => new PlatformConfiguration { MaxCharacters = 3000, Style = "professional, informative" },
            "instagram" => new PlatformConfiguration { MaxCharacters = 2200, Style = "visual, inspiring" },
            "facebook" => new PlatformConfiguration { MaxCharacters = 63206, Style = "conversational, community-focused" },
            "tiktok" => new PlatformConfiguration { MaxCharacters = 150, Style = "fun, creative, trending" },
            _ => new PlatformConfiguration { MaxCharacters = 280, Style = "general" }
        };
    }

    private List<string> ExtractKeyPoints(string content)
    {
        // Simple implementation - in reality, this would use NLP/AI
        var sentences = content.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return sentences
            .Where(s => s.Trim().Length > 20)
            .Take(3)
            .Select(s => s.Trim())
            .ToList();
    }

    private string AnalyzeTone(string content)
    {
        // Simple keyword-based tone analysis - in reality, this would use AI
        var contentLower = content.ToLower();
        
        if (contentLower.Contains("exciting") || contentLower.Contains("amazing") || contentLower.Contains("incredible"))
            return "Enthusiastic";
        else if (contentLower.Contains("professional") || contentLower.Contains("business") || contentLower.Contains("strategy"))
            return "Professional";
        else if (contentLower.Contains("challenge") || contentLower.Contains("problem") || contentLower.Contains("issue"))
            return "Problem-solving";
        else if (contentLower.Contains("fun") || contentLower.Contains("enjoy") || contentLower.Contains("celebrate"))
            return "Playful";
        else
            return "Informative";
    }

    private string AnalyzeIndustry(string content)
    {
        // Simple keyword-based industry analysis - in reality, this would use AI
        var contentLower = content.ToLower();
        
        if (contentLower.Contains("technology") || contentLower.Contains("software") || contentLower.Contains("ai"))
            return "Technology";
        else if (contentLower.Contains("marketing") || contentLower.Contains("brand") || contentLower.Contains("advertising"))
            return "Marketing";
        else if (contentLower.Contains("health") || contentLower.Contains("medical") || contentLower.Contains("wellness"))
            return "Healthcare";
        else if (contentLower.Contains("finance") || contentLower.Contains("investment") || contentLower.Contains("money"))
            return "Finance";
        else if (contentLower.Contains("education") || contentLower.Contains("learning") || contentLower.Contains("training"))
            return "Education";
        else
            return "General";
    }

    private string GenerateCallToAction(string platform, string content)
    {
        var ctas = platform.ToLower() switch
        {
            "twitter" => new[] { "What do you think?", "Share your thoughts!", "Join the conversation", "Retweet if you agree" },
            "linkedin" => new[] { "What's your experience?", "I'd love to hear your thoughts", "Connect with me to discuss further", "Share your insights" },
            "instagram" => new[] { "Double-tap if you agree!", "Save this for later", "Tag a friend who needs to see this", "What's your take?" },
            "facebook" => new[] { "What do you think about this?", "Share with your network", "Comment below with your thoughts", "React if this resonates" },
            "tiktok" => new[] { "Duet with your response", "Try this trend", "Follow for more tips", "Comment your thoughts" },
            _ => new[] { "Let us know what you think!", "Share your thoughts", "Join the discussion" }
        };

        return ctas[Random.Shared.Next(ctas.Length)];
    }
}

public class ContentAnalysis
{
    public string ProcessedContent { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string KeyPoints { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
}

public class SocialMediaContentResult
{
    public string Platform { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
    public int CharacterCount { get; set; }
    public string ImageSuggestion { get; set; } = string.Empty;
    public string CallToAction { get; set; } = string.Empty;
}

public class PlatformConfiguration
{
    public int MaxCharacters { get; set; }
    public string Style { get; set; } = string.Empty;
}
