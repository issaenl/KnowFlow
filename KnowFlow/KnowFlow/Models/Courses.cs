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
    [Table("Courses")]
    public class Course
    {
        [Key]
        [Column("courseID")]
        public int CourseId { get; set; }
        [Column("courseName")]
        public string CourseName { get; set; }
        [Column("courseDescription")]
        public string CourseDescription { get; set; }
        [Column("color")]
        public string Color { get; set; }

        [ForeignKey("User")]
        [Column("curatorId")]
        public int CuratorId { get; set; }
        [Column("curatorName")]
        public string CuratorName { get; set; }

        public ObservableCollection<MaterialSection> MaterialSections { get; set; }
        public ObservableCollection<Test> Tests { get; set; }
        public ObservableCollection<Notice> Notices { get; set; }
    }
}
