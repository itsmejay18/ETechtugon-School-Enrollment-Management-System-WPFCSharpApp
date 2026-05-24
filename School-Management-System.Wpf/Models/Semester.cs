namespace School_Management_System.Models
{
    public sealed class Semester
    {
        private int _semesterId;
        private string _name;
        private int _sortOrder;
        private bool _isActive;

        public int SemesterId { get { return _semesterId; } set { _semesterId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public int SortOrder { get { return _sortOrder; } set { _sortOrder = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
    }
}

