using System;
using System.Collections.ObjectModel;
using System.Data;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Schedule
{
    public sealed class ScheduleBoardViewModel : ViewModelBase
    {
        private readonly SectionService _sectionService;
        private readonly FacultyService _facultyService;
        private readonly ClassScheduleService _classScheduleService;
        private LookupOptionViewModel _selectedSection;
        private LookupOptionViewModel _selectedFaculty;
        private DataView _sectionScheduleRecords;
        private DataView _facultyScheduleRecords;
        private string _sectionStatus;
        private string _facultyStatus;

        public ScheduleBoardViewModel(SectionService sectionService, FacultyService facultyService, ClassScheduleService classScheduleService)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _facultyService = facultyService ?? throw new ArgumentNullException(nameof(facultyService));
            _classScheduleService = classScheduleService ?? throw new ArgumentNullException(nameof(classScheduleService));

            Sections = new ObservableCollection<LookupOptionViewModel>();
            FacultyMembers = new ObservableCollection<LookupOptionViewModel>();
            RefreshCommand = new RelayCommand(Refresh);

            Refresh();
        }

        public ObservableCollection<LookupOptionViewModel> Sections { get; private set; }
        public ObservableCollection<LookupOptionViewModel> FacultyMembers { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        public LookupOptionViewModel SelectedSection
        {
            get { return _selectedSection; }
            set
            {
                if (SetProperty(ref _selectedSection, value))
                {
                    LoadSectionSchedule();
                }
            }
        }

        public LookupOptionViewModel SelectedFaculty
        {
            get { return _selectedFaculty; }
            set
            {
                if (SetProperty(ref _selectedFaculty, value))
                {
                    LoadFacultySchedule();
                }
            }
        }

        public DataView SectionScheduleRecords
        {
            get { return _sectionScheduleRecords; }
            private set
            {
                if (SetProperty(ref _sectionScheduleRecords, value))
                {
                    OnPropertyChanged(nameof(SectionRecordCountText));
                }
            }
        }

        public DataView FacultyScheduleRecords
        {
            get { return _facultyScheduleRecords; }
            private set
            {
                if (SetProperty(ref _facultyScheduleRecords, value))
                {
                    OnPropertyChanged(nameof(FacultyRecordCountText));
                }
            }
        }

        public string SectionStatus
        {
            get { return _sectionStatus; }
            private set { SetProperty(ref _sectionStatus, value); }
        }

        public string FacultyStatus
        {
            get { return _facultyStatus; }
            private set { SetProperty(ref _facultyStatus, value); }
        }

        public string SectionRecordCountText
        {
            get { return (SectionScheduleRecords == null ? 0 : SectionScheduleRecords.Count).ToString() + " section schedule row(s)"; }
        }

        public string FacultyRecordCountText
        {
            get { return (FacultyScheduleRecords == null ? 0 : FacultyScheduleRecords.Count).ToString() + " faculty assignment row(s)"; }
        }

        public void Refresh()
        {
            LoadSections();
            LoadFacultyMembers();
        }

        private void LoadSections()
        {
            Sections.Clear();

            var table = _sectionService.GetSections(string.Empty) ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                Sections.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row["SectionId"]),
                    Title = Convert.ToString(row["SectionName"]),
                    Subtitle = Convert.ToString(row["CourseName"]) + " | " + Convert.ToString(row["AcademicYear"]) + " | " + Convert.ToString(row["Semester"])
                });
            }

            SelectedSection = Sections.Count > 0 ? Sections[0] : null;
            if (Sections.Count == 0)
            {
                SectionScheduleRecords = new DataView(new DataTable());
                SectionStatus = "No section records were found in the database.";
            }
        }

        private void LoadFacultyMembers()
        {
            FacultyMembers.Clear();

            var table = _facultyService.GetFaculty(string.Empty) ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                FacultyMembers.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row["FacultyId"]),
                    Title = Convert.ToString(row["FacultyCode"]),
                    Subtitle = Convert.ToString(row["LastName"]) + ", " + Convert.ToString(row["FirstName"])
                });
            }

            SelectedFaculty = FacultyMembers.Count > 0 ? FacultyMembers[0] : null;
            if (FacultyMembers.Count == 0)
            {
                FacultyScheduleRecords = new DataView(new DataTable());
                FacultyStatus = "No faculty records were found in the database.";
            }
        }

        private void LoadSectionSchedule()
        {
            if (SelectedSection == null)
            {
                SectionScheduleRecords = new DataView(new DataTable());
                SectionStatus = "Select a section to load the schedule.";
                return;
            }

            var table = _classScheduleService.GetBySection(SelectedSection.Id) ?? new DataTable();
            SectionScheduleRecords = table.DefaultView;
            SectionStatus = table.Rows.Count > 0
                ? "Loaded schedule for " + SelectedSection.Title + "."
                : "No schedule rows were found for the selected section.";
        }

        private void LoadFacultySchedule()
        {
            if (SelectedFaculty == null)
            {
                FacultyScheduleRecords = new DataView(new DataTable());
                FacultyStatus = "Select a faculty member to load teaching assignments.";
                return;
            }

            var table = _classScheduleService.GetByFaculty(SelectedFaculty.Id) ?? new DataTable();
            FacultyScheduleRecords = table.DefaultView;
            FacultyStatus = table.Rows.Count > 0
                ? "Loaded schedule assignments for " + SelectedFaculty.Subtitle + "."
                : "No schedule rows were found for the selected faculty member.";
        }
    }
}
