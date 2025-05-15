using KnowFlow.Models;
using KnowFlow.Pages.Сlass;
using KnowFlow.Windows;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KnowFlow.Pages
{
    public partial class CreateTestPage : Page
    {
        public ObservableCollection<Question> Questions { get; set; }
        public UserData userData;
        public Course _course;
        public Test currentTest;
        public string currentUser;
        public bool IsUser;
        private bool isEditMode;

        public CreateTestPage(Course course, string _currentUser, bool isUser, Test test = null)
        {
            InitializeComponent();
            Questions = new ObservableCollection<Question>();
            userData = new UserData();
            _course = course;
            currentUser = _currentUser;
            IsUser = isUser;
            QuestionsListBox.ItemsSource = Questions;

            if (test != null)
            {
                currentTest = test;
                isEditMode = true;
                LoadTestData();
            }
            else
            {
                currentTest = new Test
                {
                    CourseId = _course.CourseId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUser
                };
                isEditMode = false;
            }
        }

        private void LoadTestData()
        {
            TestTitleBox.Text = currentTest.Title;

            if (currentTest.TimeLimit.HasValue)
                TimeLimitTextBox.Text = currentTest.TimeLimit.Value.ToString();

            if (currentTest.MaxAttemps.HasValue)
                AttemptsTextBox.Text = currentTest.MaxAttemps.Value.ToString();

            if (currentTest.Questions != null)
            {
                foreach (var question in currentTest.Questions)
                {
                    Questions.Add(new Question
                    {
                        QuestionId = question.QuestionId,
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType,
                        Points = question.Points,
                        Answers = new ObservableCollection<Answer>(question.Answers.Select(a => new Answer
                        {
                            AnswerId = a.AnswerId,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect
                        }))
                    });
                }
            }
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = new Question
            {
                QuestionId = Questions.Count > 0 ? Questions.Max(q => q.QuestionId) + 1 : 1,
                QuestionType = 1,
                Points = 1,
                Answers = new ObservableCollection<Answer>(),
            };

            Questions.Add(question);
        }

        private void AddAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var question = (sender as Button).DataContext as Question;
            if (question != null)
            {
                var answer = new Answer
                {
                    IsCorrect = false,
                    Question = question
                };
                question.Answers.Add(answer);
            }
        }

        private void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = (sender as Button).DataContext as Question;
            if (question != null && Questions.Contains(question))
            {
                Questions.Remove(question);
            }
        }

        private void DeleteAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var answer = (sender as Button).DataContext as Answer;
            if (answer != null)
            {
                foreach (var question in Questions)
                {
                    if (question.Answers.Contains(answer))
                    {
                        question.Answers.Remove(answer);
                        break;
                    }
                }
            }
        }

        private void SaveTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TestTitleBox.Text))
            {
                MessageBox.Show("Введите название теста");
                return;
            }

            int? timeLimit = null;
            if (!string.IsNullOrWhiteSpace(TimeLimitTextBox.Text))
            {
                if (!int.TryParse(TimeLimitTextBox.Text, out var parsedTimeLimit) || parsedTimeLimit <= 0)
                {
                    MessageBox.Show("Укажите корректное время выполнения (положительное число) или оставьте поле пустым");
                    return;
                }
                timeLimit = parsedTimeLimit;
            }

            int? maxAttempts = null;
            if (!string.IsNullOrWhiteSpace(AttemptsTextBox.Text))
            {
                if (!int.TryParse(AttemptsTextBox.Text, out var parsedMaxAttempts) || parsedMaxAttempts <= 0)
                {
                    MessageBox.Show("Укажите корректное количество попыток (положительное число) или оставьте поле пустым");
                    return;
                }
                maxAttempts = parsedMaxAttempts;
            }

            if (Questions.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один вопрос");
                return;
            }

            try
            {
                ValidateQuestions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                currentTest.Title = TestTitleBox.Text;
                currentTest.TimeLimit = timeLimit;
                currentTest.MaxAttemps = maxAttempts;
                currentTest.Questions = new ObservableCollection<Question>(Questions);

                if (isEditMode)
                {
                    userData.UpdateTest(currentTest);
                    MessageBox.Show("Тест успешно обновлен");
                }
                else
                {
                    userData.SaveTest(currentTest);
                    MessageBox.Show("Тест успешно сохранен");
                }

                if (Window.GetWindow(this) is MainAppWindow mainWindow)
                {
                    var coursePage = new CoursePage(_course, currentUser, IsUser);
                    mainWindow.MainFrame.Navigate(coursePage);
                    mainWindow.AddClassButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении теста: {ex.Message}");
            }
        }

        private void ValidateQuestions()
        {
            foreach (var question in Questions)
            {
                if (string.IsNullOrWhiteSpace(question.QuestionText))
                {
                    throw new Exception("Все вопросы должны содержать текст");
                }

                if (question.QuestionType == 0)
                {
                    foreach (var answer in question.Answers)
                    {
                        answer.IsCorrect = true;
                    }
                }

                if (question.Answers.Count == 0)
                {
                    throw new Exception($"Вопрос '{question.QuestionText}' должен содержать варианты ответов");
                }

                if (question.QuestionType != 0)
                {
                    foreach (var answer in question.Answers)
                    {
                        if (string.IsNullOrWhiteSpace(answer.AnswerText))
                        {
                            throw new Exception($"Все варианты ответов должны содержать текст (вопрос: '{question.QuestionText}')");
                        }
                    }

                    var correctCount = question.Answers.Count(a => a.IsCorrect);
                    switch (question.QuestionType)
                    {
                        case 1 when correctCount != 1:
                            throw new Exception($"Вопрос с одним верным вариантом ('{question.QuestionText}') должен иметь ровно один правильный ответ");
                        case 2 when correctCount < 1:
                            throw new Exception($"Вопрос с несколькими верными вариантами ('{question.QuestionText}') должен иметь хотя бы один правильный ответ");
                    }
                }
            }
        }
    }
}
