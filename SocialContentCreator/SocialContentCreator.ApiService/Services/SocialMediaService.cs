using Microsoft.EntityFrameworkCore;
using SocialContentCreator.ApiService.Data;
using SocialContentCreator.ApiService.Models;

namespace SocialContentCreator.ApiService.Services;

public interface ISocialMediaService
{
    Task<List<SocialMediaPostResponse>> GenerateContentForPlatformsAsync(GenerateSocialContentRequest request);
    Task<List<string>> GenerateHashtagsAsync(string content, string platform);
    Task<SocialMediaPostResponse> UpdateSocialMediaPostAsync(int postId, string content, string? hashtags);
    Task<bool> SchedulePostAsync(int postId, DateTime scheduledTime);
    Task<List<SocialMediaPostResponse>> GetScheduledPostsAsync(string userId);
}

public class SocialMediaService : ISocialMediaService
{
    private readonly SocialContentDbContext _context;
    private readonly IAIContentGenerationService _aiService;
    private readonly IBrandVoiceService _brandVoiceService;
    private readonly ILogger<SocialMediaService> _logger;

    public SocialMediaService(
        SocialContentDbContext context,
        IAIContentGenerationService aiService,
        IBrandVoiceService brandVoiceService,
        ILogger<SocialMediaService> logger)
    {
        _context = context;
        _aiService = aiService;
        _brandVoiceService = brandVoiceService;
        _logger = logger;
    }

    public async Task<List<SocialMediaPostResponse>> GenerateContentForPlatformsAsync(GenerateSocialContentRequest request)
    {
        _logger.LogInformation("Generating social media content for content ID {ContentId}", request.ContentId);

        var content = await _context.Contents.FindAsync(request.ContentId);
        if (content == null)
        {
            throw new ArgumentException($"Content with ID {request.ContentId} not found");
        }

        var brandVoice = await _brandVoiceService.GetBrandVoiceAsync(content.UserId);
        var responses = new List<SocialMediaPostResponse>();

        foreach (var platform in request.Platforms)
        {
            try
            {
                var result = await _aiService.GenerateContentForPlatformAsync(
                    content.ProcessedContent ?? content.OriginalContent,
                    platform,
                    request.CustomInstructions,
                    brandVoice?.ToneDescription);

                var socialMediaPost = new SocialMediaPost
                {
                    ContentId = request.ContentId,
                    Platform = platform,
                    GeneratedContent = result.Content,
                    Hashtags = string.Join(" ", result.Hashtags),
                    CharacterCount = result.CharacterCount,
                    ImageSuggestion = result.ImageSuggestion,
                    CallToAction = result.CallToAction,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SocialMediaPosts.Add(socialMediaPost);
                await _context.SaveChangesAsync();

                responses.Add(new SocialMediaPostResponse
                {
                    Id = socialMediaPost.Id,
                    Platform = socialMediaPost.Platform,
                    Content = socialMediaPost.GeneratedContent,
                    Hashtags = socialMediaPost.Hashtags ?? string.Empty,
                    CharacterCount = socialMediaPost.CharacterCount,
                    ImageSuggestion = socialMediaPost.ImageSuggestion,
                    IsPosted = socialMediaPost.IsPosted,
                    CreatedAt = socialMediaPost.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content for platform {Platform}", platform);
                // Continue with other platforms even if one fails
            }
        }

        return responses;
    }

    public async Task<List<string>> GenerateHashtagsAsync(string content, string platform)
    {
        return await _aiService.GenerateHashtagsAsync(content, platform);
    }

    public async Task<SocialMediaPostResponse> UpdateSocialMediaPostAsync(int postId, string content, string? hashtags)
    {
        _logger.LogInformation("Updating social media post {PostId}", postId);

        var post = await _context.SocialMediaPosts.FindAsync(postId);
        if (post == null)
        {
            throw new ArgumentException($"Social media post with ID {postId} not found");
        }

        post.GeneratedContent = content;
        post.Hashtags = hashtags;
        post.CharacterCount = content.Length;

        await _context.SaveChangesAsync();

        return new SocialMediaPostResponse
        {
            Id = post.Id,
            Platform = post.Platform,
            Content = post.GeneratedContent,
            Hashtags = post.Hashtags ?? string.Empty,
            CharacterCount = post.CharacterCount,
            ImageSuggestion = post.ImageSuggestion,
            IsPosted = post.IsPosted,
            CreatedAt = post.CreatedAt
        };
    }

    public async Task<bool> SchedulePostAsync(int postId, DateTime scheduledTime)
    {
        _logger.LogInformation("Scheduling post {PostId} for {ScheduledTime}", postId, scheduledTime);

        var post = await _context.SocialMediaPosts.FindAsync(postId);
        if (post == null)
        {
            return false;
        }

        post.ScheduledAt = scheduledTime;
        
        // Create calendar entry
        var calendarEntry = new ContentCalendar
        {
            UserId = post.Content.UserId,
            SocialMediaPostId = postId,
            ScheduledDateTime = scheduledTime,
            Status = CalendarStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContentCalendars.Add(calendarEntry);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<SocialMediaPostResponse>> GetScheduledPostsAsync(string userId)
    {
        var scheduledPosts = await _context.SocialMediaPosts
            .Include(p => p.Content)
            .Where(p => p.Content.UserId == userId && p.ScheduledAt.HasValue && !p.IsPosted)
            .OrderBy(p => p.ScheduledAt)
            .ToListAsync();

        return scheduledPosts.Select(post => new SocialMediaPostResponse
        {
            Id = post.Id,
            Platform = post.Platform,
            Content = post.GeneratedContent,
            Hashtags = post.Hashtags ?? string.Empty,
            CharacterCount = post.CharacterCount,
            ImageSuggestion = post.ImageSuggestion,
            IsPosted = post.IsPosted,
            CreatedAt = post.CreatedAt
        }).ToList();
    }
}
