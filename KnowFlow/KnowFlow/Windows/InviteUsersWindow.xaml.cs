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
    /// <summary>
    /// Логика взаимодействия для InviteUsersWindow.xaml
    /// </summary>
    public partial class InviteUsersWindow : Window
    {
        private readonly int _courseId;
        private readonly UserData _userData = new UserData();

        public InviteUsersWindow(int courseId)
        {
            InitializeComponent();
            _courseId = courseId;
            LoadAvailableUsers();
        }

        private void LoadAvailableUsers()
        {
            AvailableUsersList.ItemsSource = _userData.GetAvailableUsersForCourse(_courseId);
        }

        private void InviteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                _userData.EnrollUserToCourse(userId, _courseId);
                MessageBox.Show("Пользователь успешно приглашен на курс!");
                LoadAvailableUsers();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
