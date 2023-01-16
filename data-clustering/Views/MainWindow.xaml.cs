using DataClustering.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;

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

        private async void BtnAnalizar_Click(object sender, RoutedEventArgs e)
        {
            bool dismissAltAnswers = (bool)BtnSi.IsChecked;
            double percentage = Convert.ToDouble(TxtPorcentaje.Text);

            var args = new Dictionary<string, object>
            {
                [nameof(dismissAltAnswers)] = dismissAltAnswers,
                [nameof(percentage)] = (double)percentage / 100d,
            };

            DataClusterMaker maker = new(args, DbPath);
            await maker.FillAnswerList();
        }
    }
}
