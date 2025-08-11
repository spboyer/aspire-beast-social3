using System.ComponentModel.DataAnnotations;

namespace SocialContentCreator.ApiService.Models;

public class Content
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string OriginalContent { get; set; } = string.Empty;
    
    public string? ProcessedContent { get; set; }
    
    public string? Summary { get; set; }
    
    public string? KeyPoints { get; set; }
    
    public string? Tone { get; set; }
    
    public string? Industry { get; set; }
    
    public ContentSource Source { get; set; }
    
    public string? SourceUrl { get; set; }
    
    public string? SourceFileName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public List<SocialMediaPost> SocialMediaPosts { get; set; } = new();
    
    public List<ContentTag> Tags { get; set; } = new();
}

public class SocialMediaPost
{
    public int Id { get; set; }
    
    public int ContentId { get; set; }
    
    public Content Content { get; set; } = null!;
    
    [Required]
    public string Platform { get; set; } = string.Empty; // Twitter, LinkedIn, Instagram, Facebook, TikTok
    
    [Required]
    public string GeneratedContent { get; set; } = string.Empty;
    
    public string? Hashtags { get; set; }
    
    public int CharacterCount { get; set; }
    
    public string? ImageSuggestion { get; set; }
    
    public string? CallToAction { get; set; }
    
    public DateTime? ScheduledAt { get; set; }
    
    public bool IsPosted { get; set; } = false;
    
    public DateTime? PostedAt { get; set; }
    
    public string? PostUrl { get; set; }
    
    public SocialMediaPostMetrics? Metrics { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SocialMediaPostMetrics
{
    public int Id { get; set; }
    
    public int SocialMediaPostId { get; set; }
    
    public SocialMediaPost SocialMediaPost { get; set; } = null!;
    
    public int Views { get; set; }
    
    public int Likes { get; set; }
    
    public int Shares { get; set; }
    
    public int Comments { get; set; }
    
    public int Clicks { get; set; }
    
    public double EngagementRate { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ContentTag
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Category { get; set; }
    
    public List<Content> Contents { get; set; } = new();
}

public class BrandVoice
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? ToneDescription { get; set; }
    
    public string? VoiceCharacteristics { get; set; }
    
    public string? PreferredLanguage { get; set; }
    
    public string? TargetAudience { get; set; }
    
    public string? Industry { get; set; }
    
    public string? KeyMessages { get; set; }
    
    public string? ProhibitedTerms { get; set; }
    
    public string? SampleTexts { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ContentCalendar
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public int SocialMediaPostId { get; set; }
    
    public SocialMediaPost SocialMediaPost { get; set; } = null!;
    
    public DateTime ScheduledDateTime { get; set; }
    
    public string? TimeZone { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    public string? RecurrencePattern { get; set; }
    
    public CalendarStatus Status { get; set; } = CalendarStatus.Scheduled;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ContentSource
{
    Url,
    Document,
    Text,
    Image
}

public enum CalendarStatus
{
    Scheduled,
    Posted,
    Failed,
    Cancelled
}

// Request/Response DTOs
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
