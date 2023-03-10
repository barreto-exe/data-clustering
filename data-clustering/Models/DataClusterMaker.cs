using DataClustering.Utils;
using Microsoft.Data.Sqlite;
using System;
using LiveCharts;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DataClustering.Models
{
    public class DataClusterMaker
    {
        public Dictionary<string, object> Arguments { get; set; }
        public string DbPath { get; set; }
        public List<Answer> Answers { get; set; }

        public DataClusterMaker(Dictionary<string, object> arguments, string dbPath)
        {
            Arguments = arguments;
            DbPath = dbPath;
        }

        public async Task FillAnswerList()
        {
            string query;
            SqliteConnection conn = new($"Data Source={DbPath};");

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

            Answers = answerList;
        }

        public List<Answer> GetAnswersFromOpenQuestion(int questionNumber)
        {
            return (from Answer a in Answers
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
        public object GetArgsValue(string parameter)
        {
            return Arguments.FirstOrDefault(x => x.Key == parameter).Value;
        }

        public async Task<Dictionary<string, int>> GetResult1()
        {
            bool dismissAltAnswers = (bool)GetArgsValue("dismissAltAnswers");

            List<Answer> answersQ4;
            if (dismissAltAnswers)
            {
                answersQ4 = (from Answer a in Answers
                             where a.QuestionNumber == 4 && (a.AnswerTitle == "Sí" || a.AnswerTitle == "No")
                             select a).ToList();
            }
            else
            {
                answersQ4 = (from Answer a in Answers
                             where a.QuestionNumber == 4
                             select a).ToList();
            }

            int totalAnswered = answersQ4.Count;

            var countYes = answersQ4.Where(x => x.AnswerTitle == "Sí").Count();
            var countNo = answersQ4.Where(x => x.AnswerTitle == "No").Count();

            Dictionary<string, int> result = new()
            {
                ["Sí"] = countYes,
                ["No"] = countNo,
            };

            //Counting times alt answers were given
            if (!dismissAltAnswers)
            {
                int countAlt = 
                    answersQ4
                    .Where(x => x.AnswerTitle != "Sí" && x.AnswerTitle != "No")
                    .Count();

                result.Add("Otro", countAlt);
            }

            return result;
        }
        public async Task<Dictionary<string, int>> GetResult2()
        {
            var answersQ6 = GetAnswersFromOpenQuestion(6);
            double percentage = (double)GetArgsValue("percentage");

            Dictionary<string, int> answersApparitions = new();
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

                    answersApparitions.Add(answer.AnswerText, apparitions);
                } while (alreadyCounted.Count < answersQ6.Count);
            }
            catch (Exception ex)
            {

            }

            //Take the first 10 most appearing
            var result = answersApparitions.OrderByDescending(x => x.Value).Take(10).ToDictionary(x => x.Key, x => x.Value);

            //Add and "others" section
            var otros = answersApparitions.OrderBy(x => x.Value).Take(answersApparitions.Count - 10);
            result.Add("Otros", otros.Sum(x => x.Value));

            return result;
        }
        public async Task<Dictionary<string, int>> GetResult3()
        {
            List<Answer> answersQ7 = GetAnswersFromOpenQuestion(7);

            double percentage = (double)GetArgsValue("percentage");

            Dictionary<string, int> answersApparitions = new();
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

                    answersApparitions.Add(answer.AnswerText, apparitions);
                } while (alreadyCounted.Count < answersQ7.Count);
            }
            catch (Exception ex)
            {

            }

            var textInfo = CultureInfo.CurrentCulture.TextInfo;

            //Take the first 15 most appearing
            int i = 1;
            var result = answersApparitions
                .OrderByDescending(x => x.Value)
                .Take(15)
                .ToDictionary(x => $"{i++}° - {textInfo.ToTitleCase(x.Key)}", x => x.Value);

            ////Add and "others" section
            //var otros = answersApparitions.OrderBy(x => x.Value).Take(answersApparitions.Count - 15);
            //result.Add("Otros", otros.Sum(x => x.Value));

            return result;
        }
    }
}