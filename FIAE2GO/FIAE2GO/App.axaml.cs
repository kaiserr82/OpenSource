using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Base;
using MsBox.Avalonia.Enums;
using FIAE2GO.ViewModels;
using FIAE2GO.Views;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace FIAE2GO;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ApplyPendingDatabaseUpdate();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            SetupDesktopServices(desktop);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupDesktopServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        AppSettings.ManualDatabaseCopyAction = async () =>
        {
            try
            {
                if (desktop.MainWindow is null) return;

                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel is null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Datenbankdatei auswählen",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("SQLite Database") { Patterns = new[] { "*.db" } } }
                });

                if (files.Count >= 1)
                {
                    var sourcePath = files[0].TryGetLocalPath();
                    if (sourcePath is not null)
                    {
                        // To avoid file lock issues, copy to a temp file.
                        // The update will be applied on next app start.
                        var targetUpdatePath = Path.Combine(AppContext.BaseDirectory, "fiae2go.db.update");
                        File.Copy(sourcePath, targetUpdatePath, true);
                        await Helper.Instance.ShowMessageBoxAsync("Erfolg", "Die Datenbank wurde erfolgreich importiert. Bitte starten Sie die Anwendung neu.", ButtonEnum.Ok, Icon.Success);
                    }
                    else
                    {
                        await Helper.Instance.ShowMessageBoxAsync("Fehler", "Der Pfad zur ausgewählten Datei konnte nicht ermittelt werden.", ButtonEnum.Ok, Icon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await Helper.Instance.ShowMessageBoxAsync("Fehler", $"Fehler beim Importieren der Datenbank: {ex.Message}", ButtonEnum.Ok, Icon.Error);
            }
        };
    }

    private void ApplyPendingDatabaseUpdate()
    {
        var dbName = "fiae2go.db";
        var dbPath = Path.Combine(AppContext.BaseDirectory, dbName);
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
                try
                {
                    File.Delete(dbUpdatePath);
                }
                catch { /* Ignore if deletion also fails. */ }
            }
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}