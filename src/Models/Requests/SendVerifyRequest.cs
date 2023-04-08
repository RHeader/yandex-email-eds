namespace MailClientApp.Models.Requests;

public class SendVerifyRequest
{
    /// <summary>
    /// Public certificate for check EDS
    /// </summary>
    public IFormFile PublicCertificate { get; set; } = null!;

    /// <summary>
    /// Signature associate with File
    /// </summary>
    public IFormFile Signature { get; set; } = null!;

    /// <summary>
    /// File
    /// </summary>
    public IFormFile File { get; set; } = null!;
}