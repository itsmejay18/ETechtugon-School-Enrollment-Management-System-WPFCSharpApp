using System;
using System.Collections.ObjectModel;
using System.Data;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.Models;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class DepartmentManagementViewModel : CatalogCrudWorkspaceViewModel
    {
        private readonly DepartmentService _departmentService;
        private readonly CollegeService _collegeService;

        public DepartmentManagementViewModel(DepartmentService departmentService, CollegeService collegeService)
            : base(
                "Departments",
                "Manage department codes, heads, and college assignments.",
                "Department Code",
                "Department Name",
                "College",
                "Department Head",
                null,
                "Description")
        {
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _collegeService = collegeService ?? throw new ArgumentNullException(nameof(collegeService));
            Initialize();
        }

        protected override DataTable LoadRecords(string search)
        {
            return _departmentService.GetDepartments(search);
        }

        protected override void LoadOptionRecords(ObservableCollection<LookupOptionViewModel> options)
        {
            options.Add(new LookupOptionViewModel { Id = 0, Title = "(No college)", Subtitle = string.Empty });
            var table = _collegeService.GetLookupColleges() ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                options.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row["CollegeId"]),
                    Title = Convert.ToString(row["CollegeName"]),
                    Subtitle = Convert.ToString(row["CollegeCode"])
                });
            }
        }

        protected override CatalogRecordViewModel BuildRecord(DataRow row)
        {
            return new CatalogRecordViewModel
            {
                Id = Convert.ToInt32(row["DepartmentId"]),
                OptionId = row.Table.Columns.Contains("CollegeId") && row["CollegeId"] != DBNull.Value ? (int?)Convert.ToInt32(row["CollegeId"]) : null,
                Code = Convert.ToString(row["DepartmentCode"]),
                Name = Convert.ToString(row["DepartmentName"]),
                DetailA = Convert.ToString(row["DepartmentHead"]),
                DetailB = row.Table.Columns.Contains("CollegeCode") ? Convert.ToString(row["CollegeCode"]) : string.Empty,
                Description = Convert.ToString(row["Description"])
            };
        }

        protected override ValidationResult ValidateEditor()
        {
            return _departmentService.Validate(BuildDepartment(0));
        }

        protected override int SaveCore(int id)
        {
            var department = BuildDepartment(id);
            if (id == 0)
            {
                return _departmentService.Create(department);
            }

            _departmentService.Update(department);
            return id;
        }

        protected override void DeleteCore(int id)
        {
            _departmentService.Delete(id);
        }

        private Department BuildDepartment(int id)
        {
            return new Department
            {
                DepartmentId = id,
                CollegeId = SelectedOptionId,
                DepartmentCode = (Code ?? string.Empty).Trim(),
                DepartmentName = (Name ?? string.Empty).Trim(),
                DepartmentHead = (Extra1 ?? string.Empty).Trim(),
                Description = (RecordDescription ?? string.Empty).Trim()
            };
        }
    }
}
