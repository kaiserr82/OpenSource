using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database;
using MsBox.Avalonia.Enums;

namespace FIAE2GO.ViewModels;

public partial class QuizViewModel : ViewModelBase
{
    private readonly Action _onClose;
    private List<Question> _quizQuestions = new();
    private List<Question> _wrongAnswers = new();
    private List<Question> _rightAnswers = new();
    private Dictionary<int, (bool A, bool B, bool C, bool D)> _userAnswers = new();
    private int _totalQuestionCount;
    private int _currentIndex;

    [ObservableProperty]
    private Question? _currentQuestion;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private bool _isReady;

    [ObservableProperty] private bool _isOptionASelected;
    [ObservableProperty] private bool _isOptionBSelected;
    [ObservableProperty] private bool _isOptionCSelected;
    [ObservableProperty] private bool _isOptionDSelected;

    [ObservableProperty] private bool _isResultMode;
    [ObservableProperty] private bool _isOptionACorrect;
    [ObservableProperty] private bool _isOptionBCorrect;
    [ObservableProperty] private bool _isOptionCCorrect;
    [ObservableProperty] private bool _isOptionDCorrect;
    [ObservableProperty] private bool _isOptionAWrong;
    [ObservableProperty] private bool _isOptionBWrong;
    [ObservableProperty] private bool _isOptionCWrong;
    [ObservableProperty] private bool _isOptionDWrong;

    public bool CanGoBack => _currentIndex > 0;
    public string NextButtonText => (IsResultMode && _currentIndex == _quizQuestions.Count - 1) ? "Beenden" : "Weiter";

    public QuizViewModel(int examType, int count, Action onClose)
    {
        _onClose = onClose;
        LoadQuestions(examType, count);
    }

    private async void LoadQuestions(int examType, int count)
    {
        try
        {
            var allQuestions = await SQL.Instance.GetQuestions();
            
            if (allQuestions == null || !allQuestions.Any())
            {
                await Helper.Instance.ShowMessageBoxAsync("Fehler", "Die Datenbank ist leer oder konnte nicht geladen werden.\nStellen Sie sicher, dass die Datenbank korrekt eingerichtet ist.", ButtonEnum.Ok, Icon.Error);
                _onClose?.Invoke();
                return;
            }

            IEnumerable<Question> filtered = allQuestions;

            // 0 = Zwischenprüfung, 1 = Abschlussprüfung, 2 = Gemischt
            if (examType == 0) filtered = allQuestions.Where(q => q.ExamType == "Zwischenprüfung");
            else if (examType == 1) filtered = allQuestions.Where(q => q.ExamType == "Abschlussprüfung");

            var rnd = new Random();
            _quizQuestions = filtered.OrderBy(x => rnd.Next()).Take(count).ToList();

            _totalQuestionCount = _quizQuestions.Count;
            if (_totalQuestionCount > 0)
            {
                ShowQuestion(0);
                IsReady = true;
            }
            else
            {
                await Helper.Instance.ShowMessageBoxAsync("Info", "Für die gewählte Kategorie wurden keine Fragen gefunden.", ButtonEnum.Ok, Icon.Info);
                _onClose?.Invoke();
            }
        }
        catch (Exception ex)
        {
            await Helper.Instance.ShowMessageBoxAsync("Fehler", $"Datenbankfehler: {ex.Message}", ButtonEnum.Ok, Icon.Error);
            _onClose?.Invoke();
        }
    }

