using System;
using System.Threading.Tasks;
using Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database;
using MsBox.Avalonia.Enums;

namespace FIAE2GO.ViewModels;

public partial class OptionsViewModel : ViewModelBase
{
    private readonly Action _onClose;

    [ObservableProperty]
    private int _totalQuestionCount;

    [ObservableProperty]
    private decimal _defaultQuestionCount;

    public bool IsDatabaseCopySupported => AppSettings.ManualDatabaseCopyAction != null;

    public OptionsViewModel(Action onClose)
    {
        _onClose = onClose;
        DefaultQuestionCount = AppSettings.DefaultQuestionCount;
        LoadTotalQuestionCount();
    }

    private async void LoadTotalQuestionCount()
    {
        try
        {
            var questions = await SQL.Instance.GetQuestions();
            TotalQuestionCount = questions?.Count ?? 0;
        }
        catch (Exception ex)
        {
            TotalQuestionCount = 0;
            await Helper.Instance.ShowMessageBoxAsync("Fehler", $"Fehler beim Laden der Fragenanzahl: {ex.Message}", ButtonEnum.Ok, Icon.Error);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        AppSettings.DefaultQuestionCount = (int)DefaultQuestionCount;
        AppSettings.Save();
        _onClose?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(IsDatabaseCopySupported))]
    private async Task CopyDatabase()
    {
        if (AppSettings.ManualDatabaseCopyAction != null) await AppSettings.ManualDatabaseCopyAction.Invoke();
    }
}