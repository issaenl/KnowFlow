using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("Notice")]
    public class Notice
    {
        [Key]
        [Column("noticeID")]
        public int NoticeId { get; set; }

        [Column("title")]
        public string Title { get; set; }
        [Column("content")]
        public string Content { get; set; }
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }
        [Column("createdBy")]
        public string CreatedBy { get; set; }
        [Column("expiresAt")]
        public DateTime? ExpiresAt { get; set; }


        [ForeignKey("Courses")]
        [Column("courseId")]
        public int CourseId { get; set; }

        public virtual Course Course { get; set; }
    }
}
