namespace Api.Options;

public class GitLabOptions
{
    /// <summary>
    /// Токен, который надо нагенерить в гитлаб (обязательно с возможностями Api)
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Ссылка на сам Gitlab
    /// </summary>
    public string Url { get; set; }
}