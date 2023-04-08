namespace MailClientApp.Models.Responses;

public class EmailMessageResponse
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Body { get; set; }
    public List<AttachmentResponse>? Attachments { get; set; }
}

public class AttachmentResponse
{
    public string Base64File { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}