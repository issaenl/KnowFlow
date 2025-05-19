using KnowFlow.Models;
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
using System.Windows.Shapes;
using System.IO;
using KnowFlow.Pages.Сlass;
using System.Collections.ObjectModel;

namespace KnowFlow.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddMaterialWindow.xaml
    /// </summary>
    public partial class AddMaterialWindow : Window
    {
        public List<MaterialSection> Sections { get; set; }
        public ObservableCollection<string> SavedFilePaths { get; private set; } = new ObservableCollection<string>();
        private readonly List<string> _filePaths = new List<string>();

        public string MaterialName
        {
            get => MaterialNameTextBox.Text;
            set => MaterialNameTextBox.Text = value;
        }

        public string MaterialDescription
        {
            get => DescriptionTextBox.Text;
            set => DescriptionTextBox.Text = value;
        }

        public int? SelectedSectionId
        {
            get => SectionComboBox.SelectedValue as int?;
            set => SectionComboBox.SelectedValue = value;
        }

        public AddMaterialWindow(int courseId, List<string> filePaths, List<MaterialSection> sections)
        {
            InitializeComponent();
            _filePaths = filePaths;
            Sections = sections;
            SectionComboBox.ItemsSource = Sections;
            SectionComboBox.DisplayMemberPath = "SectionName";
            SectionComboBox.SelectedValuePath = "SectionId";
            SectionComboBox.SelectedItem = Sections.FirstOrDefault(s => s.SectionName == "Без раздела");
            UpdateFileList();
        }

        private void UpdateFileList()
        {
            FilesListBox.Items.Clear();
            foreach (var filePath in _filePaths)
            {
                FilesListBox.Items.Add(System.IO.Path.GetFileName(filePath));
            }
        }

        private void AddMoreFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Title = "Выберите файлы материала"
            };

            if (dialog.ShowDialog() == true)
            {
                _filePaths.AddRange(dialog.FileNames);
                UpdateFileList();
            }
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListBox.SelectedIndex != -1)
            {
                _filePaths.RemoveAt(FilesListBox.SelectedIndex);
                UpdateFileList();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MaterialNameTextBox.Text))
            {
                MessageBox.Show("Введите название материала");
                return;
            }

            if (_filePaths.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один файл");
                return;
            }

            MaterialName = MaterialNameTextBox.Text;
            MaterialDescription = DescriptionTextBox.Text;

            if (SectionComboBox.SelectedItem != null)
            {
                SelectedSectionId = (int)SectionComboBox.SelectedValue;
            }

            try
            {
                string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                string materialsPath = System.IO.Path.Combine(projectPath, "materials");

                if (!Directory.Exists(materialsPath))
                {
                    Directory.CreateDirectory(materialsPath);
                }

                SavedFilePaths.Clear();

                foreach (var filePath in _filePaths)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string newFilePath = System.IO.Path.Combine(materialsPath, fileName);

                    try
                    {
                        if (!File.Exists(newFilePath))
                        {
                            File.Copy(filePath, newFilePath);
                        }
                        SavedFilePaths.Add(newFilePath);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Ошибка при копировании файла {fileName}: {ex.Message}");
                        continue;
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файлов: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
