using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Views.Enrollment;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Enrollment
{
    public sealed class EnrollmentWorkspaceViewModel : ViewModelBase
    {
        private readonly StudentService _studentService;
        private readonly EnrollmentService _enrollmentService;
        private readonly CourseService _courseService;
        private readonly LookupService _lookupService;
        private readonly SectionService _sectionService;
        private readonly CurriculumService _curriculumService;
        private readonly ClassScheduleService _classScheduleService;
        private readonly SystemSettingService _systemSettingService;
        private string _studentSearchText;
        private DataView _studentRecords;
        private DataRowView _selectedStudent;
        private LookupOptionViewModel _selectedCourse;
        private LookupOptionViewModel _selectedAcademicYear;
        private LookupOptionViewModel _selectedYearLevel;
        private LookupOptionViewModel _selectedSemester;
        private LookupOptionViewModel _selectedSection;
        private bool _useSectionMode;
        private string _statusMessage;
        private string _selectedStudentSummary;
        private string _summaryText;
        private bool _enrollmentSaved;
        private string _savedEnrollmentNumber;

        public EnrollmentWorkspaceViewModel(StudentService studentService, EnrollmentService enrollmentService, CourseService courseService, LookupService lookupService, SectionService sectionService, CurriculumService curriculumService, ClassScheduleService classScheduleService, SystemSettingService systemSettingService)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _curriculumService = curriculumService ?? throw new ArgumentNullException(nameof(curriculumService));
            _classScheduleService = classScheduleService ?? throw new ArgumentNullException(nameof(classScheduleService));
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));

            Courses = new ObservableCollection<LookupOptionViewModel>();
            AcademicYears = new ObservableCollection<LookupOptionViewModel>();
            YearLevels = new ObservableCollection<LookupOptionViewModel>();
            Semesters = new ObservableCollection<LookupOptionViewModel>();
            Sections = new ObservableCollection<LookupOptionViewModel>();
            AvailableSubjects = new ObservableCollection<EnrollmentSubjectOptionViewModel>();

            SearchStudentsCommand = new RelayCommand(LoadStudents);
            RefreshLookupsCommand = new RelayCommand(RefreshLookups);
            LoadSubjectsCommand = new RelayCommand(LoadSubjects);
            SaveEnrollmentCommand = new RelayCommand(SaveEnrollment, () => CanSaveEnrollment);
            PrintCorCommand = new RelayCommand(PrintCor, () => CanPrintCor);

            UseSectionMode = true;
            SelectedStudentSummary = "Select a student to begin building an enrollment.";
            SummaryText = "Choose the academic references and load subjects to prepare the enrollment summary.";
            LoadStudents();
            RefreshLookups();
        }

        public ObservableCollection<LookupOptionViewModel> Courses { get; private set; }
        public ObservableCollection<LookupOptionViewModel> AcademicYears { get; private set; }
        public ObservableCollection<LookupOptionViewModel> YearLevels { get; private set; }
        public ObservableCollection<LookupOptionViewModel> Semesters { get; private set; }
        public ObservableCollection<LookupOptionViewModel> Sections { get; private set; }
        public ObservableCollection<EnrollmentSubjectOptionViewModel> AvailableSubjects { get; private set; }
        public RelayCommand SearchStudentsCommand { get; private set; }
        public RelayCommand RefreshLookupsCommand { get; private set; }
        public RelayCommand LoadSubjectsCommand { get; private set; }
        public RelayCommand SaveEnrollmentCommand { get; private set; }
        public RelayCommand PrintCorCommand { get; private set; }

        public string StudentSearchText { get { return _studentSearchText; } set { SetProperty(ref _studentSearchText, value); } }
        public DataView StudentRecords { get { return _studentRecords; } private set { SetProperty(ref _studentRecords, value); } }
        public DataRowView SelectedStudent
        {
            get { return _selectedStudent; }
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    InvalidateSavedEnrollment();
                    UpdateSelectedStudentSummary();
                    RaiseCommandStates();
                }
            }
        }
        public LookupOptionViewModel SelectedCourse
        {
            get { return _selectedCourse; }
            set { if (SetProperty(ref _selectedCourse, value)) { LoadSections(); LoadSubjects(); } }
        }
        public LookupOptionViewModel SelectedAcademicYear
        {
            get { return _selectedAcademicYear; }
            set { if (SetProperty(ref _selectedAcademicYear, value)) { LoadSections(); LoadSubjects(); } }
        }
        public LookupOptionViewModel SelectedYearLevel
        {
            get { return _selectedYearLevel; }
            set { if (SetProperty(ref _selectedYearLevel, value)) { LoadSections(); LoadSubjects(); } }
        }
        public LookupOptionViewModel SelectedSemester
        {
            get { return _selectedSemester; }
            set { if (SetProperty(ref _selectedSemester, value)) { LoadSections(); LoadSubjects(); } }
        }
        public LookupOptionViewModel SelectedSection
        {
            get { return _selectedSection; }
            set { if (SetProperty(ref _selectedSection, value)) { LoadSubjects(); } }
        }
        public bool UseSectionMode
        {
            get { return _useSectionMode; }
            set { if (SetProperty(ref _useSectionMode, value)) { LoadSubjects(); } }
        }
        public string StatusMessage { get { return _statusMessage; } private set { SetProperty(ref _statusMessage, value); } }
        public string SelectedStudentSummary { get { return _selectedStudentSummary; } private set { SetProperty(ref _selectedStudentSummary, value); } }
        public string SummaryText { get { return _summaryText; } private set { SetProperty(ref _summaryText, value); } }
        public string TotalUnitsText { get { return AvailableSubjects.Where(s => s.IsSelected).Sum(s => s.Units) + " total unit(s) selected"; } }
        public string EnrollmentNumberPreview { get { return _enrollmentService.GetNextEnrollmentNumber(); } }
        public bool CanSaveEnrollment { get { return !_enrollmentSaved && SelectedStudent != null && AvailableSubjects.Any(s => s.IsSelected); } }
        public bool CanPrintCor { get { return SelectedStudent != null && AvailableSubjects.Any(s => s.IsSelected); } }

        private void LoadStudents()
        {
            StudentRecords = (_studentService.GetStudents(StudentSearchText ?? string.Empty) ?? new DataTable()).DefaultView;
            SelectedStudent = StudentRecords.Count > 0 ? StudentRecords[0] : null;
            StatusMessage = StudentRecords.Count > 0 ? "Loaded students from the live database." : "No students matched the current search.";
        }

        private void RefreshLookups()
        {
            LoadLookupOptions(Courses, _courseService.GetLookupCourses(), "CourseId", "CourseName", "CourseCode");
            LoadLookupOptions(AcademicYears, _lookupService.GetAcademicYears(), "AcademicYearId", "Name", null);
            LoadLookupOptions(YearLevels, _lookupService.GetYearLevels(), "YearLevelId", "Name", null);
            LoadLookupOptions(Semesters, _lookupService.GetSemesters(), "SemesterId", "Name", null);

            var activeTerm = _systemSettingService.GetActiveTerm();
            if (SelectedCourse == null && Courses.Count > 0) SelectedCourse = Courses[0];
            if (SelectedAcademicYear == null && AcademicYears.Count > 0) SelectedAcademicYear = activeTerm.AcademicYearId.HasValue ? FindOption(AcademicYears, activeTerm.AcademicYearId.Value) : AcademicYears[0];
            if (SelectedYearLevel == null && YearLevels.Count > 0) SelectedYearLevel = YearLevels[0];
            if (SelectedSemester == null && Semesters.Count > 0) SelectedSemester = activeTerm.SemesterId.HasValue ? FindOption(Semesters, activeTerm.SemesterId.Value) : Semesters[0];
            LoadSections();
            LoadSubjects();
        }

        private void LoadSections()
        {
            Sections.Clear();
            DataTable table = SelectedCourse != null && SelectedAcademicYear != null && SelectedSemester != null
                ? _sectionService.GetSectionsForTerm(SelectedCourse.Id, SelectedAcademicYear.Id, SelectedSemester.Id, SelectedYearLevel == null ? (int?)null : SelectedYearLevel.Id)
                : _sectionService.GetSections(string.Empty);

            table = table ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                Sections.Add(new LookupOptionViewModel
                {
                    Id = Convert.ToInt32(row["SectionId"]),
                    Title = Convert.ToString(row["SectionName"]),
                    Subtitle = table.Columns.Contains("CourseName") ? Convert.ToString(row["CourseName"]) : string.Empty,
                    Tag = row
                });
            }

            if (Sections.Count > 0)
            {
                if (SelectedSection == null || FindOption(Sections, SelectedSection.Id) == null)
                {
                    SelectedSection = Sections[0];
                }
            }
            else
            {
                SelectedSection = null;
            }
        }

        private void LoadSubjects()
        {
            InvalidateSavedEnrollment();

            foreach (var item in AvailableSubjects)
            {
                item.PropertyChanged -= SubjectItem_PropertyChanged;
            }

            AvailableSubjects.Clear();

            if (UseSectionMode)
            {
                LoadSubjectsBySection();
            }
            else
            {
                LoadSubjectsByCurriculum();
            }

            BuildSummary();
            RaiseCommandStates();
            OnPropertyChanged(nameof(TotalUnitsText));
        }

        private void LoadSubjectsBySection()
        {
            if (SelectedSection == null)
            {
                StatusMessage = "Select a section to load scheduled subjects.";
                return;
            }

            var table = _classScheduleService.GetBySection(SelectedSection.Id) ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                AddSubjectOption(new EnrollmentSubjectOptionViewModel
                {
                    SubjectId = Convert.ToInt32(row["SubjectId"]),
                    SubjectCode = Convert.ToString(row["SubjectCode"]),
                    SubjectName = Convert.ToString(row["SubjectName"]),
                    Units = row.Table.Columns.Contains("Units") ? Convert.ToInt32(row["Units"]) : 0,
                    ClassScheduleId = Convert.ToInt32(row["ClassScheduleId"]),
                    ScheduleText = BuildScheduleText(row),
                    IsSelected = true
                });
            }

            StatusMessage = AvailableSubjects.Count > 0 ? "Loaded section schedule subjects from the database." : "No scheduled subjects were found for the selected section.";
        }

        private void LoadSubjectsByCurriculum()
        {
            if (SelectedCourse == null || SelectedAcademicYear == null || SelectedYearLevel == null || SelectedSemester == null)
            {
                StatusMessage = "Select the course and active term to load curriculum subjects.";
                return;
            }

            var curriculumId = _curriculumService.TryGetCurriculumId(SelectedCourse.Id, SelectedYearLevel.Id, SelectedSemester.Id, SelectedAcademicYear.Id);
            if (!curriculumId.HasValue)
            {
                StatusMessage = "No curriculum was found for the selected course and term.";
                return;
            }

            var table = _curriculumService.GetCurriculumSubjects(curriculumId.Value) ?? new DataTable();
            foreach (DataRow row in table.Rows)
            {
                AddSubjectOption(new EnrollmentSubjectOptionViewModel
                {
                    SubjectId = Convert.ToInt32(row["SubjectId"]),
                    SubjectCode = Convert.ToString(row["SubjectCode"]),
                    SubjectName = Convert.ToString(row["SubjectName"]),
                    Units = row.Table.Columns.Contains("Units") ? Convert.ToInt32(row["Units"]) : 0,
                    ScheduleText = string.Empty,
                    IsSelected = true
                });
            }

            StatusMessage = AvailableSubjects.Count > 0 ? "Loaded curriculum subjects from the database." : "No curriculum subjects were found for the selected course and term.";
        }

        private void SaveEnrollment()
        {
            TrySaveEnrollment(true);
        }

        private bool TrySaveEnrollment(bool showSavedMessage)
        {
            if (!CanSaveEnrollment || SelectedStudent == null) return false;

            try
            {
                var details = BuildSelectedEnrollmentDetails();
                var enrollment = new School_Management_System.Models.Enrollment
                {
                    EnrollmentNumber = _enrollmentService.GetNextEnrollmentNumber(),
                    StudentId = Convert.ToInt32(SelectedStudent["StudentId"]),
                    CourseId = SelectedCourse == null ? 0 : SelectedCourse.Id,
                    AcademicYearId = SelectedAcademicYear == null ? 0 : SelectedAcademicYear.Id,
                    YearLevelId = SelectedYearLevel == null ? 0 : SelectedYearLevel.Id,
                    SemesterId = SelectedSemester == null ? 0 : SelectedSemester.Id,
                    SectionId = SelectedSection == null ? 0 : SelectedSection.Id,
                    EnrollDate = DateTime.Now.Date,
                    Status = "Posted"
                };

                var validation = _enrollmentService.Validate(enrollment, details);
                if (!validation.IsValid)
                {
                    MessageBox.Show(validation.ToString(), "Enrollment Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                _enrollmentService.Save(enrollment, details);
                _enrollmentSaved = true;
                _savedEnrollmentNumber = enrollment.EnrollmentNumber;
                StatusMessage = "Enrollment saved successfully as " + enrollment.EnrollmentNumber + ".";
                SummaryText = BuildSummaryText(enrollment.EnrollmentNumber);
                RaiseCommandStates();

                if (showSavedMessage)
                {
                    MessageBox.Show("Enrollment saved successfully.\n\n" + enrollment.EnrollmentNumber, "Enrollment Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Enrollment", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void UpdateSelectedStudentSummary()
        {
            if (SelectedStudent == null)
            {
                SelectedStudentSummary = "Select a student to begin building an enrollment.";
            }
            else
            {
                SelectedStudentSummary = Convert.ToString(SelectedStudent["StudentNumber"]) + " | " + Convert.ToString(SelectedStudent["LastName"]) + ", " + Convert.ToString(SelectedStudent["FirstName"]);
            }

            BuildSummary();
        }

        private void BuildSummary()
        {
            SummaryText = BuildSummaryText(_enrollmentSaved ? _savedEnrollmentNumber : EnrollmentNumberPreview);
        }

        private string BuildSummaryText(string enrollmentNumber)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Enrollment No.: " + (enrollmentNumber ?? string.Empty));
            sb.AppendLine("Student: " + (SelectedStudentSummary ?? "(none)"));
            sb.AppendLine("Course: " + (SelectedCourse == null ? "(none)" : SelectedCourse.Title));
            sb.AppendLine("Academic Year: " + (SelectedAcademicYear == null ? "(none)" : SelectedAcademicYear.Title));
            sb.AppendLine("Year Level: " + (SelectedYearLevel == null ? "(none)" : SelectedYearLevel.Title));
            sb.AppendLine("Semester: " + (SelectedSemester == null ? "(none)" : SelectedSemester.Title));
            sb.AppendLine("Section: " + (SelectedSection == null ? "(none)" : SelectedSection.Title));
            sb.AppendLine("Selection Mode: " + (UseSectionMode ? "By Section" : "By Subject"));
            sb.AppendLine("Status: " + (_enrollmentSaved ? "Posted / Ready for COR printing" : "Pending save"));
            sb.AppendLine("Subjects Selected: " + AvailableSubjects.Count(s => s.IsSelected));
            sb.AppendLine("Total Units: " + AvailableSubjects.Where(s => s.IsSelected).Sum(s => s.Units));
            return sb.ToString().Trim();
        }

        private void AddSubjectOption(EnrollmentSubjectOptionViewModel option)
        {
            option.PropertyChanged += SubjectItem_PropertyChanged;
            AvailableSubjects.Add(option);
        }

        private void SubjectItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(EnrollmentSubjectOptionViewModel.IsSelected), StringComparison.Ordinal))
            {
                InvalidateSavedEnrollment();
                OnPropertyChanged(nameof(TotalUnitsText));
                BuildSummary();
                RaiseCommandStates();
            }
        }

        private void PrintCor()
        {
            if (!CanPrintCor)
            {
                return;
            }

            try
            {
                if (!_enrollmentSaved)
                {
                    var saveFirst = MessageBox.Show(
                        "The enrollment must be saved before printing the COR.\n\nSave it now?",
                        "Print COR",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (saveFirst != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    if (!TrySaveEnrollment(false))
                    {
                        return;
                    }
                }

                var corData = BuildCorPreviewData(_savedEnrollmentNumber ?? EnrollmentNumberPreview, true);
                if (corData == null || (corData.Subjects.Count == 0 && corData.SummaryRows.Count == 0))
                {
                    MessageBox.Show("Nothing to print yet. Select a student and subjects first.", "Print COR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var preview = new CorPrintPreviewWindow(corData)
                {
                    Owner = Application.Current != null ? Application.Current.MainWindow : null
                };

                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to print the Certificate of Registration.\n\n" + ex.Message, "Print COR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RaiseCommandStates()
        {
            SaveEnrollmentCommand.RaiseCanExecuteChanged();
            PrintCorCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(CanSaveEnrollment));
            OnPropertyChanged(nameof(CanPrintCor));
        }

        private void InvalidateSavedEnrollment()
        {
            _enrollmentSaved = false;
            _savedEnrollmentNumber = null;
        }

        private List<EnrollmentDetail> BuildSelectedEnrollmentDetails()
        {
            return AvailableSubjects
                .Where(s => s.IsSelected)
                .Select(s => new EnrollmentDetail
                {
                    SubjectId = s.SubjectId,
                    Units = s.Units,
                    ClassScheduleId = s.ClassScheduleId
                })
                .ToList();
        }

        private CorPrintPreviewData BuildCorPreviewData(string enrollmentNumber, bool includeSavedNotice)
        {
            var details = AvailableSubjects.Where(s => s.IsSelected).ToList();
            if (SelectedStudent == null || details.Count == 0)
            {
                return null;
            }

            var totalUnits = details.Sum(s => s.Units);
            var tuitionPerUnit = _systemSettingService.GetDecimal(AppConstants.SettingKeys.TuitionPerUnit, 650m);
            var miscellaneousFee = _systemSettingService.GetDecimal(AppConstants.SettingKeys.MiscellaneousFee, 1850m);
            var registrationFee = _systemSettingService.GetDecimal(AppConstants.SettingKeys.RegistrationFee, 350m);
            var laboratoryFee = _systemSettingService.GetDecimal(AppConstants.SettingKeys.LaboratoryFee, 0m);
            var tuitionAmount = totalUnits * tuitionPerUnit;
            var totalAssessment = tuitionAmount + miscellaneousFee + registrationFee + laboratoryFee;

            var studentNumber = SelectedStudent == null ? string.Empty : Convert.ToString(SelectedStudent["StudentNumber"]);
            var studentName = SelectedStudent == null
                ? string.Empty
                : Convert.ToString(SelectedStudent["LastName"]) + ", " + Convert.ToString(SelectedStudent["FirstName"]);

            var data = new CorPrintPreviewData();
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Enrollment Number", Value = enrollmentNumber ?? string.Empty });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Student Number", Value = studentNumber });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Student Name", Value = studentName });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Course", Value = SelectedCourse == null ? string.Empty : SelectedCourse.DisplayName });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Academic Year", Value = SelectedAcademicYear == null ? string.Empty : SelectedAcademicYear.Title });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Year Level", Value = SelectedYearLevel == null ? string.Empty : SelectedYearLevel.Title });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Semester", Value = SelectedSemester == null ? string.Empty : SelectedSemester.Title });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Section", Value = SelectedSection == null ? string.Empty : SelectedSection.DisplayName });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Enrollment Type", Value = UseSectionMode ? "By Section (Regular)" : "By Subject (Irregular)" });
            data.SummaryRows.Add(new CorPrintKeyValueRow { Label = "Status", Value = includeSavedNotice ? "Posted / Ready for COR printing" : "Pending save" });

            foreach (var subject in details)
            {
                data.Subjects.Add(new CorPrintSubjectRow
                {
                    Code = subject.SubjectCode,
                    Subject = subject.SubjectName,
                    Units = subject.Units.ToString(),
                    Schedule = string.IsNullOrWhiteSpace(subject.ScheduleText) ? "-" : subject.ScheduleText
                });
            }

            data.AssessmentRows.Add(new CorPrintAssessmentRow
            {
                Item = "Total Units",
                Basis = totalUnits + " unit(s)",
                Amount = "-",
                IsEmphasized = false
            });
            data.AssessmentRows.Add(new CorPrintAssessmentRow
            {
                Item = "Tuition",
                Basis = totalUnits + " x " + tuitionPerUnit.ToString("N2"),
                Amount = tuitionAmount.ToString("N2"),
                IsEmphasized = false
            });
            data.AssessmentRows.Add(new CorPrintAssessmentRow
            {
                Item = "Miscellaneous Fee",
                Basis = "School fees",
                Amount = miscellaneousFee.ToString("N2"),
                IsEmphasized = false
            });
            data.AssessmentRows.Add(new CorPrintAssessmentRow
            {
                Item = "Registration Fee",
                Basis = "Processing",
                Amount = registrationFee.ToString("N2"),
                IsEmphasized = false
            });
            data.AssessmentRows.Add(new CorPrintAssessmentRow
            {
                Item = "Laboratory Fee",
                Basis = "Laboratory usage",
                Amount = laboratoryFee.ToString("N2"),
                IsEmphasized = false
            });
            data.AssessmentRows.Add(new CorPrintAssessmentRow
            {
                Item = "Total Assessment",
                Basis = "Amount due",
                Amount = totalAssessment.ToString("N2"),
                IsEmphasized = true
            });

            if (includeSavedNotice)
            {
                data.Notices.Add("Enrollment saved successfully.");
                data.Notices.Add("You may now print the Certificate of Registration.");
            }

            return data;
        }

        private static void LoadLookupOptions(ObservableCollection<LookupOptionViewModel> target, DataTable table, string idColumn, string titleColumn, string subtitleColumn)
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
                        : string.Empty,
                    Tag = row
                });
            }
        }

        private static LookupOptionViewModel FindOption(ObservableCollection<LookupOptionViewModel> options, int id)
        {
            foreach (var option in options)
            {
                if (option != null && option.Id == id) return option;
            }
            return options.Count > 0 ? options[0] : null;
        }

        private static string BuildScheduleText(DataRow row)
        {
            var day = row.Table.Columns.Contains("DayOfWeek") ? Convert.ToString(row["DayOfWeek"]) : string.Empty;
            var room = row.Table.Columns.Contains("Room") ? Convert.ToString(row["Room"]) : string.Empty;
            var start = row.Table.Columns.Contains("StartTime") && row["StartTime"] is TimeSpan ? ((TimeSpan)row["StartTime"]).ToString(@"hh\:mm") : string.Empty;
            var end = row.Table.Columns.Contains("EndTime") && row["EndTime"] is TimeSpan ? ((TimeSpan)row["EndTime"]).ToString(@"hh\:mm") : string.Empty;
            var time = string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end) ? string.Empty : start + "-" + end;
            return string.Join(" | ", new[] { day, time, room }.Where(v => !string.IsNullOrWhiteSpace(v)));
        }
    }
}
