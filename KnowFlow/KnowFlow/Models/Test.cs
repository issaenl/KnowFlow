using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("Tests")]
    public class Test
    {
        [Key]
        [Column("testId")]
        public int TestId { get; set; }

        [ForeignKey("Course")]
        [Column("courseId")]
        public int CourseId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("timeLimit")]
        public int? TimeLimit { get; set; }

        [Column("maxAttemps")]
        public int? MaxAttemps { get; set; }

    }
}
