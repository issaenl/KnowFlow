using KnowFlow.Models;
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
using System.Windows.Shapes;

namespace KnowFlow.Windows
{
    public partial class CreateNoticeWindow : Window
    {
        public ObservableCollection<TimeSpan> AvailableTimes { get; } = new ObservableCollection<TimeSpan>();
        private readonly int _courseId;
        private readonly string _createdBy;
        private Notice editingNotice = null;
        private bool isEditing => editingNotice != null;

        public Notice CreatedNotice { get; private set; }

        public string Title
        {
            get => TitleTextBox.Text;
            set => TitleTextBox.Text = value;
        }

        public string Content
        {
            get => ContentTextBox.Text;
            set => ContentTextBox.Text = value;
        }

        

        public CreateNoticeWindow(int courseId, string createdBy, Notice noticeToEdit = null)
        {
            InitializeComponent();
            _courseId = courseId;
            _createdBy = createdBy;
            DataContext = this;
            InitializeTimeOptions();

            if (noticeToEdit != null)
            {
                editingNotice = noticeToEdit;

                TitleTextBox.Text = editingNotice.Title;
                ContentTextBox.Text = editingNotice.Content;

                if (editingNotice.ExpiresAt.HasValue)
                {
                    ExpirationCheckBox.IsChecked = true;
                    ExpirationPanel.IsEnabled = true;
                    ExpirationDatePicker.SelectedDate = editingNotice.ExpiresAt.Value.Date;
                    ExpirationTimeMaskedTextBox.Text = editingNotice.ExpiresAt.Value.ToString("HH:mm");
                }
            }
        }

        private void InitializeTimeOptions()
        {
            for (int hours = 0; hours < 24; hours++)
            {
                AvailableTimes.Add(new TimeSpan(hours, 0, 0));
                AvailableTimes.Add(new TimeSpan(hours, 30, 0));
            }

            ExpirationDatePicker.SelectedDate = DateTime.Now.AddHours(1).Date;

            var defaultTime = DateTime.Now.AddHours(1).TimeOfDay;
            var closestTime = AvailableTimes.OrderBy(t => Math.Abs((t - defaultTime).Ticks)).First();
        }

        private void ExpirationCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExpirationPanel.IsEnabled = true;
        }

        private void ExpirationCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExpirationPanel.IsEnabled = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите заголовок объявления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? expiresAt = null;

            if (ExpirationCheckBox.IsChecked == true)
            {
                if (ExpirationDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату окончания", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TimeSpan selectedTime = GetSelectedTime();
                expiresAt = ExpirationDatePicker.SelectedDate.Value.Add(selectedTime);

                if (expiresAt <= DateTime.Now)
                {
                    MessageBox.Show("Дата окончания должна быть в будущем", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (isEditing)
            {
                editingNotice.Title = TitleTextBox.Text;
                editingNotice.Content = ContentTextBox.Text;
                editingNotice.ExpiresAt = expiresAt;
                CreatedNotice = editingNotice;
            }
            else
            {
                CreatedNotice = new Notice
                {
                    Title = TitleTextBox.Text,
                    Content = ContentTextBox.Text,
                    CreatedAt = DateTime.Now,
                    CreatedBy = _createdBy,
                    CourseId = _courseId,
                    ExpiresAt = expiresAt
                };
            }

            DialogResult = true;
            Close();
        }

        private TimeSpan GetSelectedTime()
        {
            if (TimeSpan.TryParse(ExpirationTimeMaskedTextBox.Text, out var parsedTime))
            {
                return parsedTime;
            }

            return DateTime.Now.TimeOfDay; 
        }
    }
}
