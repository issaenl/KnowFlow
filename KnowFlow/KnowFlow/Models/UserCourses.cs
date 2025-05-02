using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("UserCourses")]
    public class UserCourse
    {
        [Key]
        [Column("userCourseId")]
        public int UserCourseId { get; set; }

        [ForeignKey("User")]
        [Column("userId")]
        public int UserId { get; set; }

        [ForeignKey("Course")]
        [Column("courseId")]
        public int CourseId { get; set; }
    }
}
