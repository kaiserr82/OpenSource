﻿using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Base;
using MsBox.Avalonia.Enums;

namespace FIAE2GO.Android;

[Activity(
    Label = "FIAE2GO.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        SetupAndroidServices();
        try
        {
            // Check for a pending database update from a manual import.
            // This needs to run before the database is accessed by the app.
            ApplyPendingDatabaseUpdate("fiae2go.db");

            CopyDatabaseIfNotExists("fiae2go.db");
        }
        catch (Exception)
        {
            // Fehler ignorieren, damit die App nicht abstürzt (z.B. wenn Asset fehlt).
        }

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    private void SetupAndroidServices()
    {
        AppSettings.ManualDatabaseCopyAction = async () =>
        {
            try
            {
                var singleView = Avalonia.Application.Current?.ApplicationLifetime as ISingleViewApplicationLifetime;
                if (singleView?.MainView is null)
                {
                    await Helper.Instance.ShowMessageBoxAsync("Fehler", "Die Hauptansicht konnte nicht gefunden werden.", ButtonEnum.Ok, Icon.Error);
                    return;
                }

                var topLevel = TopLevel.GetTopLevel(singleView.MainView);
                if (topLevel is null)
                {
                    await Helper.Instance.ShowMessageBoxAsync("Fehler", "Die Top-Level-Ansicht konnte nicht gefunden werden.", ButtonEnum.Ok, Icon.Error);
                    return;
                }

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Datenbankdatei auswählen",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("SQLite Database") { Patterns = new[] { "*.db" } } }
                });

                if (files.Count == 1)
                {
                    // Das erste (und einzige) IStorageFile‑Objekt
                    var file = files[0];
                    var path = DatabaseHelper.GetDatabasePath();

                    // Stream vom Quell‑IStorageFile öffnen
                    await using Stream sourceStream = await file.OpenReadAsync();

                    // Ziel‑Stream öffnen (FileMode.Create überschreibt ggf. vorhandene Datei)
                    await using FileStream targetStream = new FileStream(
                        path,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 81920,          // Standard‑Puffergröße
                        useAsync: true);            // async‑optimiert

                    // Kopieren – CopyToAsync ist komplett asynchron und blockiert nicht den UI‑Thread
                    await sourceStream.CopyToAsync(targetStream);



                    await Helper.Instance.ShowMessageBoxAsync("Erfolg", $"Die Datenbank wurde erfolgreich importiert. \nBitte starten Sie die Anwendung neu.", ButtonEnum.Ok, Icon.Success);
                }
            }
            catch (Exception ex)
            {
                await Helper.Instance.ShowMessageBoxAsync("Fehler", $"Fehler beim Importieren der Datenbank: {ex.Message}", ButtonEnum.Ok, Icon.Error);
            }
        };
    }

    private void ApplyPendingDatabaseUpdate(string dbName)
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dbName);
        var dbUpdatePath = dbPath + ".update";

        if (File.Exists(dbUpdatePath))
        {
            try
            {
                // This will overwrite the existing database file.
                // It's done at startup before any connection can be made.
                File.Move(dbUpdatePath, dbPath, true);
            }
            catch (Exception)
            {
                // If moving fails, delete the update file to prevent repeated failures on startup.
                // The user will have to try importing again.
                try { File.Delete(dbUpdatePath); }
                catch { /* Ignore if deletion also fails. */ }
            }
        }
    }

    private void CopyDatabaseIfNotExists(string dbName)
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dbName);
        if (!File.Exists(dbPath))
        {
            CopyDatabaseFromAssets(dbName, false);
        }
    }

    private void CopyDatabaseFromAssets(string dbName, bool overwrite)
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dbName);
        if (!overwrite && File.Exists(dbPath)) return;

        using var br = new BinaryReader(Assets!.Open(dbName));
        using var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create));
        byte[] buffer = new byte[2048];
        int length;
        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
        {
            bw.Write(buffer, 0, length);
        }
    }
}
