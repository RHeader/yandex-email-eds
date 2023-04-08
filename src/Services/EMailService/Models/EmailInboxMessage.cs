using MailClientApp.Services.Models;

namespace MailClientApp.Services.EMailService.Models;

public class EmailInboxMessage
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Body { get; set; }
    public List<FileAttachmentModel>? Attachments { get; set; }
}