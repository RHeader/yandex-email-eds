using MailClientApp.Services.EMailService.Models;

namespace MailClientApp.Services.Models;

public class MailData
{
    // Получатели
    public List<string> To { get; }
    public List<string> Bcc { get; }

    public List<string> Cc { get; }

    // Отправитель
    public string? From { get; }

    public string? DisplayName { get; }

    public string? ReplyTo { get; }

    public string? ReplyToName { get; }

    // Часть с сообщением
    public string Subject { get; }
    public string? Body { get; }
    public List<FileAttachmentModel> Attachments { get; }

    public MailData(List<string> to,
        string subject,
        string? body = null,
        string? from = null,
        string? displayName = null,
        string? replyTo = null,
        string? replyToName = null,
        List<string>? bcc = null,
        List<string>? cc = null,
        List<FileAttachmentModel>? attachments = null)
    {
        // Receiver
        To = to;
        Bcc = bcc ?? new List<string>();
        Cc = cc ?? new List<string>();

        // Sender
        From = from;
        DisplayName = displayName;
        ReplyTo = replyTo;
        ReplyToName = replyToName;

        // Content
        Subject = subject;
        Body = body;
        Attachments = attachments ?? new List<FileAttachmentModel>();
    }
}

