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
    [Table("Questions")]
    public class Question : INotifyPropertyChanged
    {
        [Key]
        [Column("questionId")]
        public int QuestionId { get; set; }

        [ForeignKey("Test")]
        [Column("testId")]
        public int TestId { get; set; }

        [Column("questionText")]
        public string QuestionText { get; set; }

        [Column("questionType")]
        private int questionType;
        public int QuestionType
        {
            get => questionType;
            set
            {
                if (questionType != value)
                {
                    questionType = value;
                    OnPropertyChanged(nameof(QuestionType));
                }
            }
        }

        [Column("points")]
        public int Points { get; set; } = 1;

        public virtual Test Test { get; set; }
        public ObservableCollection<Answer> Answers { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
