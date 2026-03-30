using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FIAE2GO;

public static class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FIAE2GO", "settings.json");

    public static Func<Task>? ManualDatabaseCopyAction { get; set; }

    public static int DefaultQuestionCount { get; set; } = 5;

    static AppSettings()
    {
        Load();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var data = JsonSerializer.Deserialize<AppSettingsData>(json);
                if (data != null)
                {
                    DefaultQuestionCount = data.DefaultQuestionCount;
                }
            }
        }
        catch { /* Fehler ignorieren oder loggen */ }
    }

    public static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(dir) && dir != null) Directory.CreateDirectory(dir);

            var data = new AppSettingsData { DefaultQuestionCount = DefaultQuestionCount };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(SettingsPath, json);
        }
        catch { /* Fehler ignorieren oder loggen */ }
    }

    private class AppSettingsData
    {
        public int DefaultQuestionCount { get; set; }
    }
}