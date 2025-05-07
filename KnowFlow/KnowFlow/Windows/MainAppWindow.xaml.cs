using KnowFlow.Models;
using KnowFlow.Pages;
using KnowFlow.Pages.Сlass;
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

namespace KnowFlow.Windows
{
    public partial class MainAppWindow : Window
    {
        public string currentCurator;
        public int curatorId;
        UserData userData = new UserData();
        MainCoursesPage mainCoursesPage;
        public bool IsUser;

        public string AccountInitial => currentCurator?.Length > 0 ? currentCurator[0].ToString().ToUpper() : "A";
        public MainAppWindow(string currentCurator, bool isUser)
        {
            InitializeComponent();

            IsUser = isUser;

            if (IsUser)
            {
                AddClassButton.Visibility = Visibility.Collapsed;
            }

            this.currentCurator = currentCurator;
            curatorId = userData.GetUserIdByUsername(currentCurator);
            mainCoursesPage = new MainCoursesPage(currentCurator);
            MainFrame.Navigate(mainCoursesPage);
            DataContext = this;
        }

        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = new CourseEditorWindow(currentCurator);
            editor.Owner = this;
            if (editor.ShowDialog() == true && editor.Course != null)
            {
                var newCourse = new Course
                {
                    CourseName = editor.Course.CourseName,
                    CourseDescription = editor.Course.CourseDescription,
                    Color = editor.Course.Color.ToString(),
                    CuratorId = curatorId,
                    CuratorName = currentCurator
                };

                int courseId = userData.AddCourse(newCourse);

                if (courseId > 0)
                {
                    userData.EnrollUserToCourse(curatorId, courseId);

                    if (MainFrame.Content is MainCoursesPage mainPage)
                    {
                        mainPage.LoadCourses();
                    }
                }
            }
        }

        private void MainPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainCoursesPage != null)
            {
                mainCoursesPage.LoadCourses();
            }
            MainFrame.Navigate(mainCoursesPage);
            AddClassButton.IsEnabled = true;
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            var changeProfilePage = new ChangeProfilePage(currentCurator);
            MainFrame.Navigate(changeProfilePage);
            AddClassButton.IsEnabled = false;
        }
    }
}
