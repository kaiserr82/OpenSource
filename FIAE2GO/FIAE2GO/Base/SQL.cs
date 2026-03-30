using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform;
using Database;
using Microsoft.Data.Sqlite;
using MsBox.Avalonia.Enums;

namespace Base;

public class SQL()
{
    private static readonly Lazy<SQL> lazy =
    new Lazy<SQL>(() => new SQL());

    public static SQL Instance { get { return lazy.Value; } }

    public async Task<List<Question>> GetQuestions()
    {
        var list = new List<Question>();
        
        try
        {
            var dbPath = DatabaseHelper.GetDatabasePath();
            using var con = new SqliteConnection($"Data Source={dbPath}");
            await con.OpenAsync();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT id, exam_part, exam_type, topic, difficulty, type, question, option_a, option_b, option_c, option_d, correct_answer, points FROM questions;";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Question
                {
                    Id            = reader.GetInt32(reader.GetOrdinal("id")),
                    ExamPart      = reader.IsDBNull(reader.GetOrdinal("exam_part")) ? string.Empty : reader.GetString(reader.GetOrdinal("exam_part")),
                    ExamType      = reader.IsDBNull(reader.GetOrdinal("exam_type")) ? string.Empty : reader.GetString(reader.GetOrdinal("exam_type")),
                    Topic         = reader.IsDBNull(reader.GetOrdinal("topic")) ? string.Empty : reader.GetString(reader.GetOrdinal("topic")),
                    Difficulty    = reader.IsDBNull(reader.GetOrdinal("difficulty")) ? 0 : reader.GetInt32(reader.GetOrdinal("difficulty")),
                    Type          = reader.IsDBNull(reader.GetOrdinal("type")) ? string.Empty : reader.GetString(reader.GetOrdinal("type")),
                    QuestionText  = reader.IsDBNull(reader.GetOrdinal("question")) ? string.Empty : reader.GetString(reader.GetOrdinal("question")),
                    OptionA       = reader.IsDBNull(reader.GetOrdinal("option_a")) ? string.Empty : reader.GetString(reader.GetOrdinal("option_a")),
                    OptionB       = reader.IsDBNull(reader.GetOrdinal("option_b")) ? string.Empty : reader.GetString(reader.GetOrdinal("option_b")),
                    OptionC       = reader.IsDBNull(reader.GetOrdinal("option_c")) ? string.Empty : reader.GetString(reader.GetOrdinal("option_c")),
                    OptionD       = reader.IsDBNull(reader.GetOrdinal("option_d")) ? string.Empty : reader.GetString(reader.GetOrdinal("option_d")),
                    CorrectAnswer = reader.IsDBNull(reader.GetOrdinal("correct_answer")) ? string.Empty : reader.GetString(reader.GetOrdinal("correct_answer")),
                    Points        = reader.IsDBNull(reader.GetOrdinal("points")) ? 0 : reader.GetInt32(reader.GetOrdinal("points"))
                });
            }
        }
        catch (Exception ex)
        {
            await Helper.Instance.ShowMessageBoxAsync("Fehler", $"Datenbankfehler: {ex.Message}", ButtonEnum.Ok, Icon.Error);
        }

        return list;
    }
}