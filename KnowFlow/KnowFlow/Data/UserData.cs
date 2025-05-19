using KnowFlow.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BCrypt.Net;
using System.Diagnostics;
using KnowFlow.Data;
using System.Collections.ObjectModel;

namespace KnowFlow.Pages.Сlass
{
    public class UserData
    {
        public KnowFlowDbContext context = new KnowFlowDbContext();
        public List<User> LoadUsers()
        {
            return context.Users.AsNoTracking().ToList();
        }

        private bool VerifyRole(string username, string password, string role)
        {
            var user = context.Users.FirstOrDefault(u => u.Username == username && u.UserRole == role);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.UserPassword);
        }

        public bool IsAdmin(string username, string password) => VerifyRole(username, password, "Администратор");
        public bool VerifyUser(string username, string password) => VerifyRole(username, password, "Пользователь");
        public bool VerifyCurator(string username, string password) => VerifyRole(username, password, "Куратор");

        public void AddUser(string username, string password, string role)
        {

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
            var user = context.Users.Find(userId);
            if (user == null)
            {
                MessageBox.Show("Пользователь с таким ID не найден.");
                return;
            }
            user.UserRole = role;
            context.SaveChanges();
        }

        public void ResetPassword(int userId, string defaultPassword)
        {
            try
            {
                var user = context.Users.Find(userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь с таким ID не найден.");
                    return;
                }
                
                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении профиля: {ex.Message}");
            }
        }

        public void DeleteUser(int userId)
        {
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
            return context.Users.FirstOrDefault(u => u.Username == username)?.UserID ?? -1;
        }

        public int AddCourse(Course course)
        {
            try
            {
                context.Courses.Add(course);
                context.SaveChanges();

                var defaultSection = new MaterialSection
                {
                    CourseId = course.CourseId,
                    SectionName = "Без раздела"
                };

                context.MaterialSections.Add(defaultSection);
                context.SaveChanges();

                return course.CourseId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении курса: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public List<Course> LoadUserCourses(int userId)
        {
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
            if (!context.UserCourses.Any(uc => uc.UserId == userId && uc.CourseId == courseId))
            {
                context.UserCourses.Add(new UserCourse { UserId = userId, CourseId = courseId });
                context.SaveChanges();
            }
        }

        public void RemoveUserFromCourse(int userId, int courseId)
        {
            var userCourse = context.UserCourses
                .FirstOrDefault(uc => uc.UserId == userId && uc.CourseId == courseId);

            if (userCourse != null)
            {
                context.UserCourses.Remove(userCourse);
                context.SaveChanges();
            }
        }

        public List<User> GetCourseParticipants(int courseId)
        {
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
            return context.CourseMaterials
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Files)
                .ToList();
        }

        public void DeleteCourse(int courseId)
        {
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
            var sections = context.MaterialSections
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.SectionName == "Без раздела" ? 0 : 1)
                .ThenBy(s => s.SectionName)
                .ToList();

            foreach (var section in sections)
            {
                section.Materials = context.CourseMaterials
                    .Where(m => m.CourseId == courseId && m.SectionId == section.SectionId)
                    .Include(m => m.Files)
                    .ToList();
            }

            return sections;
        }

        public List<CourseMaterial> GetMaterialsWithoutSection(int courseId)
        {
            return context.CourseMaterials
                .Where(m => m.CourseId == courseId && m.SectionId == null)
                .Include(m => m.Files)
                .ToList();
        }

        public void AddCourseMaterial(CourseMaterial newMaterial)
        {
            context.CourseMaterials.Add(newMaterial);
            context.SaveChanges();
        }

        public void DeleteMaterial(int materialId)
        {
            var material = context.CourseMaterials
                                  .Include(m => m.Files)
                                  .FirstOrDefault(m => m.MaterialId == materialId);

            if (material == null)
                throw new Exception("Материал не найден");

            context.MaterialFiles.RemoveRange(material.Files);
            context.CourseMaterials.Remove(material);

            context.SaveChanges();
        }

