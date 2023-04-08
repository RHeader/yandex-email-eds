namespace MailClientApp.Services.EMailService.Models;

/// <summary>
/// File attachament object for email service
/// </summary>
public class FileAttachmentModel
{
    public byte[] FileObject { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}