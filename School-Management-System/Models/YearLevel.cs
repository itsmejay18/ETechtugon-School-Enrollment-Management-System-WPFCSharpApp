namespace School_Management_System.Models
{
    public sealed class YearLevel
    {
        private int _yearLevelId;
        private string _name;
        private int _sortOrder;
        private bool _isActive;

        public int YearLevelId { get { return _yearLevelId; } set { _yearLevelId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public int SortOrder { get { return _sortOrder; } set { _sortOrder = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
    }
}

