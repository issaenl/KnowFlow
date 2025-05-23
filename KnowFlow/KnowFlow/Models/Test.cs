﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

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

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("createdBy")]
        public string CreatedBy { get; set; }

        public ObservableCollection<Question> Questions { get; set; } = new();
    }
}
