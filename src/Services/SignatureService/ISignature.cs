namespace MailClientApp.Services.SignatureService;

public interface ISignature
{
     Task SetPrivateCert(byte[] cert);
     Task SetPublicCert(byte[] cert);
     Task<byte[]> SignDetached(string message);
     Task<bool> VerifyDetached(string message, byte[] signature);
     
     Task<string> Sign(string message);
     Task<bool> Verify(string message);
}