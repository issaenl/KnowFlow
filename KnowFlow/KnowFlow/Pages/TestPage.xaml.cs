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
using KnowFlow.ViewModels;

namespace KnowFlow.Pages
{
    public partial class TestPage : Page, INotifyPropertyChanged
    {
        private readonly UserTestSession _userSession = new UserTestSession();
        private readonly string _currentUser;
        private int _userId;
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
            _currentUser = currentUser;
            _userId = _userData.GetUserIdByUsername(currentUser);
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
            this.Unloaded += TestPage_Unloaded;
        }

        private void TestPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            this.Unloaded -= TestPage_Unloaded;
            timer.Tick -= Timer_Tick;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveTestResults();
        }

        private void SaveTestResults()
        {
            if (!Questions.Any()) return;

            var testResult = new TestResult
            {
                TestId = _test.TestId,
                UserId = _userId,
                AttemptNumber = _userData.GetUserTestAttemptNumber(_userId, _test.TestId),
                StartedAt = DateTime.Now.AddMinutes(-_test.TimeLimit ?? 0).AddSeconds(timeRemaining.TotalSeconds),
                FinishedAt = DateTime.Now,
                QuestionResults = new ObservableCollection<QuestionResult>()
            };

            int totalScore = 0;

            foreach (var question in Questions)
            {
                var questionResult = new QuestionResult
                {
                    QuestionId = question.QuestionId,
                    AnswerSelections = new ObservableCollection<AnswerSelection>(),
                    PointsEarned = 0,
                    IsCorrect = false
                };

                var correctAnswers = question.Answers.Where(a => a.IsCorrect).ToList();
                var correctAnswerIds = correctAnswers.Select(a => a.AnswerId).ToList();

                // Проверка ответов через UserTestSession
                switch (question.QuestionType)
                {
                    case 1: // Один выбор
                        if (_userSession.SelectedAnswers.TryGetValue(question.QuestionId, out var selectedIds) && selectedIds.Any())
                        {
                            var selectedAnswerId = selectedIds.First();
                            var selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);

                            if (selectedAnswer != null)
                            {
                                questionResult.AnswerSelections.Add(new AnswerSelection
                                {
                                    AnswerId = selectedAnswer.AnswerId,
                                    AnswerText = selectedAnswer.AnswerText
                                });

                                questionResult.IsCorrect = correctAnswerIds.Contains(selectedAnswerId);
                                Debug.WriteLine($"Выбран один ответ: {selectedAnswerId}");
                            }
                        }
                        break;

                    case 2: // Множественный выбор
                        if (_userSession.SelectedAnswers.TryGetValue(question.QuestionId, out var multipleSelectedIds))
                        {
                            foreach (var answerId in multipleSelectedIds)
                            {
                                var answer = question.Answers.FirstOrDefault(a => a.AnswerId == answerId);
                                if (answer != null)
                                {
                                    questionResult.AnswerSelections.Add(new AnswerSelection
                                    {
                                        AnswerId = answer.AnswerId,
                                        AnswerText = answer.AnswerText
                                    });
                                }
                            }

                            questionResult.IsCorrect = correctAnswerIds.All(id => multipleSelectedIds.Contains(id)) &&
                                                   multipleSelectedIds.All(id => correctAnswerIds.Contains(id));
                            Debug.WriteLine($"Выбраны ответы: {string.Join(", ", multipleSelectedIds)}");
                        }
                        break;

                    case 0: // Текстовый ответ
                        if (_userSession.TextAnswers.TryGetValue(question.QuestionId, out var textAnswer))
                        {
                            questionResult.AnswerSelections.Add(new AnswerSelection
                            {
                                AnswerText = textAnswer
                            });

                            questionResult.IsCorrect = correctAnswers
                                .Any(ca => string.Equals(
                                    ca.AnswerText?.Trim(),
                                    textAnswer?.Trim(),
                                    StringComparison.OrdinalIgnoreCase));
                            Debug.WriteLine($"Текстовый ответ: '{textAnswer}'");
                        }
                        break;
                }

                questionResult.PointsEarned = questionResult.IsCorrect ? question.Points : 0;
                totalScore += questionResult.PointsEarned;

                testResult.QuestionResults.Add(questionResult);
            }

            testResult.Score = totalScore;
            _userData.SaveTestResult(testResult);

            MessageBox.Show($"Тест завершен!\nВаш результат: {totalScore} из {Questions.Sum(q => q.Points)}",
                           "Результат теста",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        // Обработчик для RadioButton (один выбор)
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.DataContext is Answer answer)
            {
                int questionId = answer.QuestionId;
                int answerId = answer.AnswerId;
                _userSession.AddSelectedAnswer(questionId, answerId);
                Debug.WriteLine($"Выбран ответ: QuestionID={questionId}, AnswerID={answerId}");
            }
        }

        // Обработчик для CheckBox (множественный выбор)
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Answer answer)
            {
                int questionId = answer.QuestionId;
                int answerId = answer.AnswerId;
                _userSession.AddSelectedAnswer(questionId, answerId);
                Debug.WriteLine($"Добавлен ответ: QuestionID={questionId}, AnswerID={answerId}");
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Answer answer)
            {
                int questionId = answer.QuestionId;
                int answerId = answer.AnswerId;
                _userSession.RemoveSelectedAnswer(questionId, answerId);
                Debug.WriteLine($"Удалён ответ: QuestionID={questionId}, AnswerID={answerId}");
            }
        }

        // Обработчик для TextBox (текстовый ответ)
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is Question question)
            {
                int questionId = question.QuestionId;
                string answerText = textBox.Text;
                _userSession.AddTextAnswer(questionId, answerText);
                Debug.WriteLine($"Текстовый ответ: QuestionID={questionId}, Text='{answerText}'");
            }
        }

    }

}
