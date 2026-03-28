namespace School_Management_System.Wpf.ViewModels.Courses
{
    public sealed class CourseListItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
}
