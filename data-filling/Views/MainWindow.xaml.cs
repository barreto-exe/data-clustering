using data_filling.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace data_filling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string DbPath { get; set; }

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
        }
    }
}