    private void ShowQuestion(int index)
    {
        _currentIndex = index;
        if (_currentIndex < _quizQuestions.Count)
        {
            CurrentQuestion = _quizQuestions[_currentIndex];
            ProgressText = $"Frage {_currentIndex + 1} von {_quizQuestions.Count}";
            
            if (_userAnswers.TryGetValue(_currentIndex, out var selection))
            {
                IsOptionASelected = selection.A;
                IsOptionBSelected = selection.B;
                IsOptionCSelected = selection.C;
                IsOptionDSelected = selection.D;
            }
            else
            {
                IsOptionASelected = false;
                IsOptionBSelected = false;
                IsOptionCSelected = false;
                IsOptionDSelected = false;
            }
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(NextButtonText));
            UpdateResultIndicators();
        }
        else
        {
            if (IsResultMode)
            {
                _onClose?.Invoke();
            }
            else
            {
                FinishRound();
            }
        }
    }

    private void SaveUserAnswer()
    {
        _userAnswers[_currentIndex] = (IsOptionASelected, IsOptionBSelected, IsOptionCSelected, IsOptionDSelected);
    }

    private void UpdateResultIndicators()
    {
        if (!IsResultMode || CurrentQuestion == null)
        {
            IsOptionACorrect = IsOptionBCorrect = IsOptionCCorrect = IsOptionDCorrect = false;
            IsOptionAWrong = IsOptionBWrong = IsOptionCWrong = IsOptionDWrong = false;
            return;
        }

        var correct = CurrentQuestion.CorrectAnswer.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim().ToUpper()).ToHashSet();

        IsOptionACorrect = correct.Contains("A");
        IsOptionBCorrect = correct.Contains("B");
        IsOptionCCorrect = correct.Contains("C");
        IsOptionDCorrect = correct.Contains("D");

        IsOptionAWrong = IsOptionASelected && !IsOptionACorrect;
        IsOptionBWrong = IsOptionBSelected && !IsOptionBCorrect;
        IsOptionCWrong = IsOptionCSelected && !IsOptionCCorrect;
        IsOptionDWrong = IsOptionDSelected && !IsOptionDCorrect;
    }

    [RelayCommand]
    private void Next()
    {
        if (!IsResultMode) SaveUserAnswer();
        ShowQuestion(_currentIndex + 1);
    }

    [RelayCommand]
    private void Previous()
    {
        if (_currentIndex > 0)
        {
            if (!IsResultMode) SaveUserAnswer();
            ShowQuestion(_currentIndex - 1);
        }
    }

    private bool SplitAndCheck(Question currentQuestion, bool isA, bool isB, bool isC, bool isD)
    {
        // Build a list of selected option identifiers (A, B, C, D)
        var selectedAnswers = new List<string>();
        if (isA) selectedAnswers.Add("A");
        if (isB) selectedAnswers.Add("B");
        if (isC) selectedAnswers.Add("C");
        if (isD) selectedAnswers.Add("D");

        // Get the correct answers from the comma-separated string (e.g., "A, C")
        var correctAnswers = new HashSet<string>(
            currentQuestion.CorrectAnswer.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim()),
            StringComparer.OrdinalIgnoreCase
        );

        // Check if the set of selected answers is identical to the set of correct answers
        return correctAnswers.SetEquals(selectedAnswers);
    }

    private async void FinishRound()
    {
        _rightAnswers.Clear();
        _wrongAnswers.Clear();

        for (int i = 0; i < _quizQuestions.Count; i++)
        {
            var q = _quizQuestions[i];
            bool a = false, b = false, c = false, d = false;
            if (_userAnswers.TryGetValue(i, out var s)) { a = s.A; b = s.B; c = s.C; d = s.D; }

            bool isCorrect;
            isCorrect = SplitAndCheck(q, a, b, c, d);

            if (isCorrect) _rightAnswers.Add(q);
            else _wrongAnswers.Add(q);
        }

        // Finale Auswertung
        int correctCount = _rightAnswers.Count;
        int totalCount = _totalQuestionCount;
        int points = totalCount > 0 ? (int)Math.Round((double)correctCount / totalCount * 100) : 0;

        var quizResult = Base.QuizEvaluator.Evaluate(points);

        string title = quizResult.HasPassed ? "Bestanden!" : "Nicht bestanden";
        string message = $"Sie haben das Quiz mit Note {quizResult.Grade} abgeschlossen.\n\n" +
                         $"Richtige Antworten: {correctCount} von {totalCount}\n" +
                         $"Erreichte Punkte: {quizResult.Points} von 100";

        await Helper.Instance.ShowMessageBoxAsync(title, message, ButtonEnum.Ok,
            quizResult.HasPassed ? Icon.Success : Icon.Error);
        
        // In den Ergebnis-Modus wechseln
        IsResultMode = true;
        ShowQuestion(0);
    }

    [RelayCommand]
    private void Cancel() => _onClose?.Invoke();
}