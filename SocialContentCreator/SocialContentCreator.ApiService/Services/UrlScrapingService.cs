using HtmlAgilityPack;
using System.Text;

namespace SocialContentCreator.ApiService.Services;

public interface IUrlScrapingService
{
    Task<ScrapedContent> ScrapeUrlAsync(string url);
}

public class UrlScrapingService : IUrlScrapingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UrlScrapingService> _logger;

    public UrlScrapingService(HttpClient httpClient, ILogger<UrlScrapingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure HttpClient
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }

    public async Task<ScrapedContent> ScrapeUrlAsync(string url)
    {
        _logger.LogInformation("Scraping URL: {Url}", url);

        try
        {
            var response = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var title = ExtractTitle(doc);
            var content = ExtractMainContent(doc);
            var metaDescription = ExtractMetaDescription(doc);

            return new ScrapedContent
            {
                Title = title,
                Content = content,
                MetaDescription = metaDescription,
                Url = url,
                ScrapedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping URL: {Url}", url);
            throw new InvalidOperationException($"Failed to scrape URL: {url}", ex);
        }
    }

    private string ExtractTitle(HtmlDocument doc)
    {
        // Try multiple strategies to get title
        var titleNode = doc.DocumentNode.SelectSingleNode("//title");
        if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
        {
            return titleNode.InnerText.Trim();
        }

        // Try OpenGraph title
        var ogTitle = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
        if (ogTitle != null)
        {
            return ogTitle.GetAttributeValue("content", "").Trim();
        }

        // Try h1 tag
        var h1 = doc.DocumentNode.SelectSingleNode("//h1");
        if (h1 != null && !string.IsNullOrWhiteSpace(h1.InnerText))
        {
            return h1.InnerText.Trim();
        }

        return "Untitled";
    }

    private string ExtractMetaDescription(HtmlDocument doc)
    {
        var metaDesc = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
        if (metaDesc != null)
        {
            return metaDesc.GetAttributeValue("content", "").Trim();
        }

        var ogDesc = doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
        if (ogDesc != null)
        {
            return ogDesc.GetAttributeValue("content", "").Trim();
        }

        return string.Empty;
    }

    private string ExtractMainContent(HtmlDocument doc)
    {
        var contentBuilder = new StringBuilder();

        // Remove script and style elements
        var scriptsAndStyles = doc.DocumentNode.Descendants()
            .Where(n => n.Name == "script" || n.Name == "style")
            .ToList();

        foreach (var element in scriptsAndStyles)
        {
            element.Remove();
        }

        // Try to find main content areas
        var contentSelectors = new[]
        {
            "//article",
            "//main",
            "//div[contains(@class, 'content')]",
            "//div[contains(@class, 'post')]",
            "//div[contains(@class, 'article')]",
            "//div[contains(@class, 'entry')]",
            "//div[contains(@id, 'content')]",
            "//div[contains(@id, 'main')]"
        };

        foreach (var selector in contentSelectors)
        {
            var contentNodes = doc.DocumentNode.SelectNodes(selector);
            if (contentNodes != null)
            {
                foreach (var node in contentNodes)
                {
                    var text = ExtractTextFromNode(node);
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 100)
                    {
                        contentBuilder.AppendLine(text);
                    }
                }
            }
        }

        // If no main content found, extract from paragraphs
        if (contentBuilder.Length < 100)
        {
            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                foreach (var p in paragraphs)
                {
                    var text = p.InnerText?.Trim();
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 20)
                    {
                        contentBuilder.AppendLine(text);
                    }
                }
            }
        }

        var result = contentBuilder.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? "No content extracted" : result;
    }

    private string ExtractTextFromNode(HtmlNode node)
    {
        var textBuilder = new StringBuilder();
        
        foreach (var descendant in node.DescendantsAndSelf())
        {
            if (descendant.NodeType == HtmlNodeType.Text)
            {
                var text = descendant.InnerText?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    textBuilder.AppendLine(text);
                }
            }
        }

        return textBuilder.ToString().Trim();
    }
}

public class ScrapedContent
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; }
}
