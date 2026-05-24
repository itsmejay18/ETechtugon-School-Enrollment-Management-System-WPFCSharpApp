using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class DepartmentService
    {
        private readonly IDepartmentData _departmentData;

        public DepartmentService(IDepartmentData departmentData)
        {
            _departmentData = departmentData ?? throw new ArgumentNullException(nameof(departmentData));
        }

        public DataTable GetDepartments(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return _departmentData.GetAllActive();
            }

            return _departmentData.Search(search);
        }

        public DataTable GetLookupDepartments()
        {
            return _departmentData.GetLookupActive();
        }

        public int GetActiveCount()
        {
            return _departmentData.GetActiveCount();
        }

        public ValidationResult Validate(Department department)
        {
            var vr = new ValidationResult();
            if (department == null)
            {
                vr.Add("Department is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(department.DepartmentCode))
            {
                vr.Add("Department Code is required.");
            }

            if (string.IsNullOrWhiteSpace(department.DepartmentName))
            {
                vr.Add("Department Name is required.");
            }

            return vr;
        }

        public int Create(Department department)
        {
            return _departmentData.Insert(department);
        }

        public void Update(Department department)
        {
            _departmentData.Update(department);
        }

        public void Delete(int departmentId)
        {
            _departmentData.Delete(departmentId);
        }
    }
}
