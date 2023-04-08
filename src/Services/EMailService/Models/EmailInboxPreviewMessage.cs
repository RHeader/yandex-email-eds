namespace MailClientApp.Services.EMailService.Models;

public class EmailInboxPreviewMessage
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Title { get; set; } = null!;
    public long  CountAttachments { get; set; }
}