using Microsoft.EntityFrameworkCore;
using SocialContentCreator.ApiService.Data;
using SocialContentCreator.ApiService.Models;

namespace SocialContentCreator.ApiService.Services;

public interface IBrandVoiceService
{
    Task<BrandVoice> AnalyzeBrandVoiceAsync(string userId, string[] sampleTexts);
    Task<BrandVoice?> GetBrandVoiceAsync(string userId);
    Task<BrandVoice> UpdateBrandVoiceAsync(string userId, BrandVoiceUpdateRequest request);
    Task<bool> DeleteBrandVoiceAsync(string userId);
}

public class BrandVoiceService : IBrandVoiceService
{
    private readonly SocialContentDbContext _context;
    private readonly ILogger<BrandVoiceService> _logger;

    public BrandVoiceService(SocialContentDbContext context, ILogger<BrandVoiceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BrandVoice> AnalyzeBrandVoiceAsync(string userId, string[] sampleTexts)
    {
        _logger.LogInformation("Analyzing brand voice for user {UserId}", userId);

        try
        {
            var combinedText = string.Join("\n\n", sampleTexts);
            
            // Analyze the sample texts to determine brand voice characteristics
            var analysis = AnalyzeTextCharacteristics(combinedText);

            // Check if brand voice already exists for this user
            var existingBrandVoice = await _context.BrandVoices
                .FirstOrDefaultAsync(bv => bv.UserId == userId);

            if (existingBrandVoice != null)
            {
                // Update existing brand voice
                existingBrandVoice.ToneDescription = analysis.ToneDescription;
                existingBrandVoice.VoiceCharacteristics = analysis.VoiceCharacteristics;
                existingBrandVoice.PreferredLanguage = analysis.PreferredLanguage;
                existingBrandVoice.Industry = analysis.Industry;
                existingBrandVoice.SampleTexts = combinedText;
                existingBrandVoice.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existingBrandVoice;
            }
            else
            {
                // Create new brand voice
                var brandVoice = new BrandVoice
                {
                    UserId = userId,
                    ToneDescription = analysis.ToneDescription,
                    VoiceCharacteristics = analysis.VoiceCharacteristics,
                    PreferredLanguage = analysis.PreferredLanguage,
                    Industry = analysis.Industry,
                    SampleTexts = combinedText,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BrandVoices.Add(brandVoice);
                await _context.SaveChangesAsync();
                return brandVoice;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing brand voice for user {UserId}", userId);
            throw;
        }
    }

    public async Task<BrandVoice?> GetBrandVoiceAsync(string userId)
    {
        return await _context.BrandVoices
            .FirstOrDefaultAsync(bv => bv.UserId == userId);
    }

    public async Task<BrandVoice> UpdateBrandVoiceAsync(string userId, BrandVoiceUpdateRequest request)
    {
        _logger.LogInformation("Updating brand voice for user {UserId}", userId);

        var brandVoice = await _context.BrandVoices
            .FirstOrDefaultAsync(bv => bv.UserId == userId);

        if (brandVoice == null)
        {
            throw new ArgumentException($"Brand voice not found for user {userId}");
        }

        brandVoice.ToneDescription = request.ToneDescription ?? brandVoice.ToneDescription;
        brandVoice.VoiceCharacteristics = request.VoiceCharacteristics ?? brandVoice.VoiceCharacteristics;
        brandVoice.PreferredLanguage = request.PreferredLanguage ?? brandVoice.PreferredLanguage;
        brandVoice.TargetAudience = request.TargetAudience ?? brandVoice.TargetAudience;
        brandVoice.Industry = request.Industry ?? brandVoice.Industry;
        brandVoice.KeyMessages = request.KeyMessages ?? brandVoice.KeyMessages;
        brandVoice.ProhibitedTerms = request.ProhibitedTerms ?? brandVoice.ProhibitedTerms;
        brandVoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return brandVoice;
    }

    public async Task<bool> DeleteBrandVoiceAsync(string userId)
    {
        var brandVoice = await _context.BrandVoices
            .FirstOrDefaultAsync(bv => bv.UserId == userId);

        if (brandVoice == null)
            return false;

        _context.BrandVoices.Remove(brandVoice);
        await _context.SaveChangesAsync();
        return true;
    }

    private BrandVoiceAnalysis AnalyzeTextCharacteristics(string text)
    {
        // Simple analysis - in a real implementation, this would use AI/ML
        var analysis = new BrandVoiceAnalysis();
        
        var textLower = text.ToLower();
        var sentences = text.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Analyze tone
        if (textLower.Contains("exciting") || textLower.Contains("amazing") || textLower.Contains("incredible"))
        {
            analysis.ToneDescription = "Enthusiastic and energetic";
        }
        else if (textLower.Contains("professional") || textLower.Contains("expertise") || textLower.Contains("solution"))
        {
            analysis.ToneDescription = "Professional and authoritative";
        }
        else if (textLower.Contains("friendly") || textLower.Contains("welcome") || textLower.Contains("help"))
        {
            analysis.ToneDescription = "Friendly and approachable";
        }
        else if (textLower.Contains("innovative") || textLower.Contains("cutting-edge") || textLower.Contains("technology"))
        {
            analysis.ToneDescription = "Innovative and forward-thinking";
        }
        else
        {
            analysis.ToneDescription = "Balanced and informative";
        }

        // Analyze voice characteristics
        var characteristics = new List<string>();
        
        var avgSentenceLength = sentences.Length > 0 ? words.Length / sentences.Length : 0;
        if (avgSentenceLength < 10)
            characteristics.Add("Concise");
        else if (avgSentenceLength > 20)
            characteristics.Add("Detailed");
        else
            characteristics.Add("Moderate length");

        if (text.Contains("!"))
            characteristics.Add("Enthusiastic");
        
        if (textLower.Contains("you") || textLower.Contains("your"))
            characteristics.Add("Direct address");
        
        if (textLower.Contains("we") || textLower.Contains("our") || textLower.Contains("us"))
            characteristics.Add("Inclusive");

        analysis.VoiceCharacteristics = string.Join(", ", characteristics);

        // Detect language (simple check)
        analysis.PreferredLanguage = "English";

        // Analyze industry (basic keyword detection)
        if (textLower.Contains("technology") || textLower.Contains("software") || textLower.Contains("digital"))
            analysis.Industry = "Technology";
        else if (textLower.Contains("health") || textLower.Contains("medical") || textLower.Contains("wellness"))
            analysis.Industry = "Healthcare";
        else if (textLower.Contains("finance") || textLower.Contains("investment") || textLower.Contains("banking"))
            analysis.Industry = "Finance";
        else if (textLower.Contains("education") || textLower.Contains("learning") || textLower.Contains("training"))
            analysis.Industry = "Education";
        else if (textLower.Contains("marketing") || textLower.Contains("advertising") || textLower.Contains("brand"))
            analysis.Industry = "Marketing";
        else
            analysis.Industry = "General";

        return analysis;
    }
}

public class BrandVoiceAnalysis
{
    public string ToneDescription { get; set; } = string.Empty;
    public string VoiceCharacteristics { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
}

public class BrandVoiceUpdateRequest
{
    public string? ToneDescription { get; set; }
    public string? VoiceCharacteristics { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TargetAudience { get; set; }
    public string? Industry { get; set; }
    public string? KeyMessages { get; set; }
    public string? ProhibitedTerms { get; set; }
}
