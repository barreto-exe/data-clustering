﻿using DataClustering.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

            //Get data from DB and fill pies
            DataClusterMaker maker = new(args, DbPath);
            await maker.FillAnswerList();

            var result1 = await maker.GetResult1();
            FillChart(PieChart1, result1);
            
            var result2 = await maker.GetResult2();
            FillChart(PieChart2, result2);
            
            var result3 = await maker.GetResult3();
            FillChart(PieChart3, result3);
        }

        private void FillChart(PieChart pieChart, Dictionary<string, int> values)
        {
            pieChart.Series = new SeriesCollection();
            foreach (var item in values)
            {
                pieChart.Series.Add(new PieSeries()
                {
                    Values = new ChartValues<int> { item.Value },
                    Title = item.Key,
                    DataLabels = true,
                });
            }
        }
    }
}
