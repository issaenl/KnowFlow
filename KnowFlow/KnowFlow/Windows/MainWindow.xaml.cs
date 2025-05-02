using KnowFlow.Pages.Сlass;
using KnowFlow.Windows;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KnowFlow
{
    public partial class MainWindow : Window
    {
        private readonly UserData userData = new UserData();
        private bool isPasswordVisible = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AdminPasswordWindow adminPasswordWindow = new AdminPasswordWindow
            {
                Owner = this
            };

            if (adminPasswordWindow.ShowDialog() == true)
            {
                string login = adminPasswordWindow.Login;
                string password = adminPasswordWindow.Password;

                bool isFixedAdmin = (login == "admin" && password == "12345678");
                bool isDatabaseAdmin = userData.IsAdmin(login, password);

                if (isFixedAdmin || isDatabaseAdmin)
                {
                    MessageBox.Show("Авторизация пройдена!");
                    var adminMainWindow = new AdminMainWindow();
                    Close();
                    adminMainWindow.Show();
                }
                else
                {
                    MessageBox.Show("Неверный пароль или логин!");
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = loginBox.Text;
            string password = passwordBox.Password;

            bool isUser = userData.VerifyUser(username, password);
            bool isCurator = userData.VerifyCurator(username, password);

            if (isUser || isCurator)
            {
                MessageBox.Show("Авторизация пройдена!");
                //var curatorMainWindow = new CuratorMainWindow(username, isUser);
                //Close();
                //curatorMainWindow.Show();
            }
            else
            {
                MessageBox.Show("Неверный пароль или логин!");
            }
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                passwordTextBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                passwordTextBox.Visibility = Visibility.Visible;
                showPasswordButton.Content = "-";
            }
            else
            {
                passwordBox.Password = passwordTextBox.Text;
                passwordTextBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                showPasswordButton.Content = "*";
            }
        }
    }
}