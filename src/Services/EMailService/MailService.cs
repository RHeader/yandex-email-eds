using System.Security.Claims;
using MailClientApp.Services.EMailService.Models;
using MailClientApp.Services.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailClientApp.Services.EMailService;

public class MailService : IMailWorkerService
{
    private readonly ILogger<MailService> Logger;
    private readonly EmailOptions _emailOptions;

    private ImapClient _imapClient;
    private SmtpClient _smtpClient;

    private string _currentEmail;

    public MailService(IOptions<EmailOptions> emailOptions,
        ILogger<MailService> logger
    )
    {
        Logger = logger;
        _emailOptions = emailOptions.Value;
    }


    public async Task<IMailWorkerService> ConnectAsync(Protocol protocol, CancellationToken token)
    {
        if (protocol is Protocol.Imap)
        {
            _imapClient = new ImapClient();

            if (_emailOptions.SslEnable)
            {
                await _imapClient.ConnectAsync(_emailOptions.Imap.Dns, _emailOptions.Imap.Port,
                    SecureSocketOptions.SslOnConnect, token);
            }
            else
            {
                await _imapClient.ConnectAsync(_emailOptions.Imap.Dns, _emailOptions.Imap.Port,
                    SecureSocketOptions.StartTls,
                    token);
            }
        }

        if (protocol is Protocol.Smtp)
        {
            _smtpClient = new SmtpClient();
            if (_emailOptions.SslEnable)
            {
                await _smtpClient.ConnectAsync(_emailOptions.Smtp.Dns, _emailOptions.Smtp.Port,
                    SecureSocketOptions.Auto, token);
            }
            else
            {
                await _smtpClient.ConnectAsync(_emailOptions.Smtp.Dns, _emailOptions.Smtp.Port,
                    SecureSocketOptions.StartTls,
                    token);
            }
        }


        return this;
    }


    public async Task<EmailInboxMessage> GetMessageById(uint id)
    {
        await _imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly);
        
        var message = await _imapClient.Inbox.GetMessageAsync(new UniqueId(id));
        
        return new EmailInboxMessage()
        {
            Id = id.ToString(),
            Title = message.Subject,
            Body = message.HtmlBody,
            From = message.From.Mailboxes.First().Address,
            Attachments = message.Attachments.Select(x =>
            {
                var file = new FileAttachmentModel();
                file.ContentType = x.ContentType.MimeType;
                file.FileName = x.ContentDisposition?.FileName ?? x.ContentType.Name;
                using (var stream = new MemoryStream())
                {
                    x.WriteToAsync(stream);
                    file.FileObject = stream.ToArray();
                }
                return file;
            }).ToList()
            
        };
    }

    public async Task<List<EmailInboxPreviewMessage>> GetInboxMessages(SearchQuery query)
    {
        var inbox = _imapClient.Inbox;
         await inbox.OpenAsync(FolderAccess.ReadOnly);

         var messageCount = inbox.Count;
         var startIndex = Math.Max(0, messageCount - 10);
         
        var uids = await inbox.FetchAsync(startIndex, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope);      
        List<EmailInboxPreviewMessage> messages = new();
        foreach (var uid in uids.OrderByDescending(x=>x.Index))
        {
            var message = await _imapClient.Inbox.GetMessageAsync(uid.UniqueId);
            
            messages.Add(new EmailInboxPreviewMessage()
            {
                Id = uid.UniqueId.ToString(),
                From = message.From.Mailboxes.First().Address,
                Title = message.Subject,
                CountAttachments = message.Attachments.Count()
            });
        }

        return messages;
    }

    public async Task<bool> ValidatePassword(string email, string password, CancellationToken token)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email token missing");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(email), "Password token missing");
            }

            await _imapClient.AuthenticateAsync(email, password, token);

            return _imapClient.IsAuthenticated;
        }
        catch (Exception e)
        {
            Logger.LogError("Ошибка верификации пароля {error}", e.Message);
        }

        return false;
    }

    public async Task<IMailWorkerService> Authenticate(HttpContext context)
    {
        var principal = context.User;
        if (principal.Identity!.IsAuthenticated)
        {
            var email = principal.FindFirstValue(EmailAppConstants.DefaultEmailClaim);
            var imapPassword = principal.FindFirstValue(EmailAppConstants.DefaultPasswordClaim);

            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email token missing");
            }

            if (string.IsNullOrEmpty(imapPassword))
            {
                throw new ArgumentNullException(nameof(email), "Password token missing");
            }

            if (_imapClient is {IsConnected: true})
            {
                await _imapClient.AuthenticateAsync(email, imapPassword);
            }

            if (_smtpClient is {IsConnected: true})
            {
                await _smtpClient.AuthenticateAsync(email, imapPassword);
            }

            _currentEmail = email;
        }

        return this;
    }

    public async Task<bool> SendAsync(MailData mailData, CancellationToken token)
    {
        try
        {
            var mail = new MimeMessage();

            #region Sender / Receiver

            // Отправитель
            mail.From.Add(new MailboxAddress(mailData.DisplayName, _currentEmail));
            mail.Sender = new MailboxAddress(mailData.DisplayName, _currentEmail);

            // Получатели
            foreach (string mailAddress in mailData.To)
                mail.To.Add(MailboxAddress.Parse(mailAddress));

            // Set Reply to if specified in mail data
            if (!string.IsNullOrEmpty(mailData.ReplyTo))
                mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

            // BCC
            // Check if a BCC was supplied in the request
            if (mailData.Bcc.Any())
            {
                // Get only addresses where value is not null or with whitespace. x = value of address
                foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
            }

            // CC
            // Check if a CC address was supplied in the request
            if (mailData.Cc != null)
            {
                foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
            }

            #endregion

            #region Content

            // Add Content to Mime Message
            var body = new BodyBuilder();
            mail.Subject = mailData.Subject;
            body.HtmlBody = mailData.Body;
          


            foreach (var attachmentModel in mailData.Attachments)
            {
                var type = ContentType.Parse(attachmentModel.ContentType);
                body.Attachments.Add(attachmentModel.FileName, attachmentModel.FileObject,type);
            }
            
            #endregion

            #region Send Mail

            _smtpClient.MessageSent += async (sender, args) =>
            {
                Logger.LogInformation("Письмо отправлено \n" +
                                      " Response : {response} \n" +
                                      " Message : {message}", args.Response, args.Message);
            };
            mail.Body = body.ToMessageBody();
            await _smtpClient.SendAsync(mail, token);

            #endregion

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_smtpClient is {IsConnected: true})
        {
            _smtpClient.Disconnect(true);
            _smtpClient.Dispose();
        }

        if (_imapClient is {IsConnected: true})
        {
            _imapClient.Disconnect(true);
            _imapClient.Dispose();
        }
    }
}