        public CourseMaterial GetMaterialById(int materialId)
        {
            return context.CourseMaterials
                .Include(m => m.Files)
                .FirstOrDefault(m => m.MaterialId == materialId);
        }

        public void UpdateCourseMaterial(CourseMaterial updatedMaterial)
        {
            var existing = context.CourseMaterials
                .Include(m => m.Files)
                .FirstOrDefault(m => m.MaterialId == updatedMaterial.MaterialId);

            if (existing != null)
            {
                existing.MaterialName = updatedMaterial.MaterialName;
                existing.MaterialDescription = updatedMaterial.MaterialDescription;
                existing.SectionId = updatedMaterial.SectionId;

                var filesToRemove = context.MaterialFiles
                    .Where(f => f.MaterialId == existing.MaterialId)
                    .ToList();

                context.MaterialFiles.RemoveRange(filesToRemove);

                foreach (var file in updatedMaterial.Files)
                {
                    file.MaterialId = existing.MaterialId;
                    context.MaterialFiles.Add(file);
                }

                context.SaveChanges();
            }
        }

        public int AddMaterialSection(int courseId, string sectionName)
        {
            var section = new MaterialSection
            {
                CourseId = courseId,
                SectionName = sectionName
            };

            context.MaterialSections.Add(section);
            context.SaveChanges();

            return section.SectionId;
        }

        public bool UsernameExists(string username)
        {
            return context.Users.Any(u => u.Username == username);
        }

