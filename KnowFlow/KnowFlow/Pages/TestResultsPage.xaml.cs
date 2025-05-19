using KnowFlow.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using KnowFlow.Pages.Сlass;

namespace KnowFlow.Pages
{
    public partial class TestResultsPage : Page
    {
        private readonly Test test;
        private readonly UserData _userData = new UserData();
        private List<TestResultDisplay> _allResults;
        private List<DateItem> _availableDates = new List<DateItem>();

        public class DateItem
        {
            public DateTime Date { get; set; }
            public string DateString => Date.ToString("dd.MM.yyyy");
        }

        public TestResultsPage(Test _test)
        {
            InitializeComponent();
            test = _test;
            LoadAllResults();
            InitializeDateComboBox();
        }

        private void LoadAllResults()
        {
            _allResults = _userData.GetTestResults(test.TestId);
            ResultsGrid.ItemsSource = _allResults;
        }

        private void InitializeDateComboBox()
        {
            _availableDates = _allResults
                .Select(r => r.CompletedAt.Date)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => new DateItem { Date = d })
                .ToList();

            DateComboBox.ItemsSource = _availableDates;
        }

        private void DateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateComboBox.SelectedItem is DateItem selectedDate)
            {
                var filteredResults = _allResults
                    .Where(r => r.CompletedAt.Date == selectedDate.Date)
                    .ToList();

                ResultsGrid.ItemsSource = filteredResults;
            }
            else
            {
                ResultsGrid.ItemsSource = _allResults;
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DateComboBox.SelectedItem = null;
            ResultsGrid.ItemsSource = _allResults;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var resultsToExport = ResultsGrid.ItemsSource as IEnumerable<TestResultDisplay> ?? _allResults;

            var dialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "Результаты теста"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Results");
                    worksheet.Cell(1, 1).Value = "Пользователь";
                    worksheet.Cell(1, 2).Value = "Дата прохождения";
                    worksheet.Cell(1, 3).Value = "Баллы";

                    int row = 2;
                    foreach (var r in resultsToExport)
                    {
                        worksheet.Cell(row, 1).Value = r.Username;
                        worksheet.Cell(row, 2).Value = r.CompletedAt.ToString("dd.MM.yyyy HH:mm");
                        worksheet.Cell(row, 3).Value = r.TotalPoints;
                        row++;
                    }

                    workbook.SaveAs(dialog.FileName);
                    MessageBox.Show("Экспорт завершен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewAnswers_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TestResultDisplay result)
            {
                var window = new DetailedAnswersWindow(result);
                window.ShowDialog();
            }
        }
    }
}