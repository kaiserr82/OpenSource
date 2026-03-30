﻿﻿using System.Threading.Tasks;
using Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;

namespace FIAE2GO.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedExamType = 0;

    [ObservableProperty]
    private decimal _questionCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStartScreenVisible))]
    private bool _isQuizActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStartScreenVisible))]
    private bool _isOptionsActive;

    [ObservableProperty]
    private QuizViewModel? _currentQuizViewModel;

    [ObservableProperty]
    private OptionsViewModel? _currentOptionsViewModel;

    public bool IsStartScreenVisible => !IsQuizActive && !IsOptionsActive;

    public MainViewModel()
    {
        // Load the default question count from settings on startup
        try
        {
            QuestionCount = AppSettings.DefaultQuestionCount;
        }
        catch
        {
            QuestionCount = 15;
        }
    }


    [RelayCommand]
    private void Start()
    {
        CurrentQuizViewModel = new QuizViewModel(SelectedExamType, (int)QuestionCount, CloseQuiz);
        IsQuizActive = true;
    }

    private void CloseQuiz()
    {
        IsQuizActive = false;
        CurrentQuizViewModel = null;
    }

    [RelayCommand]
    private void OpenOptions()
    {
        CurrentOptionsViewModel = new OptionsViewModel(CloseOptions);
        IsOptionsActive = true;
    }

    private void CloseOptions()
    {
        IsOptionsActive = false;
        CurrentOptionsViewModel = null;
        QuestionCount = AppSettings.DefaultQuestionCount;
    }
}