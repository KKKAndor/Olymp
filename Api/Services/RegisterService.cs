using System.Globalization;
using System.Text;
using Api.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace Api.Services;

public class RegisterService
{
    public async Task<List<User>> Registrate(IFormFile file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encodingByCodePage = Encoding.GetEncoding(1251);

        var configuration = new CsvConfiguration(new CultureInfo("ru-RU"))
        {
            HasHeaderRecord = false,
            Delimiter = ";",
            Encoding = encodingByCodePage,
        };

        var records = new List<User>();

        using var reader = new StreamReader(file.OpenReadStream(), encodingByCodePage);
        using var csv = new CsvReader(reader, configuration);
        records = csv.GetRecords<User>().ToList();
        return records;
    }
}