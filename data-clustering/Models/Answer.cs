using System;

namespace DataClustering.Models
{
    public class Answer
    {
        public int PollNumber { get; set; }
        public int QuestionNumber { get; set; }
        public string? QuestionTitle { get; set; }
        public int AnswerNumber { get; set; }
        public string? AnswerTitle { get; set; }
        public string? AnswerText { get; set; }

        public override string? ToString()
        {
            return $"Poll: {PollNumber} - Question: {QuestionNumber} - AnswerNumer: {AnswerNumber} - AnswerText: {AnswerText}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Answer answer &&
                   PollNumber == answer.PollNumber &&
                   QuestionNumber == answer.QuestionNumber &&
                   QuestionTitle == answer.QuestionTitle &&
                   AnswerNumber == answer.AnswerNumber &&
                   AnswerTitle == answer.AnswerTitle &&
                   AnswerText == answer.AnswerText;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PollNumber, QuestionNumber, QuestionTitle, AnswerNumber, AnswerTitle, AnswerText);
        }
    }
}
