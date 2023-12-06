namespace Api.Services;

public interface IEmailSender
{
    Task SendMail(string email, string login, string name, string password);
}