using KnowFlow.Models;
using KnowFlow.Pages.Сlass;
using KnowFlow.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private System.Windows.Threading.DispatcherTimer _noticesTimer;

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
                AddNoticeButton.Visibility = Visibility.Collapsed;
                DeleteCourse.Visibility = Visibility.Collapsed;
            }

            LoadCourseData();
            LoadCourseUsers();
            LoadMaterials();
            LoadNotices();
            LoadTests();

            _noticesTimer = new System.Windows.Threading.DispatcherTimer();
            _noticesTimer.Interval = TimeSpan.FromMinutes(1);
            _noticesTimer.Tick += (s, e) => LoadNotices();
            _noticesTimer.Start();
            this.Unloaded += (s, e) => _noticesTimer.Stop();

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
                        mainAppWindow.AddClassButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении курса: {ex.Message}");
                }
            }
        }

        private void QuitCourseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
            $"Вы действительно хотите покинуть курс \"{_course.CourseName}\"?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int currentUserId = _userData.GetUserIdByUsername(_currentUser);
                    _userData.RemoveUserFromCourse(currentUserId, _course.CourseId);

                    MessageBox.Show("Вы покинули курс.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (Window.GetWindow(this) is MainAppWindow mainAppWindow)
                    {
                        var mainPage = new MainCoursesPage(_currentUser);
                        mainAppWindow.MainFrame.Navigate(mainPage);
                        mainAppWindow.AddClassButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выходе из курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        CreatedBy = _currentUser,
                        Files = new ObservableCollection<MaterialFile>()
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
                            FileName = file.FileName
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
            var inputDialog = new AddSectionWindow();
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                _userData.AddMaterialSection(_course.CourseId, inputDialog.Answer);
                LoadMaterials();
            }
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CourseMaterial material)
            {
                var fullMaterial = _userData.GetMaterialById(material.MaterialId);
                if (fullMaterial == null) return;

                var sections = _userData.GetCourseSections(_course.CourseId);

                var editWindow = new AddMaterialWindow(
                    _course.CourseId,
                    fullMaterial.Files.Select(f => f.FilePath).ToList(),
                    sections)
                {
                    MaterialName = fullMaterial.MaterialName,
                    MaterialDescription = fullMaterial.MaterialDescription,
                    SelectedSectionId = fullMaterial.SectionId
                };

                if (editWindow.ShowDialog() == true)
                {
                    fullMaterial.MaterialName = editWindow.MaterialName;
                    fullMaterial.MaterialDescription = editWindow.MaterialDescription;
                    fullMaterial.SectionId = editWindow.SelectedSectionId;

                    fullMaterial.Files.Clear();

                    foreach (var path in editWindow.SavedFilePaths)
                    {
                        fullMaterial.Files.Add(new MaterialFile
                        {
                            FilePath = path,
                            FileName = Path.GetFileName(path)
                        });
                    }

                    try
                    {
                        MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        _userData.UpdateCourseMaterial(fullMaterial);
                        LoadMaterials();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CourseMaterial material)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить материал \"{material.MaterialName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
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

                        _userData.DeleteMaterial(material.MaterialId);
                        LoadMaterials();

                        MessageBox.Show("Материал успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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

        private void AddNoticeButton_Click(object sender, RoutedEventArgs e)
        {
            var noticeWindow = new CreateNoticeWindow(_course.CourseId, _currentUser);

            if (noticeWindow.ShowDialog() == true && noticeWindow.CreatedNotice != null)
            {
                try
                {
                    _userData.AddNotice(noticeWindow.CreatedNotice);
                    LoadNotices();
                    MessageBox.Show("Объявление успешно добавлено!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении объявления: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadNotices()
        {
            try
            {
                var notices = _userData.GetActiveNotices(_course.CourseId);
                NoticeList.ItemsSource = notices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке объявлений: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteNotice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Notice notice)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить материал \"{notice.Title}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _userData.DeleteNotice(notice.NoticeId);
                        LoadNotices();

                        MessageBox.Show("Материал успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditNotice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Notice notice)
            {
                var fullNotice = _userData.GetNoticeById(notice.NoticeId);

                if (fullNotice == null)
                {
                    MessageBox.Show("Объявление не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var editWindow = new CreateNoticeWindow(_course.CourseId, _currentUser, fullNotice);

                if (editWindow.ShowDialog() == true)
                {
                    try
                    {
                        _userData.UpdateNotice(fullNotice);
                        MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadNotices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainAppWindow mainWindow)
            {
                var testPage = new CreateTestPage(_course, _currentUser, IsUser);
                mainWindow.MainFrame.Navigate(testPage);
                mainWindow.AddClassButton.IsEnabled = false;
            }
        }

        private void LoadTests()
        {
            try
            {
                var tests = _userData.GetCourseTests(_course.CourseId);
                TestsList.ItemsSource = tests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке тестов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditTest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Test test)
            {
                if (Window.GetWindow(this) is MainAppWindow mainWindow)
                {
                    var testPage = new CreateTestPage(_course, _currentUser, IsUser, test);
                    mainWindow.MainFrame.Navigate(testPage);
                    mainWindow.AddClassButton.IsEnabled = false;
                }
            }
        }

        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Test test)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить тест \"{test.Title}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _userData.DeleteTest(test.TestId);
                        LoadTests();
                        MessageBox.Show("Тест успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении теста: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void TestTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.DataContext is Test test)
            {
                var fullTest = _userData.GetTestById(test.TestId);

                if (fullTest == null)
                {
                    MessageBox.Show("Тест не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Window.GetWindow(this) is MainAppWindow mainWindow)
                {
                    int _userId = _userData.GetUserIdByUsername(_currentUser);
                    if (!_userData.HasAttemptsLeft(_userId, test.TestId, test.MaxAttemps))
                    {
                        MessageBox.Show("Вы исчерпали все попытки для этого теста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                        var testPage = new TestPage(fullTest, _currentUser);
                        mainWindow.MainFrame.Navigate(testPage);
                        mainWindow.AddClassButton.IsEnabled = false;
                    }
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MaterialFile file)
            {
                try
                {
                    if (!File.Exists(file.FilePath))
                    {
                        MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = file.FileName,
                        Title = "Сохранить файл"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.Copy(file.FilePath, saveDialog.FileName, true);
                            MessageBox.Show("Файл успешно сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при работе с файлом: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewResults_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Test test)
            {
                if (Window.GetWindow(this) is MainAppWindow mainWindow)
                {
                    var resultsPage = new TestResultsPage(test);
                    mainWindow.MainFrame.Navigate(resultsPage);
                    mainWindow.AddClassButton.IsEnabled = false;
                }
            }
        }
    }
}