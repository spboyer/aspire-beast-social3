namespace SocialContentCreator.Web.Services;

public interface IBrandVoiceManagementService
{
    Task<BrandVoiceResponse?> AnalyzeBrandVoiceAsync(string userId, string[] sampleTexts);
    Task<BrandVoiceResponse?> GetBrandVoiceAsync(string userId);
    Task<bool> HasBrandVoiceAsync(string userId);
}

public class BrandVoiceManagementService : IBrandVoiceManagementService
{
    private readonly ApiService _apiService;
    private readonly ILogger<BrandVoiceManagementService> _logger;

    public BrandVoiceManagementService(ApiService apiService, ILogger<BrandVoiceManagementService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<BrandVoiceResponse?> AnalyzeBrandVoiceAsync(string userId, string[] sampleTexts)
    {
        _logger.LogInformation("Analyzing brand voice for user {UserId}", userId);
        
        if (string.IsNullOrWhiteSpace(userId) || sampleTexts == null || sampleTexts.Length == 0)
        {
            _logger.LogWarning("Invalid parameters provided for brand voice analysis");
            return null;
        }

        // Validate sample texts
        var validTexts = sampleTexts
            .Where(text => !string.IsNullOrWhiteSpace(text) && text.Length >= 20)
            .ToArray();

        if (validTexts.Length == 0)
        {
            _logger.LogWarning("No valid sample texts provided (minimum 20 characters each)");
            return null;
        }

        return await _apiService.AnalyzeBrandVoiceAsync(userId, validTexts);
    }

    public async Task<BrandVoiceResponse?> GetBrandVoiceAsync(string userId)
    {
        _logger.LogInformation("Getting brand voice for user {UserId}", userId);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Invalid UserId provided");
            return null;
        }

        return await _apiService.GetBrandVoiceAsync(userId);
    }

    public async Task<bool> HasBrandVoiceAsync(string userId)
    {
        _logger.LogInformation("Checking if user {UserId} has brand voice", userId);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var brandVoice = await GetBrandVoiceAsync(userId);
        return brandVoice != null;
    }
}
