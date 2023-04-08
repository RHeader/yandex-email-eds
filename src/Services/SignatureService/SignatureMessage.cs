using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MailClientApp.Services.SignatureService;

//openssl req -x509 -newkey rsa:2048 -keyout private.key -out certificate.crt -days 365 -nodes

public class SignatureMessage : ISignature
{
    private RSA privateKey;
    private X509Certificate2 publicCert;

    public async Task SetPrivateCert(byte[] cert)
    {
        if (cert == null || cert.Length == 0)
            throw new ArgumentException("Certificate data is empty");

        var messageBytes = System.Text.Encoding.UTF8.GetString(cert);

        privateKey = RSA.Create();

        privateKey.ImportFromPem(messageBytes.ToCharArray());


        if (privateKey == null)
            throw new ArgumentException("Certificate does not contain a private key");
    }

    public async Task SetPublicCert(byte[] cert)
    {
        if (cert == null || cert.Length == 0)
            throw new ArgumentException("Certificate data is empty");

        publicCert = new X509Certificate2(cert);

        if (publicCert.PublicKey == null)
            throw new ArgumentException("Certificate does not contain a public key");
    }

    public async Task<byte[]> SignDetached(string message)
    {
        if (privateKey == null)
            throw new InvalidOperationException("Private key is not set");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message is empty");

        var hash = HashingMessage(message);

        var signatureBytes = privateKey.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return signatureBytes;
    }

    public async Task<bool> VerifyDetached(string message, byte[] signature)
    {
        if (publicCert == null)
            throw new InvalidOperationException("Public key is not set");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message is empty");

        var hash = HashingMessage(message);

        var publicKey = publicCert.GetRSAPublicKey();
        var verified = publicKey.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return verified;
    }

    private string WrapMessageSignature(string message, string signature)
    {
        return $"-----BEGIN EDSSIGNATURE----- \n {signature} \n -----END EDSSIGNATURE----- \n \n \n ---BEGIN MESSAGE--- \n{message} \n---END MESSAGE---";
    }

    public async Task<string> Sign(string message)
    {
        if (privateKey == null)
            throw new InvalidOperationException("Private key is not set");

        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message is empty");

        var byteString = Encoding.UTF8.GetBytes(message.Trim());

        var signatureBytes = privateKey.SignData(byteString, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return WrapMessageSignature(message, Convert.ToBase64String(signatureBytes));
    }

    public async Task<bool> Verify(string message)
    {
        if (publicCert == null)
            throw new InvalidOperationException("Public key is not set");


        var messageHeader = "---BEGIN MESSAGE---";
        var messageFooter = "---END MESSAGE---";
        
        var  messageStartIndex = message.IndexOf(messageHeader);
        var  messageEndIndex = message.IndexOf(messageFooter);
        if (messageStartIndex == -1 || messageEndIndex == -1 || messageEndIndex <= messageStartIndex)
            throw new ArgumentException("Not have message");

        var messageFromSignString = message.Substring(messageStartIndex + messageHeader.Length, messageEndIndex - messageStartIndex - messageHeader.Length).Trim();

        var dataToVerify = Encoding.UTF8.GetBytes(messageFromSignString);

        var signatureHeader = "-----BEGIN EDSSIGNATURE-----";
        var signatureFooter = "-----END EDSSIGNATURE-----";
        var signatureStartIndex = message.IndexOf(signatureHeader);
        var signatureEndIndex = message.IndexOf(signatureFooter);
        if (signatureStartIndex == -1 || signatureEndIndex == -1 || signatureEndIndex <= signatureStartIndex)
            throw new ArgumentException("Message does not contain EDS signature");

         var signatureBase64 = message.Substring(signatureStartIndex + signatureHeader.Length, signatureEndIndex - signatureStartIndex - signatureHeader.Length).Trim();
         
         var signatureBytes = Convert.FromBase64String(signatureBase64);
         


        var publicKey = publicCert.GetRSAPublicKey();

        return publicKey.VerifyData(dataToVerify, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }


    private byte[] HashingMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message is empty");
        }

        var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

        var hash = SHA256.Create().ComputeHash(messageBytes);

        return hash;
    }
}