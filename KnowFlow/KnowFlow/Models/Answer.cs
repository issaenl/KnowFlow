using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace KnowFlow.Models
{
    [Table("Answers")]
    public class Answer
    {
        [Key]
        [Column("answerId")]
        public int AnswerId { get; set; }

        [ForeignKey("Question")]
        [Column("questionId")]
        public int QuestionId { get; set; }

        [Column("answerText")]
        public string AnswerText { get; set; }

        [Column("isCorrect")]
        public bool IsCorrect { get; set; } = false;
    }
}
