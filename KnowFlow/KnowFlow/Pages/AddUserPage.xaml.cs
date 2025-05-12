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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KnowFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddUserPage.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {
        private UserData userData;
        public AddUserPage()
        {
            InitializeComponent();
            userData = new UserData();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = userData.LoadUsers();
            UsersDataGrid.ItemsSource = users;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Text;
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            Logger.Log($"Добавлен пользователь: {username}, роль: {role}");
            userData.AddUser(username, password, role);
            LoadUsers();
        }
    }
}
