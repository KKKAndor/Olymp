namespace Api.Models;

public class UserFailureResponse
{
    public UserFailureResponse(string email, string failureAt, string message)
    {
        Email = email;
        FailureAt = failureAt;
        Message = message;
    }

    public string Email { get; set; }

    public string FailureAt { get; set; }

    public string Message { get; set; }
}