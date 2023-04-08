namespace MailClientApp;

public class EmailAppConstants
{
    public const string DefaultEmailClaim = "Email";
    public const string DefaultPasswordClaim = "Password";
    public const string SignatureExtension = ".sig";
    public const string SignatureContentType = "application/pgp-signature";
}

public enum Protocol
{
    Imap,
    Smtp
}