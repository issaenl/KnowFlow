using KnowFlow.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BCrypt.Net;
using System.Diagnostics;
using KnowFlow.Data;

namespace KnowFlow.Pages.Сlass
{
    public class UserData
    {
        public List<User> LoadUsers()
        {
            using var context = new KnowFlowDbContext();
            return context.Users.AsNoTracking().ToList();
        }

        private bool VerifyRole(string username, string password, string role)
        {
            using var context = new KnowFlowDbContext();
            var user = context.Users.FirstOrDefault(u => u.Username == username && u.UserRole == role);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.UserPassword);
        }

        public bool IsAdmin(string username, string password) => VerifyRole(username, password, "Администратор");
        public bool VerifyUser(string username, string password) => VerifyRole(username, password, "Пользователь");
        public bool VerifyCurator(string username, string password) => VerifyRole(username, password, "Куратор");

        public void AddUser(string username, string password, string role)
        {
            using var context = new KnowFlowDbContext();

            if (context.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Ошибка: Пользователь с таким именем уже существует!");
                return;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = username, UserPassword = hashedPassword, UserRole = role };

            context.Users.Add(user);
            context.SaveChanges();

            MessageBox.Show("Пользователь успешно добавлен!");
        }

        public void ChangeRole(int userId, string role)
        {
            using var context = new KnowFlowDbContext();
            var user = context.Users.Find(userId);
            if (user == null)
            {
                MessageBox.Show("Пользователь с таким ID не найден.");
                return;
            }
            user.UserRole = role;
            context.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            using var context = new KnowFlowDbContext();
            var user = context.Users.Find(userId);
            if (user != null)
            {
                context.Users.Remove(user);
                context.SaveChanges();
            }
            else
            {
                MessageBox.Show("Пользователь с таким ID не найден.");
            }
        }

        public int GetUserIdByUsername(string username)
        {
            using var context = new KnowFlowDbContext();
            return context.Users.FirstOrDefault(u => u.Username == username)?.UserID ?? -1;
        }

        public int AddCourse(Course course)
        {
            using var context = new KnowFlowDbContext();

            var curator = context.Users.Find(course.CuratorId);
            if (curator != null)
            {
                course.CuratorName = curator.Username;
            }

            context.Courses.Add(course);
            context.SaveChanges();
            return course.CourseId;
        }

        public List<Course> LoadUserCourses(int userId)
        {
            using var context = new KnowFlowDbContext();
            return context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Join(context.Courses,
                    uc => uc.CourseId,
                    c => c.CourseId,
                    (uc, c) => c)
                .AsNoTracking()
                .ToList();
        }

        public void EnrollUserToCourse(int userId, int courseId)
        {
            using var context = new KnowFlowDbContext();
            if (!context.UserCourses.Any(uc => uc.UserId == userId && uc.CourseId == courseId))
            {
                context.UserCourses.Add(new UserCourse { UserId = userId, CourseId = courseId });
                context.SaveChanges();
            }
        }

        public List<User> GetCourseParticipants(int courseId)
        {
            using var context = new KnowFlowDbContext();
            return context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .Join(context.Users,
                      uc => uc.UserId,
                      u => u.UserID,
                      (uc, u) => u)
                .ToList();
        }

        public List<CourseMaterial> GetCourseMaterials(int courseId)
        {
            using var context = new KnowFlowDbContext();

            return context.CourseMaterials
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Files)
                .ToList();
        }

        public void DeleteCourse(int courseId)
        {
            using var context = new KnowFlowDbContext();
            var course = context.Courses.Find(courseId);
            if (course != null)
            {
                context.Courses.Remove(course);
                context.SaveChanges();
            }
            else
            {
                MessageBox.Show("Курс с таким ID не найден.");
            }
        }

        public List<User> GetAvailableUsersForCourse(int courseId)
        {
            using var context = new KnowFlowDbContext();

            var enrolledUserIds = context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .Select(uc => uc.UserId);

            return context.Users
                .Where(u => !enrolledUserIds.Contains(u.UserID) &&
                            (u.UserRole == "Куратор" || u.UserRole == "Пользователь"))
                .ToList();
        }

        public List<MaterialSection> GetCourseSections(int courseId)
        {
            using var context = new KnowFlowDbContext();
            return context.MaterialSections
                .Where(s => s.CourseId == courseId)
                .Include(s => s.Materials)
                .ThenInclude(m => m.Files)
                .ToList();
        }

        public List<CourseMaterial> GetMaterialsWithoutSection(int courseId)
        {
            using var context = new KnowFlowDbContext();
            return context.CourseMaterials
                .Where(m => m.CourseId == courseId && m.SectionId == null)
                .Include(m => m.Files)
                .ToList();
        }

        public void AddCourseMaterial(CourseMaterial newMaterial)
        {
            using var context = new KnowFlowDbContext();
            context.CourseMaterials.Add(newMaterial);
            context.SaveChanges();
        }
    }
}