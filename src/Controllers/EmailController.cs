using System.Security.Claims;
using MailClientApp.Models.Requests;
using MailClientApp.Models.Responses;
using MailClientApp.Services.EMailService;
using MailClientApp.Services.EMailService.Models;
using MailClientApp.Services.Models;
using MailClientApp.Services.SignatureService;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailClientApp.Controllers;

[ApiController]
[Route("email")]
[Authorize]
public class EmailController : Controller
{
    private readonly IMailWorkerService _mailService;
    private readonly ISignature _signatureService;

    public EmailController(IMailWorkerService service, ISignature signatureService)
    {
        _mailService = service;
        _signatureService = signatureService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> EnterToEmailBox([FromBody] EmailLoginRequest request, CancellationToken token)
    {
        await _mailService.ConnectAsync(Protocol.Imap, token);

        var isValid = await _mailService.ValidatePassword(request.Email,
            request.Password, token);

        if (isValid)
        {
            await Authenticate(request.Email, request.Password);
            return Ok();
        }

        return BadRequest(new
        {
            message = "Невалидные учетные данные"
        });
    }

    private async Task Authenticate(string email, string password)
    {
        var claims = new List<Claim>
        {
            new Claim(EmailAppConstants.DefaultEmailClaim, email),
            new Claim(EmailAppConstants.DefaultPasswordClaim, password)
        };
        ClaimsIdentity id =
            new ClaimsIdentity(claims, "ApplicationCookie",
                EmailAppConstants.DefaultEmailClaim, ClaimsIdentity.DefaultRoleClaimType);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }

    [AllowAnonymous]
    [HttpGet("authenticated")]
    public async Task<IActionResult> IsAuthenticated()
    {
        if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
        {
            return Ok(new
            {
                IsAuthenticated = true
            });
        }

        return Unauthorized();
    }


    [HttpGet("logout")]
    public async Task<IActionResult> LogoutEmailBox()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/");
    }


    [HttpGet("messages/{id}")]
    public async Task<IActionResult> GetMyEmails([FromRoute] uint id, CancellationToken token)
    {
        await _mailService.ConnectAsync(Protocol.Imap, token);

        await _mailService.Authenticate(HttpContext);

        var message = await _mailService.GetMessageById(id);

        var messageResponses = new EmailMessageResponse()
        {
            Id = message.Id,
            Body = message.Body,
            Title = message.Title,
            From = message.From,
            Attachments = message.Attachments.Select(x => new AttachmentResponse()
            {
                FileName = x.FileName,
                ContentType = x.ContentType,
                Base64File = Convert.ToBase64String(x.FileObject)
            }).ToList()
        };

        return Ok(messageResponses);
    }


    [HttpGet("messages")]
    public async Task<IActionResult> GetMyEmails(CancellationToken token)
    {
        await _mailService.ConnectAsync(Protocol.Imap, token);

        await _mailService.Authenticate(HttpContext);


        var messages = await _mailService.GetInboxMessages(SearchQuery.All);

        return Ok(messages);
    }


    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromForm] SendVerifyRequest request, CancellationToken token)
    {
        var publicKey = FileHelper.ConvertIFormFile(request.PublicCertificate);
        await _signatureService.SetPublicCert(publicKey.FileObject);

        var signature = FileHelper.ConvertIFormFile(request.Signature);

        var base64FileString = Convert.ToBase64String(signature.FileObject);

        var result = await _signatureService.VerifyDetached(base64FileString, signature.FileObject);

        return Ok(new
        {
            verify = result
        });
    }


    [HttpPost("verify/body")]
    public async Task<IActionResult> Verify([FromForm] SendVerifyBodyRequest request, CancellationToken token)
    {
        var publicKey = FileHelper.ConvertIFormFile(request.PublicCertificate);
        await _signatureService.SetPublicCert(publicKey.FileObject);
        
        await _mailService.ConnectAsync(Protocol.Imap, token);

        await _mailService.Authenticate(HttpContext);

        var message = await _mailService.GetMessageById(request.Id);
        
        var result = await _signatureService.Verify(message.Body ?? string.Empty);

        return Ok(new
        {
            verify = result
        });
    }


    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromForm] SendEmailRequest
        request, CancellationToken token)
    {
        await _mailService.ConnectAsync(Protocol.Smtp, token);

        await _mailService.ConnectAsync(Protocol.Imap, token);

        await _mailService.Authenticate(HttpContext);

        List<FileAttachmentModel> files = new List<FileAttachmentModel>(request.Attachments?.Count ?? 1);

        if (request.Attachments != null)
        {
            foreach (var file in request.Attachments)
            {
                files.Add(FileHelper.ConvertIFormFile(file));
            }
        }

        if (request.SignatureCertificate != null)
        {
            var privateKey = FileHelper.ConvertIFormFile(request.SignatureCertificate).FileObject;
            await _signatureService.SetPrivateCert(privateKey);
        }

        if (request.SignatureCertificate != null && request.Body != null && request.SignBody)
        {
            request.Body = await _signatureService.Sign(request.Body);
        }


        if (files.Any() && request.SignatureCertificate != null)
        {
            foreach (var filesForSignature in new List<FileAttachmentModel>(files))
            {
                var base64FileString = Convert.ToBase64String(filesForSignature.FileObject);

                var signature = await _signatureService.SignDetached(base64FileString);

                files.Add(new FileAttachmentModel()
                {
                    FileName = filesForSignature.FileName + EmailAppConstants.SignatureExtension,
                    FileObject = signature,
                    ContentType = EmailAppConstants.SignatureContentType
                });
            }
        }

        var result = await _mailService.SendAsync(new MailData(request.To,
            request.Subject,
            request.Body,
            displayName: request.DisplayName,
            attachments: files), token);
        if (result)
        {
            return Ok();
        }

        return BadRequest();
    }
}