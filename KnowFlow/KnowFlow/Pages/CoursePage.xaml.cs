using KnowFlow.Models;
using KnowFlow.Pages.Сlass;
using KnowFlow.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KnowFlow.Pages
{
    public partial class CoursePage : Page
    {
        private Course _course;
        private string _currentUser;
        private readonly UserData _userData = new UserData();
        public bool IsUser { get; }

        public CoursePage(Course course, string currentUser, bool isUser) 
        {
            InitializeComponent();
            _course = course;
            _currentUser = currentUser;
            CourseTitle.Text = course.CourseName;
            CourseDescription.Text = course.CourseDescription;
            IsUser = isUser;

            if (IsUser)
            {
                InviteButton.Visibility = Visibility.Collapsed;
                AddMaterialButton.Visibility = Visibility.Collapsed;
                AddSectionButton.Visibility = Visibility.Collapsed;
                AddTestButton.Visibility = Visibility.Collapsed;
                DeleteCourse.Visibility = Visibility.Collapsed;
            }

            LoadCourseData();
            LoadCourseUsers();
            LoadMaterials();
        }

        private void LoadCourseData()
        {
            CourseTitle.Text = _course.CourseName;
            CourseDescription.Text = _course.CourseDescription;
        }

        private void LoadCourseUsers()
        {
            var participants = _userData.GetCourseParticipants(_course.CourseId);

            var curators = participants.Where(u => u.UserRole == "Куратор").ToList();
            var users = participants.Where(u => u.UserRole != "Куратор").ToList();

            CuratorsList.ItemsSource = curators;
            UsersList.ItemsSource = users;
        }

        private void InviteUsers_Click(object sender, RoutedEventArgs e)
        {
            var availableUsers = _userData.GetAvailableUsersForCourse(_course.CourseId);
            var inviteWindow = new InviteUsersWindow(_course.CourseId);
            inviteWindow.ShowDialog();

            LoadCourseUsers();
        }

        private void DeleteCourseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите удалить этот курс? Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var materials = _userData.GetCourseMaterials(_course.CourseId);
                    foreach (var material in materials)
                    {
                        foreach (var file in material.Files)
                        {
                            try
                            {
                                if (File.Exists(file.FilePath))
                                {
                                    File.Delete(file.FilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Ошибка при удалении файла: {ex.Message}");
                            }
                        }
                    }

                    _userData.DeleteCourse(_course.CourseId);

                    if (Window.GetWindow(this) is MainAppWindow mainAppWindow)
                    {
                        var mainCoursesPage = new MainCoursesPage(_currentUser);
                        mainAppWindow.MainFrame.Navigate(mainCoursesPage);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении курса: {ex.Message}");
                }
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Title = "Выберите файлы материала"
            };

            if (dialog.ShowDialog() == true)
            {
                var sections = _userData.GetCourseSections(_course.CourseId);
                var addMaterialWindow = new AddMaterialWindow(
                    _course.CourseId,
                    dialog.FileNames.ToList(),
                    sections);

                if (addMaterialWindow.ShowDialog() == true)
                {
                    var newMaterial = new CourseMaterial
                    {
                        CourseId = _course.CourseId,
                        MaterialName = addMaterialWindow.MaterialName,
                        MaterialDescription = addMaterialWindow.MaterialDescription,
                        SectionId = addMaterialWindow.SelectedSectionId,
                        CreatedBy = _currentUser
                    };

                    foreach (var filePath in addMaterialWindow.SavedFilePaths)
                    {
                        newMaterial.Files.Add(new MaterialFile
                        {
                            FilePath = filePath,
                            FileName = Path.GetFileName(filePath)
                        });
                    }

                    try
                    {
                        _userData.AddCourseMaterial(newMaterial);
                        LoadMaterials(); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LoadMaterials()
        {
            try
            {
                var sections = _userData.GetCourseSections(_course.CourseId);
                var noSectionMaterials = _userData.GetMaterialsWithoutSection(_course.CourseId);

                if (noSectionMaterials.Any())
                {
                    sections.Add(new MaterialSection
                    {
                        SectionId = -1,
                        CourseId = _course.CourseId,
                        SectionName = "Без раздела",

                        Materials = noSectionMaterials
                    });
                }

                SectionsList.ItemsSource = sections;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке материалов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Material_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock textBlock && textBlock.DataContext is MaterialFile file)
            {
                try
                {
                    if (!File.Exists(file.FilePath))
                    {
                        MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            FileName = file.FilePath
                        });
                    }
                    catch
                    {
                        var saveDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            FileName = file.FileName,
                            Filter = $"Файлы (*{Path.GetExtension(file.FileName)}|*{Path.GetExtension(file.FileName)}|Все файлы (*.*)|*.*"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            File.Copy(file.FilePath, saveDialog.FileName, true);
                            MessageBox.Show("Файл успешно сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при работе с файлом: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                e.Handled = true;
            }
        }

        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            //var inputDialog = new AddSectionWindow();
            //if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            //{
            //    _userData.AddMaterialSection(_course.CourseId, inputDialog.Answer);
            //    LoadMaterials();
            //}
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            //if (sender is Button button && button.Tag is CourseMaterial material)
            //{
            //    var fullMaterial = _userData.GetMaterialById(material.MaterialId);
            //    if (fullMaterial == null) return;

            //    var sections = _userData.GetCourseSections(_course.CourseId);
            //    var editWindow = new AddMaterialWindow(
            //        _course.CourseId,
            //        fullMaterial.Files.Select(f => f.FilePath).ToList(),
            //        sections)
            //    {
            //        MaterialName = fullMaterial.MaterialName,
            //        MaterialDescription = fullMaterial.MaterialDescription,
            //        SelectedSectionId = fullMaterial.SectionId
            //    };

            //    if (editWindow.ShowDialog() == true)
            //    {
            //        fullMaterial.MaterialName = editWindow.MaterialName;
            //        fullMaterial.MaterialDescription = editWindow.MaterialDescription;
            //        fullMaterial.SectionId = editWindow.SelectedSectionId;

            //        fullMaterial.Files.Clear();
            //        foreach (var filePath in editWindow.SavedFilePaths)
            //        {
            //            fullMaterial.Files.Add(new MaterialFile
            //            {
            //                FilePath = filePath,
            //                FileName = Path.GetFileName(filePath)
            //            });
            //        }

            //        _userData.UpdateCourseMaterial(fullMaterial);
            //        LoadMaterials();
            //    }
            //}
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            //if (sender is Button button && button.Tag is CourseMaterial material)
            //{
            //    MessageBoxResult result = MessageBox.Show(
            //        $"Вы уверены, что хотите удалить материал \"{material.MaterialName}\"?",
            //        "Подтверждение удаления",
            //        MessageBoxButton.YesNo,
            //        MessageBoxImage.Warning);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        try
            //        {
            //            // Удаление файлов материала
            //            foreach (var file in material.Files)
            //            {
            //                try
            //                {
            //                    if (File.Exists(file.FilePath))
            //                    {
            //                        File.Delete(file.FilePath);
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    Debug.WriteLine($"Ошибка при удалении файла: {ex.Message}");
            //                }
            //            }

            //            // Удаление материала из БД
            //            _userData.DeleteMaterial(material.MaterialId);
            //            LoadMaterials();

            //            MessageBox.Show("Материал успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show($"Ошибка при удалении материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //        }
            //    }
            //}
        }

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.ContextMenu.IsEnabled = true;
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}