using System;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Models;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Courses;
using School_Management_System.Wpf.ViewModels.Faculty;
using School_Management_System.Wpf.ViewModels.Students;

namespace School_Management_System.Tests.Integration
{
    [TestFixture]
    [NonParallelizable]
    [Apartment(ApartmentState.STA)]
    [Category("Database")]
    public sealed class WpfCrudSmokeTests
    {
        [SetUp]
        public void SetUp()
        {
            ConnectionModeHelper.ApplyRuntimeMode("Local");
            if (!HasSmokeTestDatabaseConfiguration())
            {
                Assert.Ignore("Database smoke tests require local MySQL credentials. Set SMS_DB_USER and SMS_DB_PASSWORD to run them.");
            }
        }

        [Test]
        public void StudentService_CanCreateUpdateAndDelete_TemporaryRecord()
        {
            var bootstrapper = AppBootstrapper.CreateForCurrentMode();
            var token = BuildToken();
            var phoneSuffix = BuildPhoneSuffix();
            var originalNumber = "STU-" + token;
            var updatedNumber = originalNumber + "-UPD";

            var student = new Student
            {
                StudentNumber = originalNumber,
                FirstName = "Smoke",
                LastName = "Crud" + token,
                MiddleName = "Local",
                Gender = "Other",
                BirthDate = new DateTime(2004, 6, 18),
                Email = "student." + token.ToLowerInvariant() + "@example.test",
                Phone = "0917" + phoneSuffix,
                Address = "Temporary smoke-test record"
            };

            var studentId = bootstrapper.StudentService.Create(student);

            try
            {
                var created = bootstrapper.StudentService.GetStudents(originalNumber);
                Assert.That(ContainsValue(created, "StudentId", studentId), Is.True, "Created student was not returned by search.");

                student.StudentId = studentId;
                student.StudentNumber = updatedNumber;
                student.LastName = "Updated" + token;
                bootstrapper.StudentService.Update(student);

                var updated = bootstrapper.StudentService.GetStudents(updatedNumber);
                Assert.That(ContainsValue(updated, "StudentId", studentId), Is.True, "Updated student was not returned by search.");

                bootstrapper.StudentService.Delete(studentId);

                var afterDelete = bootstrapper.StudentService.GetStudents(updatedNumber);
                Assert.That(ContainsValue(afterDelete, "StudentId", studentId), Is.False, "Deleted student still appears active.");
            }
            finally
            {
                TryDeleteStudent(bootstrapper, studentId);
            }
        }

        [Test]
        public void FacultyService_CanCreateUpdateAndDelete_TemporaryRecord()
        {
            var bootstrapper = AppBootstrapper.CreateForCurrentMode();
            var token = BuildToken();
            var phoneSuffix = BuildPhoneSuffix();
            var originalCode = "FAC" + token;
            var updatedCode = "FC" + token + "U";

            var faculty = new Faculty
            {
                FacultyCode = originalCode,
                FirstName = "Smoke",
                LastName = "Faculty" + token,
                MiddleName = "Local",
                Email = "faculty." + token.ToLowerInvariant() + "@example.test",
                Phone = "0918" + phoneSuffix,
                Address = "Temporary smoke-test record",
                HireDate = DateTime.Today
            };

            var facultyId = bootstrapper.FacultyService.Create(faculty);

            try
            {
                var created = bootstrapper.FacultyService.GetFaculty(originalCode);
                Assert.That(ContainsValue(created, "FacultyId", facultyId), Is.True, "Created faculty was not returned by search.");

                faculty.FacultyId = facultyId;
                faculty.FacultyCode = updatedCode;
                faculty.LastName = "Updated" + token;
                bootstrapper.FacultyService.Update(faculty);

                var updated = bootstrapper.FacultyService.GetFaculty(updatedCode);
                Assert.That(ContainsValue(updated, "FacultyId", facultyId), Is.True, "Updated faculty was not returned by search.");

                bootstrapper.FacultyService.Delete(facultyId);

                var afterDelete = bootstrapper.FacultyService.GetFaculty(updatedCode);
                Assert.That(ContainsValue(afterDelete, "FacultyId", facultyId), Is.False, "Deleted faculty still appears active.");
            }
            finally
            {
                TryDeleteFaculty(bootstrapper, facultyId);
            }
        }

