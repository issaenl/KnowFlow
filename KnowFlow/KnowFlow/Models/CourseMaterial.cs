using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

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

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [Column("createdBy")]
        public string CreatedBy { get; set; }

        [ForeignKey("MaterialSection")]
        [Column("sectionId")]
        public int? SectionId { get; set; }

        public virtual MaterialSection Section { get; set; }
        public virtual ObservableCollection<MaterialFile> Files { get; set; }
    }
}