        public bool UpdateUser(string currentUsername, string newUsername, string newPassword)
        {
            try
            {
                var user = context.Users.FirstOrDefault(u => u.Username == currentUsername);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден");
                    return false;
                }

                user.Username = newUsername;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    user.UserPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }

                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении профиля: {ex.Message}");
                return false;
            }
        }

        public void AddNotice(Notice notice)
        {
            if (notice == null)
                throw new ArgumentNullException(nameof(notice));

            context.Notices.Add(notice);
            context.SaveChanges();
        }

        public List<Notice> GetActiveNotices(int courseId)
        {
            var now = DateTime.Now;
            return context.Notices
                .Where(n => n.CourseId == courseId &&
                           (n.ExpiresAt == null || n.ExpiresAt > now))
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public List<Notice> GetAllNotices(int courseId)
        {
            return context.Notices
                .Where(n => n.CourseId == courseId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public void DeleteNotice(int noticeId)
        {
            var notice = context.Notices.Find(noticeId);
            if (notice != null)
            {
                context.Notices.Remove(notice);
                context.SaveChanges();
            }
        }

        public void UpdateNotice(Notice updatedNotice)
        {
            var existing = context.Notices.Find(updatedNotice.NoticeId);
            if (existing != null)
            {
                existing.Title = updatedNotice.Title;
                existing.Content = updatedNotice.Content;
                existing.ExpiresAt = updatedNotice.ExpiresAt;
                context.SaveChanges();
            }
        }

        public Notice GetNoticeById(int noticeId)
        {
            return context.Notices
                .FirstOrDefault(m => m.NoticeId == noticeId);
        }

        public void SaveTest(Test test)
        {
            if (test == null)
                throw new ArgumentNullException(nameof(test));

            try
            {
                foreach (var question in test.Questions)
                {
                    question.QuestionId = 0;
                    question.TestId = test.TestId;

                    foreach (var answer in question.Answers)
                    {
                        answer.AnswerId = 0;
                        answer.QuestionId = question.QuestionId;
                    }
                }

                context.Tests.Add(test);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при сохранении теста", ex);
            }
        }

        public List<Test> GetCourseTests(int courseId)
        {
            return context.Tests
                .Where(t => t.CourseId == courseId)
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public Test GetTestById(int testId)
        {
            return context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestId == testId);
        }

        public void UpdateTest(Test updatedTest)
        {
            var existingTest = context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestId == updatedTest.TestId);

            if (existingTest == null)
            {
                throw new Exception("Тест не найден");
            }

            existingTest.Title = updatedTest.Title;
            existingTest.TimeLimit = updatedTest.TimeLimit;
            existingTest.MaxAttemps = updatedTest.MaxAttemps;

            var questionsToRemove = existingTest.Questions
                .Where(q => !updatedTest.Questions.Any(uq => uq.QuestionId == q.QuestionId))
                .ToList();

            foreach (var question in questionsToRemove)
            {
                context.Questions.Remove(question);
            }

            foreach (var updatedQuestion in updatedTest.Questions)
            {
                var existingQuestion = existingTest.Questions
                    .FirstOrDefault(q => q.QuestionId == updatedQuestion.QuestionId);

                if (existingQuestion != null)
                {
                    existingQuestion.QuestionText = updatedQuestion.QuestionText;
                    existingQuestion.QuestionType = updatedQuestion.QuestionType;
                    existingQuestion.Points = updatedQuestion.Points;

                    var answersToRemove = existingQuestion.Answers
                        .Where(a => !updatedQuestion.Answers.Any(ua => ua.AnswerId == a.AnswerId))
                        .ToList();

                    foreach (var answer in answersToRemove)
                    {
                        context.Answers.Remove(answer);
                    }

                    foreach (var updatedAnswer in updatedQuestion.Answers)
                    {
                        var existingAnswer = existingQuestion.Answers
                            .FirstOrDefault(a => a.AnswerId == updatedAnswer.AnswerId);

                        if (existingAnswer != null)
                        {
                            existingAnswer.AnswerText = updatedAnswer.AnswerText;
                            existingAnswer.IsCorrect = updatedAnswer.IsCorrect;
                        }

                        else
                        {
                            var newAnswer = new Answer
                            {
                                AnswerText = updatedAnswer.AnswerText,
                                IsCorrect = updatedAnswer.IsCorrect,
                                QuestionId = existingQuestion.QuestionId
                            };
                            context.Answers.Add(newAnswer);
                        }
                    }
                }
                else
                {
                    updatedQuestion.TestId = existingTest.TestId;
                    context.Questions.Add(updatedQuestion);
                }
            }

            context.SaveChanges();
        }

        public void DeleteTest(int testId)
        {
            var test = context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestId == testId);

            if (test != null)
            {
                context.Tests.Remove(test);
                context.SaveChanges();
            }
        }

        public int SaveTestResult(TestResult result)
        {
            try
            {
                context.TestResults.Add(result);
                context.SaveChanges();
                return result.ResultId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении результата теста: {ex.Message}");
                return -1;
            }
        }

        public int GetUserTestAttemptNumber(int userId, int testId)
        {
            return context.TestResults
                .Where(r => r.UserId == userId && r.TestId == testId)
                .Count() + 1;
        }

        public bool HasAttemptsLeft(int userId, int testId, int? maxAttempts)
        {
            if (maxAttempts == null) return true;

            int attemptsUsed = context.TestResults
                .Count(r => r.UserId == userId && r.TestId == testId);

            return attemptsUsed < maxAttempts;
        }

        public bool CheckTextAnswer(int questionId, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            var correctAnswers = context.Answers
                .Where(a => a.QuestionId == questionId && a.IsCorrect)
                .Select(a => a.AnswerText.Trim())
                .ToList();

            if (!correctAnswers.Any())
                return false;

            var userAnswerNormalized = userAnswer.Trim();

            return correctAnswers.Any(correctAnswer =>
                string.Equals(correctAnswer, userAnswerNormalized, StringComparison.OrdinalIgnoreCase));
        }

        public List<TestResultDisplay> GetTestResults(int testId)
        {
            return context.TestResults
                .Include(r => r.QuestionResults)
                    .ThenInclude(q => q.Question)
                .Include(r => r.QuestionResults)
                    .ThenInclude(q => q.AnswerSelections)
                        .ThenInclude(s => s.Answer)
                .Include(r => r.User)
                .Where(r => r.TestId == testId)
                .Select(r => new TestResultDisplay
                {
                    Username = r.User.Username,
                    StartedAt = r.StartedAt,
                    CompletedAt = r.FinishedAt,
                    TotalPoints = r.QuestionResults.Sum(q => q.PointsEarned),
                    QuestionResults = r.QuestionResults
                })
                .ToList();
        }


    }    
}