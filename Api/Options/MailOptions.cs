namespace Api.Options;

public class MailOptions
{
    public string SmtpType { get; set; }

    public int SmtpPort { get; set; }

    public string Address { get; set; }

    public string Password { get; set; }
}