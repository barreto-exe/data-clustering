using data_filling.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace data_filling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string DbPath { get; set; }

        private string namesJson = "[\r\n  \"Castro, Stone W.\",\r\n  \"Shoshana X Sebastian\",\r\n  \"Daria Y Fernandez\",\r\n  \"Jonas O Paz\",\r\n  \"Lilah Juan\",\r\n  \"Sepulveda\",\r\n  \"Perez\",\r\n  \"Zephr Pia\",\r\n  \"Guy P Hernandez\",\r\n  \"Bentlee, Hilel T.\",\r\n  \"Chiquita Figueroa\",\r\n  \"Fernandez, Stacey V.\"]";

        private string virtuesJson = "[\r\n  \"es disciplinado.\",\r\n  \"bueno amigo\",\r\n  \"Carismático\",\r\n  \"sociable\",\r\n  \"persona responsable\"]\r\n";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
            {
                DbPath = openFileDialog.FileName;
                LblPath.Content = DbPath;
            }
        }

        private async void BtnInsertar_Click(object sender, RoutedEventArgs e)
        {
            await InsertPollRow();
        }
        private async Task InsertPollRow()
        {
            int sexo = (bool)BtnFemenino.IsChecked ? 1 : 2;
            int semestre = Convert.ToInt32(((ComboBoxItem)CbSemestre.SelectedItem).DataContext);
            int carrera = Convert.ToInt32(((ComboBoxItem)CbCarrera.SelectedItem).DataContext);
            int conoceEIU = (bool)BtnSiConoce.IsChecked ? 1 : 2;
            int actividadEIU = Convert.ToInt32(((ComboBoxItem)CbActividad.SelectedItem).DataContext);

            var cualidades = new List<string>()
            {
                TextCualidad1.Text,
                TextCualidad2.Text,
                TextCualidad3.Text,
            };

            var postulados = new List<string>()
            {
                TextEstudiante1.Text,
                TextEstudiante2.Text,
                TextEstudiante3.Text,
            };

            Poll poll = new(sexo, semestre, carrera, conoceEIU, actividadEIU, cualidades, postulados);

            await poll.InsertDbRow(DbPath);

            ResetAllControls(StkMain);
        }

        private async void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            await FillRandomAnswers();
        }
        private async Task FillRandomAnswers()
        {
            Random random = new();

            CbSemestre.SelectedIndex = random.Next(0, 10);
            CbCarrera.SelectedIndex = random.Next(0, 14);
            CbActividad.SelectedIndex = random.Next(0, 8);
            if (random.Next(1, 3) == 1)
            {
                BtnFemenino.IsChecked = true;
            }
            else
            {
                BtnMasculino.IsChecked = true;
            }
            if (random.Next(1, 3) == 1)
            {
                BtnSiConoce.IsChecked = true;
            }
            else
            {
                BtnNoConoce.IsChecked = true;
            }

            var virtuesList = JsonConvert.DeserializeObject<string[]>(virtuesJson);
            int virtues = virtuesList?.Length ?? 0;

            var namesList = JsonConvert.DeserializeObject<string[]>(namesJson);
            int names = namesList?.Length ?? 0;

            TextCualidad1.Text = Randomize(virtuesList[random.Next(0, virtues)]);
            TextCualidad2.Text = Randomize(virtuesList[random.Next(0, virtues)]);
            TextCualidad3.Text = Randomize(virtuesList[random.Next(0, virtues)]);

            TextEstudiante1.Text = Randomize(namesList[random.Next(0, names)]);
            TextEstudiante2.Text = Randomize(namesList[random.Next(0, names)]);
            TextEstudiante3.Text = Randomize(namesList[random.Next(0, names)]);
        }

        private string Randomize(string data)
        {
            Random random = new();
            bool gonnaDoIt;

            var dataArray = data.ToCharArray();

            //All "gonnaDoIt" sections determine to do
            //the messing or not by a probability of 1 out of 20.
            int probability = 21;

            for (int i = 0; i < dataArray.Length; i++)
            {
                //Randomly swaps characters
                gonnaDoIt = random.Next(1, probability) == 5;
                if (gonnaDoIt)
                {
                    int randomIndex = random.Next(0, dataArray.Length);

                    char temp = dataArray[randomIndex];

                    dataArray[randomIndex] = dataArray[i];

                    dataArray[i] = temp;
                }

                //Randomly changes characters to upper or lower
                gonnaDoIt = random.Next(1, probability) == 5;
                if (gonnaDoIt)
                {
                    int randomIndex = random.Next(0, dataArray.Length);
                    if (random.Next(1, 3) == 1)
                    {
                        dataArray[randomIndex] = dataArray[randomIndex].ToString().ToUpper()[0];
                    }
                    else
                    {
                        dataArray[randomIndex] = dataArray[randomIndex].ToString().ToLower()[0];
                    }
                }
            }

            data = new string(dataArray);

            //Randomly add spaces
            gonnaDoIt = random.Next(1, probability) == 5;
            if (gonnaDoIt)
            {
                int randomIndex = random.Next(0, dataArray.Length);

                int spacesNumber = random.Next(1, 4);

                data = data.Insert(randomIndex, new string(' ', spacesNumber));
            }

            return data;
        }

        private async void BtnCrear100_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 30; i++)
            {
                await FillRandomAnswers();
                await InsertPollRow();
            }
        }


        private void ResetAllControls(Panel mainPanel)
        {
            foreach (var item in mainPanel.Children)
            {
                if (item is Panel panel)
                {
                    ResetAllControls(panel);
                }

                if (item is Control control)
                {
                    if (control is TextBox textBox)
                    {
                        textBox.Text = null;
                    }

                    if (control is ComboBox comboBox)
                    {
                        if (comboBox.Items.Count > 0)
                            comboBox.SelectedIndex = 0;
                    }

                    if (control is RadioButton button)
                    {
                        button.IsChecked = false;
                    }
                }
            }
        }
    }
}
