using MailClientApp.Services.SignatureService;

namespace tests;

public class UnitTest1
{
    private ISignature _signatureService;
    public UnitTest1()
    {
        _signatureService = new SignatureMessage();
        
        var currentFolder = Environment.CurrentDirectory;
        
        var bytePrivateKey =  File.ReadAllBytes( Path.Combine(currentFolder,"cert/private.key"));
        
        var bytePublicKey =  File.ReadAllBytes(Path.Combine(currentFolder,"cert/certificate.crt"));
        
         _signatureService.SetPrivateCert(bytePrivateKey);
        
         _signatureService.SetPublicCert(bytePublicKey);
         
    }
    
    
    
    [Fact]
    public async void TestSignMessageData()
    {
        string message = "Подписанное сообщение";
        
        var signature = await  _signatureService.Sign(message);

        bool isValid = await _signatureService.Verify(signature);
       
        Assert.True(isValid);
       
    }
    
    
    
    [Fact]
    public async void TestDetachedSignature()
    {
        string message = "Подписанное сообщение";
        

       var signature = await  _signatureService.SignDetached(message);

       bool isValid = await _signatureService.VerifyDetached(message, signature);
       
       Assert.True(isValid);
       
    }
    
    
    [Fact]
    public async void TestNotValidDetachedSignature()
    {
        string message = "Подписанное сообщение";

        var currentFolder = Environment.CurrentDirectory;
        
        var signature = await  _signatureService.SignDetached(message);

        string messageNotValid = "Подписанное сообщение1";

        bool isValid = await _signatureService.VerifyDetached(messageNotValid, signature);
       
        Assert.False(isValid);
       
    }
    
    [Fact]
    public async void TestFilesDetachedSignature()
    {
        var currentFolder = Environment.CurrentDirectory;

        var file1 = File.ReadAllBytes(Path.Combine(currentFolder, "files/test1.docx"));
        
        var file2 = File.ReadAllBytes(Path.Combine(currentFolder, "files/test2.docx"));

        var fileString1 = Convert.ToBase64String(file1);

        var fileString2 = System.Text.Encoding.UTF8.GetString(file2);

       
        var signature1 = await  _signatureService.SignDetached(fileString1);

        bool isValid1 = await _signatureService.VerifyDetached(fileString1, signature1);

        var signature2 = await  _signatureService.SignDetached(fileString2);

        bool isValid2 = await _signatureService.VerifyDetached(fileString2, signature2);
       
        Assert.True(isValid1 && isValid2);
       
    }
    
    
    [Fact]
    public async void TestFilesNotValidDetachedSignature()
    {
        var currentFolder = Environment.CurrentDirectory;

        var file1 = File.ReadAllBytes(Path.Combine(currentFolder, "files/test1.docx"));
        
        var file2 = File.ReadAllBytes(Path.Combine(currentFolder, "files/test2.docx"));
        

        var fileString1 = System.Text.Encoding.UTF8.GetString(file1);

        var fileString2 = System.Text.Encoding.UTF8.GetString(file2);

       
        var signature1 = await  _signatureService.SignDetached(fileString1);

        bool isValid1 = await _signatureService.VerifyDetached(fileString2, signature1);

        var signature2 = await  _signatureService.SignDetached(fileString2);

        bool isValid2 = await _signatureService.VerifyDetached(fileString1, signature2);
       
        Assert.False(isValid1 ||  isValid2);
       
    }
    
    
}