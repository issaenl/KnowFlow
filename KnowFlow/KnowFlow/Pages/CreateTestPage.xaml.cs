using KnowFlow.Models;
using KnowFlow.Pages.Сlass;
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

        public CreateTestPage(Course course)
        {
            InitializeComponent();
            Questions = new ObservableCollection<Question>();
            userData = new UserData();
            _course = course;
            QuestionsListBox.ItemsSource = Questions;
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = new Question
            {
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
                var test = new Test
                {
                    CourseId = _course.CourseId,
                    Title = TestTitleBox.Text,
                    TimeLimit = timeLimit,
                    MaxAttemps = maxAttempts,
                    Questions = new ObservableCollection<Question>(Questions)
                };

                userData.SaveTest(test);
                MessageBox.Show("Тест успешно сохранен");
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
