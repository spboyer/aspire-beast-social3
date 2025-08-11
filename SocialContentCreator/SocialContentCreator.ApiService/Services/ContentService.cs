using Microsoft.EntityFrameworkCore;
using SocialContentCreator.ApiService.Data;
using SocialContentCreator.ApiService.Models;

namespace SocialContentCreator.ApiService.Services;

public interface IContentService
{
    Task<ContentResponse> ProcessUrlContentAsync(string url, string userId);
    Task<ContentResponse> ProcessDocumentAsync(IFormFile file, string userId);
    Task<ContentResponse> ProcessTextContentAsync(string content, string userId);
    Task<List<ContentResponse>> GetUserContentAsync(string userId);
    Task<ContentResponse?> GetContentByIdAsync(int id);
    Task<bool> DeleteContentAsync(int id);
}

public class ContentService : IContentService
{
    private readonly SocialContentDbContext _context;
    private readonly IUrlScrapingService _urlScrapingService;
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IAIContentGenerationService _aiService;
    private readonly ILogger<ContentService> _logger;

    public ContentService(
        SocialContentDbContext context,
        IUrlScrapingService urlScrapingService,
        IDocumentProcessingService documentProcessingService,
        IAIContentGenerationService aiService,
        ILogger<ContentService> logger)
    {
        _context = context;
        _urlScrapingService = urlScrapingService;
        _documentProcessingService = documentProcessingService;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<ContentResponse> ProcessUrlContentAsync(string url, string userId)
    {
        _logger.LogInformation("Processing URL content for user {UserId}: {Url}", userId, url);

        try
        {
            var scrapedContent = await _urlScrapingService.ScrapeUrlAsync(url);
            
            var content = new Content
            {
                UserId = userId,
                Title = scrapedContent.Title ?? "Untitled",
                OriginalContent = scrapedContent.Content,
                Source = ContentSource.Url,
                SourceUrl = url,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Analyze content with AI
            var analysis = await _aiService.AnalyzeContentAsync(scrapedContent.Content);
            content.ProcessedContent = analysis.ProcessedContent;
            content.Summary = analysis.Summary;
            content.KeyPoints = analysis.KeyPoints;
            content.Tone = analysis.Tone;
            content.Industry = analysis.Industry;

            _context.Contents.Add(content);
            await _context.SaveChangesAsync();

            return MapToContentResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing URL content for user {UserId}: {Url}", userId, url);
            throw;
        }
    }

    public async Task<ContentResponse> ProcessDocumentAsync(IFormFile file, string userId)
    {
        _logger.LogInformation("Processing document for user {UserId}: {FileName}", userId, file.FileName);

        try
        {
            var extractedContent = await _documentProcessingService.ExtractTextFromDocumentAsync(file);
            
            var content = new Content
            {
                UserId = userId,
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                OriginalContent = extractedContent,
                Source = ContentSource.Document,
                SourceFileName = file.FileName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Analyze content with AI
            var analysis = await _aiService.AnalyzeContentAsync(extractedContent);
            content.ProcessedContent = analysis.ProcessedContent;
            content.Summary = analysis.Summary;
            content.KeyPoints = analysis.KeyPoints;
            content.Tone = analysis.Tone;
            content.Industry = analysis.Industry;

            _context.Contents.Add(content);
            await _context.SaveChangesAsync();

            return MapToContentResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document for user {UserId}: {FileName}", userId, file.FileName);
            throw;
        }
    }

    public async Task<ContentResponse> ProcessTextContentAsync(string textContent, string userId)
    {
        _logger.LogInformation("Processing text content for user {UserId}", userId);

        try
        {
            var content = new Content
            {
                UserId = userId,
                Title = textContent.Length > 50 ? textContent[..50] + "..." : textContent,
                OriginalContent = textContent,
                Source = ContentSource.Text,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Analyze content with AI
            var analysis = await _aiService.AnalyzeContentAsync(textContent);
            content.ProcessedContent = analysis.ProcessedContent;
            content.Summary = analysis.Summary;
            content.KeyPoints = analysis.KeyPoints;
            content.Tone = analysis.Tone;
            content.Industry = analysis.Industry;

            _context.Contents.Add(content);
            await _context.SaveChangesAsync();

            return MapToContentResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text content for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ContentResponse>> GetUserContentAsync(string userId)
    {
        var contents = await _context.Contents
            .Where(c => c.UserId == userId)
            .Include(c => c.SocialMediaPosts)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return contents.Select(MapToContentResponse).ToList();
    }

    public async Task<ContentResponse?> GetContentByIdAsync(int id)
    {
        var content = await _context.Contents
            .Include(c => c.SocialMediaPosts)
            .FirstOrDefaultAsync(c => c.Id == id);

        return content != null ? MapToContentResponse(content) : null;
    }

    public async Task<bool> DeleteContentAsync(int id)
    {
        var content = await _context.Contents.FindAsync(id);
        if (content == null)
            return false;

        _context.Contents.Remove(content);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ContentResponse MapToContentResponse(Content content)
    {
        return new ContentResponse
        {
            Id = content.Id,
            Title = content.Title,
            Summary = content.Summary ?? string.Empty,
            Source = content.Source.ToString(),
            CreatedAt = content.CreatedAt,
            SocialMediaPosts = content.SocialMediaPosts.Select(smp => new SocialMediaPostResponse
            {
                Id = smp.Id,
                Platform = smp.Platform,
                Content = smp.GeneratedContent,
                Hashtags = smp.Hashtags ?? string.Empty,
                CharacterCount = smp.CharacterCount,
                ImageSuggestion = smp.ImageSuggestion,
                IsPosted = smp.IsPosted,
                CreatedAt = smp.CreatedAt
            }).ToList()
        };
    }
}
