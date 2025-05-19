using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KnowFlow.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("userID")]
        public int UserID { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("userPassword")]
        public string UserPassword { get; set; }

        [Column("userRole")]
        public string UserRole { get; set; }

        public ObservableCollection<TestResult> Results { get; set; }
    }

}
