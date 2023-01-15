using DataClustering.Models;
using Microsoft.Data.Sqlite;
using System.Data;

string dbPath = GetDbPath();
var arguments = GetArguments();
var answerList = await GetAnswerList(dbPath);

GetResult1(answerList, arguments);
GetResult2(answerList, arguments);
GetResult3(answerList, arguments);

#region Functions
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
    bool considerAltAnswers = Console.ReadKey().Key == ConsoleKey.S;

    Console.WriteLine(
        $"Indique el margen de similitud aceptado para las respuestas abiertas. " +
        $"Siendo (0) muy flexible y (100) muy estricto");
    int percentage = Convert.ToInt32(Console.ReadLine());

    return new Dictionary<string, object>
    {
        [nameof(considerAltAnswers)] = considerAltAnswers,
        [nameof(percentage)] = percentage,
    };
}
static async Task<List<Answer>> GetAnswerList(string dbPath)
{
    string query;
    SqliteConnection conn = new($"Data Source={dbPath};");

    await conn.OpenAsync();

    List<Answer> answerList = new();

    query = "SELECT * FROM Respuesta";
    using (var command = new SqliteCommand(query, conn))
    {
        var reader = command.ExecuteReader();

        while (await reader.ReadAsync())
        {
            Answer answer = new()
            {
                AnswerNumber = reader.GetInt32("num_encuesta"),
                QuestionNumber = reader.GetInt32("num_pregunta"),
                OptionNumber = reader.GetInt32("num_opcion"),
                Text = reader.IsDBNull("respuesta") ? null : reader.GetString("respuesta"),
            };

            answerList.Add(answer);
        }

        await reader.CloseAsync();
    }

    return answerList;
}
static async Task GetResult1(List<Answer> answers, Dictionary<string, object> args)
{

}
static async Task GetResult2(List<Answer> answers, Dictionary<string, object> args)
{

}
static async Task GetResult3(List<Answer> answers, Dictionary<string, object> args)
{

}
#endregion