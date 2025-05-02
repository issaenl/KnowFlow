using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("CourseMaterials")]
    public class CourseMaterial
    {
        [Key]
        [Column("materialId")]
        public int MaterialId { get; set; }

        [ForeignKey("Course")]
        [Column("courseId")]
        public int CourseId { get; set; }

        [Column("materialName")]
        public string MaterialName { get; set; }

        [Column("materialDescription")]
        public string MaterialDescription { get; set; }

        [ForeignKey("MaterialSection")]
        [Column("sectionId")]
        public int? SectionId { get; set; }
    }
}
