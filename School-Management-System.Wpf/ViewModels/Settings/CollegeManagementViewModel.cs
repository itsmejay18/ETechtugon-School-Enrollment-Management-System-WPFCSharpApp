using System;
using System.Collections.ObjectModel;
using System.Data;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.Models;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class CollegeManagementViewModel : CatalogCrudWorkspaceViewModel
    {
        private readonly CollegeService _collegeService;

        public CollegeManagementViewModel(CollegeService collegeService)
            : base(
                "Colleges",
                "Manage colleges and their dean assignments.",
                "College Code",
                "College Name",
                null,
                "Dean Name",
                null,
                "Description")
        {
            _collegeService = collegeService ?? throw new ArgumentNullException(nameof(collegeService));
            Initialize();
        }

        protected override DataTable LoadRecords(string search)
        {
            return _collegeService.GetColleges(search);
        }

        protected override void LoadOptionRecords(ObservableCollection<LookupOptionViewModel> options)
        {
        }

        protected override CatalogRecordViewModel BuildRecord(DataRow row)
        {
            return new CatalogRecordViewModel
            {
                Id = Convert.ToInt32(row["CollegeId"]),
                Code = Convert.ToString(row["CollegeCode"]),
                Name = Convert.ToString(row["CollegeName"]),
                DetailA = Convert.ToString(row["DeanName"]),
                DetailB = row.Table.Columns.Contains("DepartmentCount") ? Convert.ToString(row["DepartmentCount"]) + " department(s)" : string.Empty,
                Description = Convert.ToString(row["Description"])
            };
        }

        protected override ValidationResult ValidateEditor()
        {
            return _collegeService.Validate(BuildCollege(0));
        }

        protected override int SaveCore(int id)
        {
            var college = BuildCollege(id);
            if (id == 0)
            {
                return _collegeService.Create(college);
            }

            _collegeService.Update(college);
            return id;
        }

        protected override void DeleteCore(int id)
        {
            _collegeService.Delete(id);
        }

        private College BuildCollege(int id)
        {
            return new College
            {
                CollegeId = id,
                CollegeCode = (Code ?? string.Empty).Trim(),
                CollegeName = (Name ?? string.Empty).Trim(),
                DeanName = (Extra1 ?? string.Empty).Trim(),
                Description = (RecordDescription ?? string.Empty).Trim()
            };
        }
    }
}
