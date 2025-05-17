using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("AnswerSelections")]
    public class AnswerSelection
    {
        [Key]
        [Column("selectionId")]
        public int SelectionId { get; set; }
        [ForeignKey("QuestionResult")]
        [Column("questionResultId")]
        public int QuestionResultId { get; set; }
        [ForeignKey("Answer")]
        [Column("answerId")]
        public int? AnswerId { get; set; }
        [Column("answerText")]
        public string? AnswerText { get; set; }

        public virtual QuestionResult QuestionResult { get; set; }
        public virtual Answer Answer { get; set; }
    }
}
