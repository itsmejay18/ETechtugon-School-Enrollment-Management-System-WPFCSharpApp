using System;
using System.Data;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Wpf.Infrastructure;

namespace School_Management_System.Wpf.ViewModels.Calendar
{
    public sealed class AcademicCalendarViewModel : ViewModelBase
    {
        private readonly LookupService _lookupService;
        private readonly SectionService _sectionService;
        private DataView _academicYears;
        private DataView _semesters;
        private DataView _yearLevels;
        private DataView _sections;
        private string _statusMessage;

        public AcademicCalendarViewModel(LookupService lookupService, SectionService sectionService)
        {
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            RefreshCommand = new RelayCommand(Refresh);
            Refresh();
        }

        public RelayCommand RefreshCommand { get; private set; }

        public DataView AcademicYears
        {
            get { return _academicYears; }
            private set { SetProperty(ref _academicYears, value); }
        }

        public DataView Semesters
        {
            get { return _semesters; }
            private set { SetProperty(ref _semesters, value); }
        }

        public DataView YearLevels
        {
            get { return _yearLevels; }
            private set { SetProperty(ref _yearLevels, value); }
        }

        public DataView Sections
        {
            get { return _sections; }
            private set { SetProperty(ref _sections, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { SetProperty(ref _statusMessage, value); }
        }

        public void Refresh()
        {
            AcademicYears = (_lookupService.GetAcademicYears() ?? new DataTable()).DefaultView;
            Semesters = (_lookupService.GetSemesters() ?? new DataTable()).DefaultView;
            YearLevels = (_lookupService.GetYearLevels() ?? new DataTable()).DefaultView;
            Sections = (_sectionService.GetSections(string.Empty) ?? new DataTable()).DefaultView;
            StatusMessage = "Academic reference tables were loaded from the current database.";
        }
    }
}
