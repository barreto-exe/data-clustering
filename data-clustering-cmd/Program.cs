using DataClustering.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using DataClustering;
using System.Text.RegularExpressions;
using DataClustering.Utils;

string dbPath = GetDbPath();
var arguments = GetArguments();
var answerList = await GetAnswerList(dbPath);

GetResult1(answerList, arguments);
GetResult2(answerList, arguments);
GetResult3(answerList, arguments);

#region Result Functions
static async Task GetResult1(List<Answer> answers, Dictionary<string, object> args)
{
    bool dismissAltAnswers = (bool) GetArgsValue(args, "dismissAltAnswers");

    List<Answer> answersQ4;
    if(dismissAltAnswers)
    {
        answersQ4 = (from Answer a in answers
                                where a.QuestionNumber == 4 && (a.AnswerTitle == "Sí" || a.AnswerTitle == "No")
                                select a).ToList();
    }
    else
    {
        answersQ4 = (from Answer a in answers
                                where a.QuestionNumber == 4
                                select a).ToList();
    }
    
    int totalAnswered = answersQ4.Count;

    var countYes = answersQ4.Where(x => x.AnswerTitle == "Sí").Count();
    double percentageYes = (double)countYes / (double)totalAnswered;

    var countNo = answersQ4.Where(x => x.AnswerTitle == "No").Count();
    double percentageNo = (double)countNo / (double)totalAnswered;

    //Counting times alt answers were given
    double percentageAlt = 0;
    if (!dismissAltAnswers)
    {
        var countAlt = answersQ4.Where(x => x.AnswerTitle != "Sí" && x.AnswerTitle != "No").Count();
        percentageAlt = (double)countAlt / (double)totalAnswered;
    }
}
static async Task GetResult2(List<Answer> answers, Dictionary<string, object> args)
{
    var answersQ6 = GetAnswersFromOpenQuestion(answers, 6);
    double percentage = (double) GetArgsValue(args, "percentage");

    Dictionary<Answer, int> answersApparitions = new();
    List<Answer> alreadyCounted = new();
    try
    {
        do
        {
            //Get the first answer of the list that hasn't been clustered yet
            Answer answer = answersQ6.FirstOrDefault(x => !alreadyCounted.Contains(x));
            alreadyCounted.Add(answer);

            int apparitions = 1;

            foreach (Answer subAnswer in answersQ6)
            {
                if (alreadyCounted.Contains(subAnswer)) continue;

                if (SimilarityTool.CompareStrings(answer.AnswerText, subAnswer.AnswerText) >= percentage)
                {
                    alreadyCounted.Add(subAnswer);
                    apparitions++;
                }
            }

            //Put text in lower and capitalize the first letter
            answer.AnswerText = answer.AnswerText.ToLower().Trim();
            string first = answer.AnswerText[0].ToString().ToUpper();
            answer.AnswerText = first + answer.AnswerText.Substring(1);

            answersApparitions.Add(answer, apparitions);
        } while (alreadyCounted.Count < answersQ6.Count);
    }
    catch (Exception ex)
    {

    }
}
static async Task GetResult3(List<Answer> answers, Dictionary<string, object> args)
{
    List<Answer> answersQ7 = GetAnswersFromOpenQuestion(answers, 7);

    double percentage = (double) GetArgsValue(args, "percentage");

    Dictionary<Answer, int> answersApparitions = new();
    List<Answer> alreadyCounted = new();
    try
    {
        do
        {
            //Get the first answer of the list that hasn't been clustered yet
            Answer answer = answersQ7.FirstOrDefault(x => !alreadyCounted.Contains(x));
            alreadyCounted.Add(answer);

            int apparitions = 1;

            foreach (Answer subAnswer in answersQ7)
            {
                if (alreadyCounted.Contains(subAnswer)) continue;

                if (SimilarityTool.CompareStrings(answer.AnswerText, subAnswer.AnswerText) >= percentage)
                {
                    alreadyCounted.Add(subAnswer);
                    apparitions++;
                }
            }

            //Put text in lower and capitalize the first letter
            answer.AnswerText = answer.AnswerText.ToLower().Trim();
            string first = answer.AnswerText[0].ToString().ToUpper();
            answer.AnswerText = first + answer.AnswerText.Substring(1);

            answersApparitions.Add(answer, apparitions);
        } while (alreadyCounted.Count < answersQ7.Count);
    }
    catch (Exception ex)
    {

    }
}
#endregion

