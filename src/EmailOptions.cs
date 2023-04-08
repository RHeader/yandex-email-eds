namespace MailClientApp;

public class EmailOptions
{
    public Imap Imap { get; set; } = null!;
    public Smtp Smtp { get; set; } = null!;
    public bool SslEnable { get; set; } = true;
}

public class Smtp
{
    public string Dns { get; set; } = null!;
    public int Port { get; set; }
}

public class Imap
{
    public string Dns { get; set; } = null!;
    public int Port { get; set; }
}