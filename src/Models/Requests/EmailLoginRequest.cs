using System.ComponentModel;

namespace MailClientApp.Models.Requests;


public class EmailLoginRequest
{
    [DefaultValue("Identity.osp7@yandex.ru")]
    public string Email { get; set; } = null!;
    [DefaultValue("rninxqvuxwrhipoc")]
    public string Password { get; set; } = null!;
}