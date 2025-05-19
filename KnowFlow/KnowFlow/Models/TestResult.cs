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
    [Table ("TestResults")]
    public class TestResult
    {
        [Key]
        [Column ("resultId")]
        public int ResultId { get; set; }
        [ForeignKey("Test")]
        [Column("testId")]
        public int TestId { get; set; }
        [ForeignKey("User")]
        [Column("userId")]
        public int UserId { get; set; }
        [Column("attemptNumber")]
        public int AttemptNumber { get; set; }
        [Column("score")]
        public int Score { get; set; }
        [Column("startedAt")]
        public DateTime StartedAt { get; set; }
        [Column("finishedAt")]
        public DateTime FinishedAt { get; set; }

        public virtual Test Test { get; set; }
        public virtual User User { get; set; }
        public virtual ObservableCollection<QuestionResult> QuestionResults { get; set; }
    }
}
