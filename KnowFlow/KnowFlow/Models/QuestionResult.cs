using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("QuestionResults")]
    public class QuestionResult
    {
        [Key]
        [Column("questionResultId")]
        public int QuestionResultId { get; set; }
        [ForeignKey("TestResult")]
        [Column("resultId")]
        public int ResultId { get; set; }
        [ForeignKey("Question")]
        [Column("questionId")]
        public int QuestionId { get; set; }
        [Column("isCorrect")]
        public bool IsCorrect { get; set; }
        [Column("pointsEarned")]
        public int PointsEarned { get; set; }

        public virtual TestResult TestResult { get; set; }
        public virtual Question Question { get; set; }
        public virtual ObservableCollection<AnswerSelection> AnswerSelections { get; set; }
    }
}
