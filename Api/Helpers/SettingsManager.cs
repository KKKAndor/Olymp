using System.Reflection;

namespace Api.Helpers;

public class SettingsManager
{
    private static SettingsManager _appSettings;

    private SettingsManager(IConfiguration config, string key)
    {
        AppSettingValue = config.GetValue<string>(key);
    }

    private string AppSettingValue { get; }

    public static string Get(string key)
    {
        _appSettings = GetCurrentSettings(key);
        return _appSettings?.AppSettingValue;
    }
    public static T Get<T>(string key)
    {
        _appSettings = GetCurrentSettings(key);
        return _appSettings?.AppSettingValue == null ? default 
            : (T)Convert.ChangeType(_appSettings.AppSettingValue, typeof(T));
    }
    
    private static SettingsManager GetCurrentSettings(string key)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var assemblyDirectory = Path.GetDirectoryName(executingAssembly.Location);
        var solutionDirectory = Directory.GetParent(assemblyDirectory).Parent?.Parent?.Parent?.FullName;
        var projectName =  Assembly.GetAssembly(typeof(SettingsManager))?.GetName().Name;

        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(solutionDirectory, projectName))
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var settings = new SettingsManager(configuration, key);

        return settings;
    }
}