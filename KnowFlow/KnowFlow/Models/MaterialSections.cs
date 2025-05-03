using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("MaterialSections")]
    public class MaterialSection
    {
        [Key]
        [Column("sectionId")]
        public int SectionId { get; set; }

        [ForeignKey("Course")]
        [Column("courseId")]
        public int CourseId { get; set; }

        [Column("sectionName")]
        public string SectionName { get; set; }

        public virtual List<CourseMaterial> Materials { get; set; } = new();
    }
}
