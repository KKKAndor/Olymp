using CsvHelper.Configuration.Attributes;

namespace Api.Models;

public class User
{
    [Index(0)] 
    public string Email { get; set; }
    [Index(1)]
    public string Name { get; set; }
    [Index(2)]
    public string Group { get; set; }
}