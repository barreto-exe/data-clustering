using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Windows;

namespace data_filling.Models
{
    public class Poll
    {
        public int Sexo { get; set; }
        public int Semestre { get; set; }
        public int Carrera { get; set; }
        public int ConoceEIU { get; set; }
        public int ActividadEIU { get; set; }
        public IEnumerable<string> Cualidades { get; set; }
        public IEnumerable<string> Postulados { get; set; }

        public Poll(int sexo, int semestre, int carrera, int conoceEIU, int actividadEIU, IEnumerable<string> cualidades, IEnumerable<string> postulados)
        {
            Sexo = sexo;
            Semestre = semestre;
            Carrera = carrera;
            ConoceEIU = conoceEIU;
            ActividadEIU = actividadEIU;
            Cualidades = cualidades;
            Postulados = postulados;
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

            var cualidades = Cualidades.ToArray();
            var postulados = Postulados.ToArray();

            //Insert answers
            query =
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 1, {Sexo}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 2, {Semestre}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 3, {Carrera}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 4, {ConoceEIU}, NULL);" +
                $"INSERT INTO Respuesta VALUES({idEncuesta}, 5, {ActividadEIU}, NULL);" +

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
