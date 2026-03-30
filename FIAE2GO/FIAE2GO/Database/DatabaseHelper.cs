using System;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using Microsoft.Data.Sqlite;

public static class DatabaseHelper
{
    public static string GetDatabasePath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dbPath = Path.Combine(appData, "fiae2go.db");

        if (!File.Exists(dbPath))
        {
            CopyDatabaseFromAssets(dbPath);
        }

        return dbPath;
    }

    private static void CopyDatabaseFromAssets(string targetPath)
    {
        var assets = AssetLoader.Open(new Uri("avares://FIAE2GO/Assets/fiae2go.db"));

        using var fileStream = File.Create(targetPath);
        assets.CopyTo(fileStream);
    }
}
