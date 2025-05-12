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

        public CreateTestPage()
        {
            InitializeComponent();
            Questions = new ObservableCollection<Question>();
            QuestionsListBox.ItemsSource = Questions;
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var question = new Question
            {
                QuestionText = "Новый вопрос",
                QuestionType = 1,
                Points = 1,
                Answers = new ObservableCollection<Answer>()
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
                    AnswerText = "Новый вариант ответа",
                    IsCorrect = false
                };
                question.Answers.Add(answer);
            }
        }
    }

}