        [Test]
        public void CourseService_CanCreateUpdateAndDelete_TemporaryRecord()
        {
            var bootstrapper = AppBootstrapper.CreateForCurrentMode();
            var token = BuildToken();
            var originalCode = "CRS" + token;
            var updatedCode = "CR" + token + "U";

            var firstDepartment = bootstrapper.DepartmentService.GetLookupDepartments()
                .Rows
                .Cast<DataRow>()
                .Select(row => row["DepartmentId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["DepartmentId"]))
                .FirstOrDefault(id => id.HasValue && id.Value > 0);

            var course = new Course
            {
                CourseCode = originalCode,
                CourseName = "Smoke Course " + token,
                Description = "Temporary smoke-test record",
                DepartmentId = firstDepartment
            };

            var courseId = bootstrapper.CourseService.Create(course);

            try
            {
                var created = bootstrapper.CourseService.GetCourses(originalCode);
                Assert.That(ContainsValue(created, "CourseId", courseId), Is.True, "Created course was not returned by search.");

                course.CourseId = courseId;
                course.CourseCode = updatedCode;
                course.CourseName = "Updated Course " + token;
                bootstrapper.CourseService.Update(course);

                var updated = bootstrapper.CourseService.GetCourses(updatedCode);
                Assert.That(ContainsValue(updated, "CourseId", courseId), Is.True, "Updated course was not returned by search.");

                bootstrapper.CourseService.Delete(courseId);

                var afterDelete = bootstrapper.CourseService.GetCourses(updatedCode);
                Assert.That(ContainsValue(afterDelete, "CourseId", courseId), Is.False, "Deleted course still appears active.");
            }
            finally
            {
                TryDeleteCourse(bootstrapper, courseId);
            }
        }

        [Test]
        public void StudentDirectory_Commands_OpenAndCloseExpectedModalStates()
        {
            var bootstrapper = AppBootstrapper.CreateForCurrentMode();
            var vm = new StudentDirectoryViewModel(bootstrapper.StudentService, bootstrapper.SystemSettingService, null);

            Assert.That(vm.AddCommand.CanExecute(null), Is.True);
            vm.AddCommand.Execute(null);
            Assert.That(vm.IsEditorActive, Is.True);
            Assert.That(vm.IsDetailsModalOpen, Is.True);
            Assert.That(vm.SaveCommand.CanExecute(null), Is.True);

            vm.CancelCommand.Execute(null);
            Assert.That(vm.IsEditorActive, Is.False);
            Assert.That(vm.IsDetailsModalOpen, Is.False);

            if (vm.SelectedRecord != null)
            {
                Assert.That(vm.EditCommand.CanExecute(null), Is.True);
                Assert.That(vm.OpenDetailsCommand.CanExecute(null), Is.True);

                vm.OpenDetailsCommand.Execute(null);
                Assert.That(vm.IsDetailsModalOpen, Is.True);

                vm.CloseDetailsCommand.Execute(null);
                Assert.That(vm.IsDetailsModalOpen, Is.False);
            }
        }

        [Test]
        public void FacultyDirectory_CanOpenAndClosePreviewModal()
        {
            var bootstrapper = AppBootstrapper.CreateForCurrentMode();
            var vm = new FacultyDirectoryViewModel(bootstrapper.FacultyService, bootstrapper.ClassScheduleService);

            if (vm.SelectedRecord == null)
            {
                Assert.Pass("No faculty records are available in the local database to smoke-test preview modal commands.");
            }

            Assert.That(vm.OpenDetailsCommand.CanExecute(null), Is.True);
            vm.OpenDetailsCommand.Execute(null);
            Assert.That(vm.IsDetailsModalOpen, Is.True);

            vm.CloseDetailsCommand.Execute(null);
            Assert.That(vm.IsDetailsModalOpen, Is.False);
        }

        [Test]
        public void CourseManagement_Commands_OpenAndCloseExpectedModalStates()
        {
            var bootstrapper = AppBootstrapper.CreateForCurrentMode();
            var vm = new CourseManagementViewModel(bootstrapper.CourseService, bootstrapper.DepartmentService, null);

            Assert.That(vm.AddCommand.CanExecute(null), Is.True);
            vm.AddCommand.Execute(null);
            Assert.That(vm.IsEditorActive, Is.True);
            Assert.That(vm.IsEditorModalOpen, Is.True);
            Assert.That(vm.SaveCommand.CanExecute(null), Is.True);

            vm.CancelCommand.Execute(null);
            Assert.That(vm.IsEditorActive, Is.False);
            Assert.That(vm.IsEditorModalOpen, Is.False);

            if (vm.SelectedCourse != null)
            {
                Assert.That(vm.EditCommand.CanExecute(null), Is.True);
                Assert.That(vm.DeleteCommand.CanExecute(null), Is.True);
            }
        }

        private static string BuildToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        }

        private static string BuildPhoneSuffix()
        {
            return (Math.Abs(Guid.NewGuid().GetHashCode()) % 10000000).ToString("D7");
        }

        private static bool ContainsValue(DataTable table, string columnName, int expectedValue)
        {
            if (table == null || string.IsNullOrWhiteSpace(columnName) || !table.Columns.Contains(columnName))
            {
                return false;
            }

            return table.Rows.Cast<DataRow>().Any(row => row[columnName] != DBNull.Value && Convert.ToInt32(row[columnName]) == expectedValue);
        }

        private static bool HasSmokeTestDatabaseConfiguration()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMS_DB_USER")) ||
                   !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMS_DB_USER_LOCAL"));
        }

        private static void TryDeleteStudent(AppBootstrapper bootstrapper, int studentId)
        {
            if (bootstrapper == null || studentId <= 0)
            {
                return;
            }

            try
            {
                bootstrapper.StudentService.Delete(studentId);
            }
            catch
            {
            }
        }

        private static void TryDeleteFaculty(AppBootstrapper bootstrapper, int facultyId)
        {
            if (bootstrapper == null || facultyId <= 0)
            {
                return;
            }

            try
            {
                bootstrapper.FacultyService.Delete(facultyId);
            }
            catch
            {
            }
        }

        private static void TryDeleteCourse(AppBootstrapper bootstrapper, int courseId)
        {
            if (bootstrapper == null || courseId <= 0)
            {
                return;
            }

            try
            {
                bootstrapper.CourseService.Delete(courseId);
            }
            catch
            {
            }
        }
    }
}
