using KnowFlow.Models;
using KnowFlow.Pages.Сlass;
using KnowFlow.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace KnowFlow.Pages
{
    public partial class MainCoursesPage : Page
    {
        public ObservableCollection<Course> Courses { get; } = new ObservableCollection<Course>();
        private readonly UserData _userData = new UserData();
        private readonly int _curatorId;
        private readonly string _currentUser;

        public MainCoursesPage(string currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            CoursesTiles.ItemsSource = Courses;
            ClassesList.ItemsSource = Courses;
            _curatorId = _userData.GetUserIdByUsername(currentUser);
            LoadCourses();
        }

        public void LoadCourses()
        {
            try
            {
                var courses = _userData.LoadUserCourses(_curatorId);
                Courses.Clear();
                foreach (var course in courses)
                {
                    Courses.Add(course);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Course course)
            {
                if (Window.GetWindow(this) is MainAppWindow mainWindow)
                {
                    bool isUser = mainWindow.IsUser;
                    var coursePage = new CoursePage(course, _currentUser, isUser);
                    mainWindow.MainFrame.Navigate(coursePage);
                    mainWindow.AddClassButton.IsEnabled = false;
                }
            }
        }
    }
}
