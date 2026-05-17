using System;
using System.Collections.ObjectModel;
using System.Data;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.Models;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class SubjectManagementViewModel : CatalogCrudWorkspaceViewModel
    {
        private readonly SubjectService _subjectService;
        private readonly CourseService _courseService;

        public SubjectManagementViewModel(SubjectService subjectService, CourseService courseService)
            : base(
                "Subjects",
                "Manage subject codes, course assignments, units, and enrollment capacity.",
                "Subject Code",
                "Subject Name",
                "Course",
                "Units",
                "Max Students",
                null)
        {
            _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            Initialize();
        }

        protected override DataTable LoadRecords(string search)
        {
            return _subjectService.GetSubjects(search);
        }

        protected override void LoadOptionRecords(ObservableCollection<LookupOptionViewModel> options)
        {
            options.Add(new LookupOptionViewModel { Id = 0, Title = "(Shared subject)", Subtitle = string.Empty });
            var table = _courseService.GetLookupCourses() ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                options.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row["CourseId"]),
                    Title = Convert.ToString(row["CourseName"]),
                    Subtitle = Convert.ToString(row["CourseCode"])
                });
            }
        }

        protected override CatalogRecordViewModel BuildRecord(DataRow row)
        {
            var maxStudents = row.Table.Columns.Contains("MaxStudents") && row["MaxStudents"] != DBNull.Value
                ? Convert.ToString(row["MaxStudents"])
                : string.Empty;
            var current = row.Table.Columns.Contains("CurrentEnrolledCount")
                ? Convert.ToString(row["CurrentEnrolledCount"])
                : "0";

            return new CatalogRecordViewModel
            {
                Id = Convert.ToInt32(row["SubjectId"]),
                OptionId = row.Table.Columns.Contains("CourseId") && row["CourseId"] != DBNull.Value ? (int?)Convert.ToInt32(row["CourseId"]) : null,
                Code = Convert.ToString(row["SubjectCode"]),
                Name = Convert.ToString(row["SubjectName"]),
                DetailA = Convert.ToString(row["Units"]),
                DetailB = string.IsNullOrWhiteSpace(maxStudents) ? current + " enrolled" : current + " / " + maxStudents,
                Description = string.Empty
            };
        }

        protected override ValidationResult ValidateEditor()
        {
            return _subjectService.Validate(BuildSubject(0));
        }

        protected override int SaveCore(int id)
        {
            var subject = BuildSubject(id);
            if (id == 0)
            {
                return _subjectService.Create(subject);
            }

            _subjectService.Update(subject);
            return id;
        }

        protected override void DeleteCore(int id)
        {
            _subjectService.Delete(id);
        }

        private Subject BuildSubject(int id)
        {
            return new Subject
            {
                SubjectId = id,
                SubjectCode = (Code ?? string.Empty).Trim(),
                SubjectName = (Name ?? string.Empty).Trim(),
                CourseId = SelectedOptionId,
                Units = ParseRequiredInt(Extra1),
                MaxStudents = ParseOptionalInt(Extra2)
            };
        }
    }
}
