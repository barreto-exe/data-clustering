using DataClustering.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataClustering.Views
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

        private void BtnFindFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
            {
                DbPath = openFileDialog.FileName;
                LblPath.Content = DbPath;
            }
        }

        private void BtnAnalizar_Click(object sender, RoutedEventArgs e)
        {
            bool dismissAltAnswers = (bool)BtnSi.IsChecked;
            double percentage = Convert.ToDouble(TxtPorcentaje.Text);

            var args = new Dictionary<string, object>
            {
                [nameof(dismissAltAnswers)] = dismissAltAnswers,
                [nameof(percentage)] = (double)percentage / 100d,
            };

            DataClusterMaker maker = new(args, DbPath);
        }
    }
}
