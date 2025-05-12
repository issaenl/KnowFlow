using KnowFlow.Models;
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
    /// Логика взаимодействия для DeleteUserPage.xaml
    /// </summary>
    public partial class DeleteUserPage : Page
    {
        private readonly UserData _userData = new UserData();
        private User _selectedUser;

        public DeleteUserPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                UsersDataGrid.ItemsSource = _userData.LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedUser = UsersDataGrid.SelectedItem as User;

            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя из списка!");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя {_selectedUser.Username}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Logger.Log($"Удалён пользователь: {_selectedUser.Username}");
                    _userData.DeleteUser(_selectedUser.UserID); 
                    MessageBox.Show("Пользователь успешно удален!");
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}");
                }
            }
        }

        private void SearchUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameTextBox.Text;

                if (string.IsNullOrEmpty(username))
                {
                    LoadUsers();
                    return;
                }

                var users = _userData.LoadUsers()
                    .Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                UsersDataGrid.ItemsSource = users;

                if (!users.Any())
                {
                    MessageBox.Show("Пользователи с таким именем не найдены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске пользователей: {ex.Message}");
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string username = UsernameTextBox.Text;
                var allUsers = _userData.LoadUsers();

                UsersDataGrid.ItemsSource = string.IsNullOrEmpty(username)
                    ? allUsers
                    : allUsers.Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации пользователей: {ex.Message}");
            }
        }
    }
}
