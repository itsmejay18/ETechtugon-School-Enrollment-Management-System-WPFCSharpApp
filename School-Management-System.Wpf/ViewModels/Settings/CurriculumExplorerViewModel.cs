using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class CurriculumExplorerViewModel : ViewModelBase
    {
        private readonly CurriculumService _curriculumService;
        private readonly CourseService _courseService;
        private readonly LookupService _lookupService;
        private LookupOptionViewModel _selectedCourse;
        private LookupOptionViewModel _selectedAcademicYear;
        private LookupOptionViewModel _selectedYearLevel;
        private LookupOptionViewModel _selectedSemester;
        private DataView _curriculumSubjects;
        private string _statusMessage;

        public CurriculumExplorerViewModel(CurriculumService curriculumService, CourseService courseService, LookupService lookupService)
        {
            _curriculumService = curriculumService ?? throw new ArgumentNullException(nameof(curriculumService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));

            Courses = new ObservableCollection<LookupOptionViewModel>();
            AcademicYears = new ObservableCollection<LookupOptionViewModel>();
            YearLevels = new ObservableCollection<LookupOptionViewModel>();
            Semesters = new ObservableCollection<LookupOptionViewModel>();
            RefreshCommand = new RelayCommand(Refresh);

            Refresh();
        }

        public ObservableCollection<LookupOptionViewModel> Courses { get; private set; }
        public ObservableCollection<LookupOptionViewModel> AcademicYears { get; private set; }
        public ObservableCollection<LookupOptionViewModel> YearLevels { get; private set; }
        public ObservableCollection<LookupOptionViewModel> Semesters { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        public LookupOptionViewModel SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                if (SetProperty(ref _selectedCourse, value))
                {
                    LoadCurriculum();
                    RaiseCurriculumIdentityChanged();
                }
            }
        }

        public LookupOptionViewModel SelectedAcademicYear
        {
            get { return _selectedAcademicYear; }
            set
            {
                if (SetProperty(ref _selectedAcademicYear, value))
                {
                    LoadCurriculum();
                    RaiseCurriculumIdentityChanged();
                }
            }
        }

        public LookupOptionViewModel SelectedYearLevel
        {
            get { return _selectedYearLevel; }
            set
            {
                if (SetProperty(ref _selectedYearLevel, value))
                {
                    LoadCurriculum();
                    RaiseCurriculumIdentityChanged();
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
                    LoadCurriculum();
                    RaiseCurriculumIdentityChanged();
                }
            }
        }

        public DataView CurriculumSubjects
        {
            get { return _curriculumSubjects; }
            private set { SetProperty(ref _curriculumSubjects, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { SetProperty(ref _statusMessage, value); }
        }

        public string SelectedCurriculumCode
        {
            get { return BuildCurriculumCode(); }
        }

        public string SelectedCurriculumName
        {
            get { return BuildCurriculumName(); }
        }

        public void Refresh()
        {
            LoadCourses();
            LoadAcademicReferences();
            LoadCurriculum();
            RaiseCurriculumIdentityChanged();
        }

        private void LoadCourses()
        {
            Courses.Clear();
            var table = _courseService.GetLookupCourses() ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                Courses.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row["CourseId"]),
                    Title = Convert.ToString(row["CourseName"]),
                    Subtitle = Convert.ToString(row["CourseCode"])
                });
            }

            SelectedCourse = Courses.Count > 0 ? Courses[0] : null;
        }

        private void LoadAcademicReferences()
        {
            LoadLookupCollection(AcademicYears, _lookupService.GetAcademicYears(), "AcademicYearId", "Name", null);
            LoadLookupCollection(YearLevels, _lookupService.GetYearLevels(), "YearLevelId", "Name", null);
            LoadLookupCollection(Semesters, _lookupService.GetSemesters(), "SemesterId", "Name", null);

            if (SelectedAcademicYear == null && AcademicYears.Count > 0) SelectedAcademicYear = AcademicYears[0];
            if (SelectedYearLevel == null && YearLevels.Count > 0) SelectedYearLevel = YearLevels[0];
            if (SelectedSemester == null && Semesters.Count > 0) SelectedSemester = Semesters[0];
            RaiseCurriculumIdentityChanged();
        }

        private void LoadCurriculum()
        {
            if (SelectedCourse == null || SelectedAcademicYear == null || SelectedYearLevel == null || SelectedSemester == null)
            {
                CurriculumSubjects = new DataView(new DataTable());
                StatusMessage = "Select the academic references to load curriculum subjects.";
                return;
            }

            var curriculumId = _curriculumService.TryGetCurriculumId(
                SelectedCourse.Id,
                SelectedYearLevel.Id,
                SelectedSemester.Id,
                SelectedAcademicYear.Id);

            if (!curriculumId.HasValue)
            {
                CurriculumSubjects = new DataView(new DataTable());
                StatusMessage = "No curriculum mapping exists yet for the selected course and term.";
                return;
            }

            CurriculumSubjects = (_curriculumService.GetCurriculumSubjects(curriculumId.Value) ?? new DataTable()).DefaultView;
            StatusMessage = "Loaded curriculum subjects from the academic database.";
        }

        private string BuildCurriculumCode()
        {
            var parts = new List<string>();
            AddCodePart(parts, SelectedCourse == null ? null : SelectedCourse.Subtitle);
            AddCodePart(parts, SelectedYearLevel == null ? null : SelectedYearLevel.Title);
            AddCodePart(parts, SelectedSemester == null ? null : SelectedSemester.Title);
            AddCodePart(parts, SelectedAcademicYear == null ? null : SelectedAcademicYear.Title);

            return parts.Count == 0
                ? "No curriculum code selected"
                : string.Join("-", parts);
        }

        private string BuildCurriculumName()
        {
            var parts = new List<string>();
            AddNamePart(parts, SelectedCourse == null ? null : SelectedCourse.DisplayName);
            AddNamePart(parts, SelectedYearLevel == null ? null : SelectedYearLevel.Title);
            AddNamePart(parts, SelectedSemester == null ? null : SelectedSemester.Title);
            AddNamePart(parts, SelectedAcademicYear == null ? null : SelectedAcademicYear.Title);

            return parts.Count == 0
                ? "Select course, year level, semester, and academic year"
                : string.Join(" | ", parts);
        }

        private void RaiseCurriculumIdentityChanged()
        {
            OnPropertyChanged(nameof(SelectedCurriculumCode));
            OnPropertyChanged(nameof(SelectedCurriculumName));
        }

        private static void LoadLookupCollection(ObservableCollection<LookupOptionViewModel> target, DataTable table, string idColumn, string titleColumn, string subtitleColumn)
        {
            target.Clear();
            table = table ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                target.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row[idColumn]),
                    Title = Convert.ToString(row[titleColumn]),
                    Subtitle = !string.IsNullOrWhiteSpace(subtitleColumn) && table.Columns.Contains(subtitleColumn)
                        ? Convert.ToString(row[subtitleColumn])
                        : string.Empty
                });
            }
        }

        private static void AddCodePart(ICollection<string> parts, string value)
        {
            var normalized = NormalizeCodePart(value);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                parts.Add(normalized);
            }
        }

        private static void AddNamePart(ICollection<string> parts, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parts.Add(value.Trim());
            }
        }

        private static string NormalizeCodePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = new List<char>();
            foreach (var c in value.Trim().ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(c) || c == '-')
                {
                    chars.Add(c);
                }
            }

            return new string(chars.ToArray());
        }
    }
}
