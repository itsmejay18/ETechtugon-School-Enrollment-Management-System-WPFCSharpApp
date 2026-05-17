using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class GlobalSettingsViewModel : ViewModelBase
    {
        private readonly SystemSettingService _systemSettingService;
        private readonly LookupService _lookupService;
        private LookupOptionViewModel _selectedAcademicYear;
        private LookupOptionViewModel _selectedSemester;
        private string _statusMessage;

        public GlobalSettingsViewModel(SystemSettingService systemSettingService, LookupService lookupService)
        {
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));

            AcademicYears = new ObservableCollection<LookupOptionViewModel>();
            Semesters = new ObservableCollection<LookupOptionViewModel>();
            RefreshCommand = new RelayCommand(Refresh);
            SaveCommand = new RelayCommand(Save, () => SelectedAcademicYear != null && SelectedSemester != null);

            Refresh();
        }

        public ObservableCollection<LookupOptionViewModel> AcademicYears { get; private set; }
        public ObservableCollection<LookupOptionViewModel> Semesters { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }

        public LookupOptionViewModel SelectedAcademicYear
        {
            get { return _selectedAcademicYear; }
            set
            {
                if (SetProperty(ref _selectedAcademicYear, value))
                {
                    OnPropertyChanged(nameof(ActiveTermText));
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public LookupOptionViewModel SelectedSemester
        {
            get { return _selectedSemester; }
            set
            {
                if (SetProperty(ref _selectedSemester, value))
                {
                    OnPropertyChanged(nameof(ActiveTermText));
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { SetProperty(ref _statusMessage, value); }
        }

        public string ActiveTermText
        {
            get
            {
                return "Current Semester: " + (SelectedSemester == null ? "(not set)" : SelectedSemester.Title) +
                       " | School Year: " + (SelectedAcademicYear == null ? "(not set)" : SelectedAcademicYear.Title);
            }
        }

        public void Refresh()
        {
            LoadLookup(AcademicYears, _lookupService.GetAcademicYears(), "AcademicYearId", "Name");
            LoadLookup(Semesters, _lookupService.GetSemesters(), "SemesterId", "Name");

            var activeTerm = _systemSettingService.GetActiveTerm();
            SelectedAcademicYear = activeTerm.AcademicYearId.HasValue
                ? AcademicYears.FirstOrDefault(x => x.Id == activeTerm.AcademicYearId.Value) ?? AcademicYears.FirstOrDefault()
                : AcademicYears.FirstOrDefault();
            SelectedSemester = activeTerm.SemesterId.HasValue
                ? Semesters.FirstOrDefault(x => x.Id == activeTerm.SemesterId.Value) ?? Semesters.FirstOrDefault()
                : Semesters.FirstOrDefault();

            StatusMessage = "Global settings loaded from the system settings table.";
            OnPropertyChanged(nameof(ActiveTermText));
        }

        private void Save()
        {
            if (SelectedAcademicYear == null || SelectedSemester == null)
            {
                return;
            }

            try
            {
                _systemSettingService.SetActiveTerm(SelectedAcademicYear.Id, SelectedSemester.Id);
                StatusMessage = "Global settings saved. Enrollment and schedules will use the selected active term.";
                OnPropertyChanged(nameof(ActiveTermText));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Global Settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void LoadLookup(ObservableCollection<LookupOptionViewModel> target, DataTable table, string idColumn, string titleColumn)
        {
            target.Clear();
            table = table ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                target.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row[idColumn]),
                    Title = Convert.ToString(row[titleColumn]),
                    Subtitle = string.Empty
                });
            }
        }
    }
}
