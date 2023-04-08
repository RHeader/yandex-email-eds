namespace MailClientApp.Models.Requests;

public class SendEmailRequest
{
    public List<string> To { get; set; } = new();
    public string DisplayName { get; set; } = null!;
    public string Subject { get; set; } = null!;
    
    public bool SignBody { get; set; }
    public string? Body { get; set; }
    public IFormFile? SignatureCertificate { get; set; }
    public IFormFileCollection? Attachments { get; set; } 
}