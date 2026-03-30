namespace Database;

public class Question
{
    public int Id { get; set; }
    public string ExamPart { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Type { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Points { get; set; }
}
