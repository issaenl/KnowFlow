using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("Questions")]
    public class Question
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
        public int QuestionType { get; set; }

        [Column("points")]
        public int Points { get; set; } = 1;

    }
}
