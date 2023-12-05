using System.Globalization;
using Api.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace Api.Services;

public class RegisterService
{
    public async Task<List<User>> Registrate(IFormFile file)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = ";"
        };

        var records = new List<User>();
        using (var reader = new StreamReader("filePersons.csv"))
        using (var csv = new CsvReader(reader, configuration))
        {
            records = csv.GetRecords<User>().ToList();
            return records;
        }

        
    }
}