using System.Security.Cryptography;
using System.Text;

namespace Api.Services;

public static class Helper
{
    public static string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[length];
        rng.GetBytes(randomBytes);

        var password = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            var randomNumber = randomBytes[i] % validChars.Length;
            password.Append(validChars[randomNumber]);
        }

        return password.ToString();
    }

    public static string TransliterateToLatin(string text)
    {
        var cyrillicToLatin = new Dictionary<char, string>
        {
            {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"}, {'Е', "E"}, {'Ё', "YO"},
            {'Ж', "ZH"}, {'З', "Z"}, {'И', "I"}, {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"},
            {'Н', "N"}, {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"}, {'У', "U"},
            {'Ф', "F"}, {'Х', "KH"}, {'Ц', "TS"}, {'Ч', "CH"}, {'Ш', "SH"}, {'Щ', "SCH"}, {'Ъ', ""},
            {'Ы', "Y"}, {'Ь', ""}, {'Э', "E"}, {'Ю', "YU"}, {'Я', "YA"},
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"}, {'е', "e"}, {'ё', "yo"},
            {'ж', "zh"}, {'з', "z"}, {'и', "i"}, {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"},
            {'н', "n"}, {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"}, {'у', "u"},
            {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"}, {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""},
            {'ы', "y"}, {'ь', ""}, {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        var latinText = new StringBuilder();

        foreach (var symbol in text)
        {
            if (cyrillicToLatin.TryGetValue(symbol, out var value))
            {
                latinText.Append(value);
            }
            else
            {
                latinText.Append(symbol);
            }
        }

        return latinText.ToString();
    }
}