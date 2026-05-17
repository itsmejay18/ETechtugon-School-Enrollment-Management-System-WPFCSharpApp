using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.Models;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class SectionManagementViewModel : CatalogCrudWorkspaceViewModel
    {
        private readonly SectionService _sectionService;
        private readonly CourseService _courseService;
        private readonly LookupService _lookupService;
        private readonly SystemSettingService _systemSettingService;

        public SectionManagementViewModel(SectionService sectionService, CourseService courseService, LookupService lookupService, SystemSettingService systemSettingService)
            : base(
                "Sections",
                "Manage simplified section codes, course assignments, and section names.",
                "Section Code",
                "Section Name",
                "Course",
                null,
                null,
                null)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));
            Initialize();
        }

        protected override DataTable LoadRecords(string search)
        {
            return _sectionService.GetSections(search, SelectedOptionId);
        }

        protected override void LoadOptionRecords(ObservableCollection<LookupOptionViewModel> options)
        {
            options.Add(new LookupOptionViewModel { Id = 0, Title = "(All courses)", Subtitle = string.Empty });
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
            return new CatalogRecordViewModel
            {
                Id = Convert.ToInt32(row["SectionId"]),
                OptionId = Convert.ToInt32(row["CourseId"]),
                Code = row.Table.Columns.Contains("SectionCode") ? Convert.ToString(row["SectionCode"]) : string.Empty,
                Name = Convert.ToString(row["SectionName"]),
                DetailA = row.Table.Columns.Contains("CourseCode") ? Convert.ToString(row["CourseCode"]) : Convert.ToString(row["CourseName"]),
                DetailB = row.Table.Columns.Contains("CourseName") ? Convert.ToString(row["CourseName"]) : string.Empty
            };
        }

        protected override ValidationResult ValidateEditor()
        {
            return _sectionService.Validate(BuildSection(0));
        }

        protected override int SaveCore(int id)
        {
            var section = BuildSection(id);
            if (id == 0)
            {
                return _sectionService.Create(section);
            }

            _sectionService.Update(section);
            return id;
        }

        protected override void DeleteCore(int id)
        {
            _sectionService.Delete(id);
        }

        private Section BuildSection(int id)
        {
            var activeTerm = _systemSettingService.GetActiveTerm();
            var yearLevelId = GetFirstYearLevelId();
            var courseId = SelectedOptionId.GetValueOrDefault();
            if (courseId <= 0)
            {
                courseId = GetFirstCourseId();
            }

            return new Section
            {
                SectionId = id,
                SectionCode = (Code ?? string.Empty).Trim(),
                SectionName = (Name ?? string.Empty).Trim(),
                CourseId = courseId,
                YearLevelId = yearLevelId,
                AcademicYearId = activeTerm.AcademicYearId.GetValueOrDefault(GetFirstAcademicYearId()),
                SemesterId = activeTerm.SemesterId.GetValueOrDefault(1)
            };
        }

        private int GetFirstCourseId()
        {
            var option = Options.FirstOrDefault(o => o.Id > 0);
            return option == null ? 0 : option.Id;
        }

        private int GetFirstYearLevelId()
        {
            var table = _lookupService.GetYearLevels() ?? new DataTable();
            return table.Rows.Count == 0 ? 0 : Convert.ToInt32(table.Rows[0]["YearLevelId"]);
        }

        private int GetFirstAcademicYearId()
        {
            var table = _lookupService.GetAcademicYears() ?? new DataTable();
            return table.Rows.Count == 0 ? 0 : Convert.ToInt32(table.Rows[0]["AcademicYearId"]);
        }
    }
}
