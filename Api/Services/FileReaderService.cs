using System.Globalization;
using System.Text;
using Api.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace Api.Services;

public class FileReaderService : IFileReaderService
{
    public async Task<List<User>> ReadUsers(IFormFile file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encodingByCodePage = Encoding.GetEncoding(1251);

        var configuration = new CsvConfiguration(new CultureInfo("ru-RU"))
        {
            HasHeaderRecord = false,
            Delimiter = ";",
            Encoding = encodingByCodePage,
        };

        using var reader = new StreamReader(file.OpenReadStream(), encodingByCodePage);
        using var csv = new CsvReader(reader, configuration);
        var records = csv.GetRecords<User>().ToList();
        return records;
    }
}