#region Other Functions
static string GetDbPath()
{
    //Console.WriteLine("Indique el path de la BD:");
    //return Console.ReadLine();

    return "D:\\Desktop\\datos.s3db";
}
Dictionary<string, object> GetArguments()
{
    Console.WriteLine(
        "Desea desechar respuestas alternativas en las preguntas cerradas? [S/N]");
    bool dismissAltAnswers = Console.ReadKey().Key == ConsoleKey.S;
    Console.WriteLine("\n\n");

    Console.WriteLine(
        $"Indique el margen de similitud aceptado para las respuestas abiertas. " +
        $"Siendo (0) muy flexible y (100) muy estricto");
    double percentage = Convert.ToDouble(Console.ReadLine());
    Console.WriteLine("\n\n");

    return new Dictionary<string, object>
    {
        [nameof(dismissAltAnswers)] = dismissAltAnswers,
        [nameof(percentage)] = (double)percentage / 100d,
    };
}
static async Task<List<Answer>> GetAnswerList(string dbPath)
{
    string query;
    SqliteConnection conn = new($"Data Source={dbPath};");

    await conn.OpenAsync();

    List<Answer> answerList = new();

    query =
        "SELECT " +
            "r.num_encuesta, r.num_pregunta, p.titulo as tit_preg, " +
            "r.num_opcion as num_resp, o.titulo as tit_resp, r.respuesta as texto_resp\n" +
        "FROM Respuesta r\n" +
        "INNER JOIN Pregunta p ON r.num_pregunta = p.num_pregunta\n" +
        "INNER JOIN Opcion o ON r.num_opcion = o.num_opcion AND r.num_pregunta = o.num_pregunta\n" +
        "ORDER BY r.num_encuesta, r.num_pregunta, r.num_opcion";
    using (var command = new SqliteCommand(query, conn))
    {
        var reader = command.ExecuteReader();

        while (await reader.ReadAsync())
        {
            Answer answer = new()
            {
                PollNumber = reader.GetInt32("num_encuesta"),
                AnswerNumber = reader.GetInt32("num_resp"),
                AnswerTitle = reader.GetString("tit_resp"),
                QuestionNumber = reader.GetInt32("num_pregunta"),
                QuestionTitle = reader.GetString("tit_preg"),
                AnswerText = reader.IsDBNull("texto_resp") ? null : reader.GetString("texto_resp"),
            };

            answerList.Add(answer);
        }

        await reader.CloseAsync();
    }

    return answerList;
}
static List<Answer> GetAnswersFromOpenQuestion(List<Answer> answers, int questionNumber)
{
    return (from Answer a in answers
            where a.QuestionNumber == questionNumber && !string.IsNullOrEmpty(a.AnswerText)
            select new Answer
            {
                PollNumber = a.PollNumber,
                AnswerNumber = a.AnswerNumber,
                AnswerTitle = a.AnswerTitle,
                QuestionTitle = a.QuestionTitle,
                QuestionNumber = a.QuestionNumber,
                AnswerText = Regex.Replace(a.AnswerText, @"[!@#$%^-_><&*0-9]", String.Empty),
            }).ToList();
}
static object GetArgsValue(Dictionary<string, object> args, string parameter)
{
    return args.FirstOrDefault(x => x.Key == parameter).Value;
}
#endregion