using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    [Table("MaterialFiles")]
    public class MaterialFile
    {
        [Key]
        [Column("fileId")]
        public int FileId { get; set; }

        [ForeignKey("CourseMaterial")]
        [Column("materialId")]
        public int MaterialId { get; set; }

        [Column("filePath")]
        public string FilePath { get; set; }

        [Column("fileName")]
        public string FileName { get; set; }

        public virtual CourseMaterial CourseMaterial { get; set; }
    }
}
