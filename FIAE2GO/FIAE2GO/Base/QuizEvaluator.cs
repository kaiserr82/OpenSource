using System;

namespace FIAE2GO.Base; // Oder ein anderer passender Namespace

public class QuizResult
{
    public int Points { get; init; }
    public int Grade { get; init; }
    public bool HasPassed { get; init; }
}

public static class QuizEvaluator
{
    /// <summary>
    /// Wertet die erreichten Punkte eines Quiz aus und gibt Note und Ergebnis zurück.
    /// </summary>
    /// <param name="points">Die erreichten Punkte (0-100).</param>
    /// <returns>Ein QuizResult-Objekt mit allen Informationen.</returns>
    public static QuizResult Evaluate(int points)
    {
        if (points < 0 || points > 100)
        {
            // Ungültige Punktzahl abfangen
            points = Math.Clamp(points, 0, 100);
        }

        int grade;
        if (points >= 92) grade = 1;
        else if (points >= 81) grade = 2;
        else if (points >= 67) grade = 3;
        else if (points >= 50) grade = 4;
        else if (points >= 30) grade = 5;
        else grade = 6;

        return new QuizResult
        {
            Points = points,
            Grade = grade,
            HasPassed = grade <= 4
        };
    }
}