using KnowFlow.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Data
{
    public class KnowFlowDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<MaterialFile> MaterialFiles { get; set; }
        public DbSet<MaterialSection> MaterialSections { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<QuestionResult> QuestionResults { get; set; }
        public DbSet<AnswerSelection> AnswerSelections { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("Server=localhost;Port=3306;Database=KnowFlow;Uid=root;Pwd=1111;",
                new MySqlServerVersion(new Version(8, 0, 23)));
        }


    }
}
