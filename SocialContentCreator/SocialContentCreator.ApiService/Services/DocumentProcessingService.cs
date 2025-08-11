using System.Text;

namespace SocialContentCreator.ApiService.Services;

public interface IDocumentProcessingService
{
    Task<string> ExtractTextFromDocumentAsync(IFormFile file);
}

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(ILogger<DocumentProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextFromDocumentAsync(IFormFile file)
    {
        _logger.LogInformation("Extracting text from document: {FileName}", file.FileName);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        try
        {
            return extension switch
            {
                ".txt" => await ExtractFromTextFileAsync(file),
                ".pdf" => await ExtractFromPdfAsync(file),
                ".docx" => await ExtractFromWordDocumentAsync(file),
                ".doc" => await ExtractFromWordDocumentAsync(file),
                ".html" => await ExtractFromHtmlAsync(file),
                ".htm" => await ExtractFromHtmlAsync(file),
                _ => throw new NotSupportedException($"File type {extension} is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from document: {FileName}", file.FileName);
            throw;
        }
    }

    private async Task<string> ExtractFromTextFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> ExtractFromPdfAsync(IFormFile file)
    {
        // For now, return a placeholder. In a real implementation, you would use a PDF library
        // like iText7 or PdfPig to extract text from PDF files
        _logger.LogWarning("PDF extraction not implemented. Returning placeholder text.");
        
        return $"[PDF Document: {file.FileName}]\n" +
               "PDF text extraction is not yet implemented. " +
               "Please convert your PDF to text format or use the text input option.";
    }

    private async Task<string> ExtractFromWordDocumentAsync(IFormFile file)
    {
        // For now, return a placeholder. In a real implementation, you would use a library
        // like DocumentFormat.OpenXml or NPOI to extract text from Word documents
        _logger.LogWarning("Word document extraction not implemented. Returning placeholder text.");
        
        return $"[Word Document: {file.FileName}]\n" +
               "Word document text extraction is not yet implemented. " +
               "Please convert your document to text format or use the text input option.";
    }

    private async Task<string> ExtractFromHtmlAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var htmlContent = await reader.ReadToEndAsync();

        // Basic HTML tag removal - in a real implementation, you might want to use HtmlAgilityPack
        var textContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);
        
        // Clean up whitespace
        textContent = System.Text.RegularExpressions.Regex.Replace(textContent, @"\s+", " ");
        
        return textContent.Trim();
    }
}
