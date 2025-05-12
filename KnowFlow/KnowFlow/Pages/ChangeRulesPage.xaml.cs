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
    public partial class ChangeRulesPage : Page
    {
        private readonly UserData _userData = new UserData();
        private User _selectedUser;

        public ChangeRulesPage()
        {
            InitializeComponent();
            InitializeRoleComboBox();
            LoadUsers();
        }

        private void InitializeRoleComboBox()
        {
            RoleComboBox.ItemsSource = new List<string> { "Администратор", "Куратор", "Пользователь" };
        }

        private void LoadUsers()
        {
            try
            {
                UsersDataGrid.ItemsSource = _userData.LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void ChangeUserRoleButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedUser = UsersDataGrid.SelectedItem as User;

            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя из списка!");
                return;
            }

            if (!(RoleComboBox.SelectedItem is string newRole))
            {
                MessageBox.Show("Выберите новую роль!");
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Изменить роль пользователя {_selectedUser.Username} на {newRole}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    Logger.Log($"Изменена роль пользователя: {_selectedUser.Username} -> {newRole}");
                    _userData.ChangeRole(_selectedUser.UserID, newRole);
                    MessageBox.Show("Роль успешно изменена!");
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedUser = UsersDataGrid.SelectedItem as User;

            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя!");
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Удалить пользователя {_selectedUser.Username}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    _userData.DeleteUser(_selectedUser.UserID);
                    MessageBox.Show("Пользователь удален!");
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = UsernameTextBox.Text;
            var allUsers = _userData.LoadUsers();

            UsersDataGrid.ItemsSource = string.IsNullOrEmpty(searchText)
                ? allUsers
                : allUsers.Where(u => u.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }
    }
}
