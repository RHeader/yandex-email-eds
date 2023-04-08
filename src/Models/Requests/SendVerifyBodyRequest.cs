namespace MailClientApp.Models.Requests;

public class SendVerifyBodyRequest
{
    /// <summary>
    /// Public certificate for check EDS
    /// </summary>
    public IFormFile PublicCertificate { get; set; } = null!;

    /// <summary>
    /// Id email message
    /// </summary>
    public uint Id { get; set; } 
}