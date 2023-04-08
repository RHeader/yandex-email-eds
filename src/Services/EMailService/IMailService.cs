using MailClientApp.Services.EMailService.Models;
using MailClientApp.Services.Models;
using MailKit.Search;

namespace MailClientApp.Services.EMailService;

public interface IMailWorkerService:IDisposable
{
    Task<bool> SendAsync(MailData mailData, CancellationToken token);
    Task<IMailWorkerService> ConnectAsync(Protocol protocol, CancellationToken token);
    Task<IMailWorkerService> Authenticate(HttpContext context);
    Task<EmailInboxMessage> GetMessageById(uint id);
    Task<List<EmailInboxPreviewMessage>> GetInboxMessages(SearchQuery query);
    Task<bool> ValidatePassword(string email, string password, CancellationToken token);
}