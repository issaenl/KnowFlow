using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using KnowFlow.Models;
using KnowFlow.Pages.Сlass;

namespace KnowFlow.Pages
{
    public partial class TestPage : Page, INotifyPropertyChanged
    {
        private readonly Test _test;
        private readonly UserData _userData = new UserData();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private TimeSpan timeRemaining;

        public string TestTitle => _test?.Title;
        public string TimeLeft => _test?.TimeLimit.HasValue == true
        ? timeRemaining.ToString(@"mm\:ss") : "Нет ограничения";

        public ObservableCollection<Question> Questions { get; }

        public TestPage(Test test, string currentUser)
        {
            _test = _userData.GetTestById(test.TestId);

            if (_test == null)
            {
                Debug.WriteLine("Тест не найден!");
                return;
            }

            if (_test.Questions == null || !_test.Questions.Any())
            {
                Debug.WriteLine("В тесте нет вопросов!");
            }

            Questions = new ObservableCollection<Question>(_test.Questions);

            DataContext = this;

            if (_test.TimeLimit.HasValue)
            {
                timeRemaining = TimeSpan.FromMinutes(_test.TimeLimit.Value);

                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }

            InitializeComponent();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(TimeLeft));

            if (timeRemaining <= TimeSpan.Zero)
            {
                timer.Stop();
                MessageBox.Show("Время вышло! Тест завершён!", "Тест", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
 
}
