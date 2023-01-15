using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace data_filling.Models
{
    public class Poll
    {
        public int Sex { get; set; }
        public int Semester { get; set; }
        public int Career { get; set; }
        public int KnowsProgram { get; set; }
        public int ActivitiesProgram { get; set; }
        public IEnumerable<string> Values { get; set; }
        public IEnumerable<string> Postulates { get; set; }

        public Poll(int sex, int semester, int career, int knowsProgram, int activitiesProgram, IEnumerable<string> Values, IEnumerable<string> postulates)
        {
            Sex = sex;
            Semester = semester;
            Career = career;
            KnowsProgram = knowsProgram;
            ActivitiesProgram = activitiesProgram;
            this.Values = Values;
            Postulates = postulates;
        }

        public async Task InsertDbRow(string dbPath)
        {
            string query;
            int idEncuesta;
            using SqliteConnection conn = new($"Data Source={dbPath};");

            await conn.OpenAsync();

            //Insert main row for Poll
            query =
                "INSERT INTO Encuesta " +
                "VALUES((SELECT MAX(num_encuesta) FROM Encuesta)+1,'{0}', '{1}');" +
                "SELECT MAX(num_encuesta) " +
                "FROM Encuesta";
            query = string.Format(query,
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            using (var command = new SqliteCommand(query, conn))
            {
                var reader = command.ExecuteReader();

                await reader.ReadAsync();

                idEncuesta = reader.GetInt32(0);

                await reader.CloseAsync();
            }

            var cualidades = Values.ToArray();
            var postulados = Postulates.ToArray();

            //Insert answers
            query =
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 1, {Sex}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 2, {Semester}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 3, {Career}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 4, {KnowsProgram}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 5, {ActivitiesProgram}, NULL);" +

                $"INSERT INTO Respuesta VALUES({idEncuesta}, 6, 1, '{cualidades[0]}');" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 6, 2, '{cualidades[1]}');" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 6, 3, '{cualidades[2]}');" +

                $"INSERT INTO Respuesta VALUES({idEncuesta}, 7, 1, '{postulados[0]}');" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 7, 2, '{postulados[1]}');" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 7, 3, '{postulados[2]}');";
            using (var command = new SqliteCommand(query, conn))
            {
                await command.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
        }
    }
}
