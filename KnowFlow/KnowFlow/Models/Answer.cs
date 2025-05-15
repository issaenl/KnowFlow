using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KnowFlow.Models
{
    [Table("Answers")]
    public class Answer : INotifyPropertyChanged
    {
        [Key]
        [Column("answerId")]
        public int AnswerId { get; set; }

        [ForeignKey("Question")]
        [Column("questionId")]
        public int QuestionId { get; set; }

        [Required]
        [Column("answerText")]
        private string answerText;
        public string AnswerText
        {
            get => answerText;
            set
            {
                if (answerText != value)
                {
                    answerText = value;
                    OnPropertyChanged(nameof(AnswerText));
                }
            }
        }

        [Column("isCorrect")]
        private bool isCorrect;
        public bool IsCorrect
        {
            get => isCorrect;
            set
            {

                if (isCorrect != value)
                {
                    if (value && Question?.QuestionType == 1)
                    {
                        foreach (var answer in Question.Answers)
                        {
                            if (answer != this)
                            {
                                answer.IsCorrect = false;
                            }
                        }
                    }
                    isCorrect = value;
                    OnPropertyChanged(nameof(IsCorrect));
                }
            }
        }

        public Question Question { get; set; } = null!;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
