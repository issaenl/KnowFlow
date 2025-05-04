using KnowFlow.Pages.Сlass;
using KnowFlow.Windows;
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

namespace KnowFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangeProfilePage.xaml
    /// </summary>
    public partial class ChangeProfilePage : Page
    {
        private string currentUsername;
        private UserData userData = new UserData();

        public string CurrentUsername { get; set; }

        public ChangeProfilePage(string username)
        {
            InitializeComponent();
            currentUsername = username;
            CurrentUsername = username;
            DataContext = this;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = UsernameTextBox.Text.Trim();
            string newPassword = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                MessageBox.Show("Имя не может быть пустым!");
                return;
            }

            if (!string.IsNullOrEmpty(newPassword) && newPassword != confirmPassword)
            {
                MessageBox.Show("Пароль не совпадает!");
                return;
            }

            bool usernameChanged = newUsername != currentUsername;
            bool passwordChanged = !string.IsNullOrEmpty(newPassword);

            if (usernameChanged && userData.UsernameExists(newUsername))
            {
                MessageBox.Show("Такое имя уже занято!");
                return;
            }
            else
            {
                if (userData.UpdateUser(currentUsername, newUsername, passwordChanged ? newPassword : null))
                {
                    currentUsername = newUsername;
                    MessageBox.Show("Профиль успешно обнавлен!");

                    if (Window.GetWindow(this) is MainAppWindow mainWindow)
                    {
                        mainWindow.currentCurator = newUsername;
                        var coursePage = new MainCoursesPage(newUsername);
                        mainWindow.MainFrame.Navigate(coursePage);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка обновления профиля!");
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();

            if (Window.GetWindow(this) is MainAppWindow mainWindow)
            {
                mainWindow.Close();
            }
        }
    }
}
