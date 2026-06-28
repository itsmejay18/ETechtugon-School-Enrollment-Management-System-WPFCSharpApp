using System;
using System.Collections.ObjectModel;
using School_Management_System.Wpf.Infrastructure;

namespace School_Management_System.Wpf.ViewModels.Academic
{
    public sealed class LegacyAcademicWorkspaceViewModel : ViewModelBase
    {
        private string _selectedLabType;
        private string _offeringType;
        private string _totalUnitsText;

        public LegacyAcademicWorkspaceViewModel(string formKey)
        {
            FormKey = NormalizeFormKey(formKey);
            Title = ResolveTitle(FormKey);
            Description = ResolveDescription(FormKey);
            StatusText = "Encoding layout ready. Fields are arranged to match the requested registrar workflow.";
            MatrixTitle = ResolveMatrixTitle(FormKey);

            StudentHeaderFields = CreateFields(
                "Student ID No.",
                "Family Name",
                "First Name",
                "Middle Name",
                "Suffix",
                "Contact No.");

            StudentPersonalFields = new ObservableCollection<FormFieldViewModel>
            {
                new FormFieldViewModel("Permanent Address", string.Empty, 420, true),
                new FormFieldViewModel("Zip Code"),
                new FormFieldViewModel("BirthDate"),
                new FormFieldViewModel("BirthPlace"),
                new FormFieldViewModel("Gender"),
                new FormFieldViewModel("Civil Status"),
                new FormFieldViewModel("Citizenship"),
                new FormFieldViewModel("Religion")
            };

            StudentOtherInfoFields = new ObservableCollection<FormFieldViewModel>
            {
                new FormFieldViewModel("Father's Name"),
                new FormFieldViewModel("Mother's Name"),
                new FormFieldViewModel("Address of Parents", string.Empty, 420, true),
                new FormFieldViewModel("Spouse Name"),
                new FormFieldViewModel("Address of Spouse", string.Empty, 420, true),
                new FormFieldViewModel("Guardian Name"),
                new FormFieldViewModel("Contact No. of Guardian")
            };

            StudentFooterFields = CreateFields(
                "NSTP Serial No.",
                "Payment Scheme",
                "Current SY",
                "Current SEM");

            SubjectCoreFields = CreateFields(
                "Subject Code",
                "SubjCode to display in TOR",
                "Description");

            SubjectUnitsFields = CreateFields(
                "Lec Hours",
                "Lab Hours",
                "Lec Units",
                "Lab Units",
                "Credit");

            SubjectOtherFields = CreateFields(
                "# of Weeks",
                "Faculty Credit",
                "Department");

            LabTypeOptions = new ObservableCollection<string> { "BLANK", "C", "L" };
            SelectedLabType = LabTypeOptions[0];

            OfferedSubjectFields = CreateFields(
                "Semester",
                "Subject",
                "Section",
                "Block",
                "Max Stud",
                "Enrolled Stud",
                "Faculty",
                "Faculty Units",
                "For Department",
                "For College");
            OfferingType = "Regular";
            LectureSchedules = new ObservableCollection<LectureScheduleRow>
            {
                new LectureScheduleRow { Day = "MWF", Time = "08:00-09:00", Room = "101" },
                new LectureScheduleRow { Day = "TTH", Time = "10:30-12:00", Room = "Lab 1" }
            };

            BlockCodes = new ObservableCollection<BlockCodeItem>
            {
                new BlockCodeItem { Code = "BSIT-1A", Description = "First Year IT" },
                new BlockCodeItem { Code = "BSBA-2B", Description = "Second Year BA" },
                new BlockCodeItem { Code = "BSED-3A", Description = "Third Year Education" }
            };
            BlockSchedules = new ObservableCollection<BlockScheduleRow>
            {
                new BlockScheduleRow { Subcode = "IT101", Section = "A", Days = "MWF", Time = "08:00-09:00" },
                new BlockScheduleRow { Subcode = "MATH1", Section = "A", Days = "TTH", Time = "09:00-10:30" },
                new BlockScheduleRow { Subcode = "ENG1", Section = "A", Days = "F", Time = "13:00-16:00" }
            };

            FeeMatrixRows = new ObservableCollection<FeeMatrixRow>
            {
                new FeeMatrixRow { ProgCode = "BSIT", NightClass = false, AmountPerUnit = "650.00", SY = "2026-2027", Semester = "1st" },
                new FeeMatrixRow { ProgCode = "BSBA", NightClass = false, AmountPerUnit = "625.00", SY = "2026-2027", Semester = "1st" },
                new FeeMatrixRow { ProgCode = "BSIT", NightClass = true, AmountPerUnit = "700.00", SY = "2026-2027", Semester = "1st" }
            };

            ValidGradeRows = new ObservableCollection<ValidGradeRow>
            {
                new ValidGradeRow { Grade = "1.00", GradeEquiv = "Excellent", AllowCompletionGrade = false, NeedCompletion = false, PassingGrade = true, Remarks = "Passed" },
                new ValidGradeRow { Grade = "3.00", GradeEquiv = "Passed", AllowCompletionGrade = false, NeedCompletion = false, PassingGrade = true, Remarks = "Passed" },
                new ValidGradeRow { Grade = "5.00", GradeEquiv = "Failed", AllowCompletionGrade = false, NeedCompletion = false, PassingGrade = false, Remarks = "Failed" },
                new ValidGradeRow { Grade = "INC", GradeEquiv = "Incomplete", AllowCompletionGrade = true, NeedCompletion = true, PassingGrade = false, Remarks = "INC" }
            };

            SemesterRows = new ObservableCollection<SemesterSetupRow>
            {
                new SemesterSetupRow { SY = "2026-2027", Semester = "1st", Control = true, Current = true, Block = false, CorRemarks = "Open enrollment" },
                new SemesterSetupRow { SY = "2026-2027", Semester = "2nd", Control = false, Current = false, Block = false, CorRemarks = "Pending" },
                new SemesterSetupRow { SY = "2025-2026", Semester = "Summer", Control = false, Current = false, Block = true, CorRemarks = "Closed" }
            };

            RegistrationAccessToggles = new ObservableCollection<AccessToggleViewModel>
            {
                new AccessToggleViewModel("Registrar", true),
                new AccessToggleViewModel("Clerk", true),
                new AccessToggleViewModel("Controller", false),
                new AccessToggleViewModel("View Only", false)
            };

            RegistrationStudentFields = CreateFields(
                "I.D. No.",
                "Name",
                "Sex",
                "School Year",
                "Term",
                "Date",
                "Course",
                "Year",
                "GPA",
                "GPA Previous Semester",
                "Scholarship Status");

            EnrolledSubjects = new ObservableCollection<EnrolledSubjectRow>
            {
                new EnrolledSubjectRow { CourseNo = "IT101", Sctn = "A", DescriptiveTitle = "Introduction to Computing", Time = "08:00-09:00", Days = "MWF", Room = "101", Lec = "3", Lab = "0", Grade = string.Empty, Compl = string.Empty },
                new EnrolledSubjectRow { CourseNo = "IT102", Sctn = "A", DescriptiveTitle = "Computer Programming 1", Time = "10:30-12:00", Days = "TTH", Room = "Lab 1", Lec = "2", Lab = "1", Grade = string.Empty, Compl = string.Empty }
            };
            TotalUnitsText = "Total Units: 6";

            PrerequisiteRows = new ObservableCollection<PrerequisiteRow>
            {
                new PrerequisiteRow { SubjectCode = "IT102", Description = "Computer Programming 1", Prerequisite = "IT101" },
                new PrerequisiteRow { SubjectCode = "IT201", Description = "Data Structures", Prerequisite = "IT102" }
            };

            GenericFields = CreateFields("Code", "Description", "Category", "Remarks");
            GenericRows = CreateGenericRows(FormKey);
        }

        public string FormKey { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string StatusText { get; private set; }
        public string MatrixTitle { get; private set; }

        public ObservableCollection<FormFieldViewModel> StudentHeaderFields { get; private set; }
        public ObservableCollection<FormFieldViewModel> StudentPersonalFields { get; private set; }
        public ObservableCollection<FormFieldViewModel> StudentOtherInfoFields { get; private set; }
        public ObservableCollection<FormFieldViewModel> StudentFooterFields { get; private set; }
        public ObservableCollection<FormFieldViewModel> SubjectCoreFields { get; private set; }
        public ObservableCollection<FormFieldViewModel> SubjectUnitsFields { get; private set; }
        public ObservableCollection<FormFieldViewModel> SubjectOtherFields { get; private set; }
        public ObservableCollection<string> LabTypeOptions { get; private set; }
        public ObservableCollection<FormFieldViewModel> OfferedSubjectFields { get; private set; }
        public ObservableCollection<LectureScheduleRow> LectureSchedules { get; private set; }
        public ObservableCollection<BlockCodeItem> BlockCodes { get; private set; }
        public ObservableCollection<BlockScheduleRow> BlockSchedules { get; private set; }
        public ObservableCollection<FeeMatrixRow> FeeMatrixRows { get; private set; }
        public ObservableCollection<ValidGradeRow> ValidGradeRows { get; private set; }
        public ObservableCollection<SemesterSetupRow> SemesterRows { get; private set; }
        public ObservableCollection<AccessToggleViewModel> RegistrationAccessToggles { get; private set; }
        public ObservableCollection<FormFieldViewModel> RegistrationStudentFields { get; private set; }
        public ObservableCollection<EnrolledSubjectRow> EnrolledSubjects { get; private set; }
        public ObservableCollection<PrerequisiteRow> PrerequisiteRows { get; private set; }
        public ObservableCollection<FormFieldViewModel> GenericFields { get; private set; }
        public ObservableCollection<SimpleSetupRow> GenericRows { get; private set; }

        public string SelectedLabType
        {
            get { return _selectedLabType; }
            set { SetProperty(ref _selectedLabType, value); }
        }

        public bool IncludeGradeInGpa { get; set; }

        public string OfferingType
        {
            get { return _offeringType; }
            set { SetProperty(ref _offeringType, value); }
        }

        public string TotalUnitsText
        {
            get { return _totalUnitsText; }
            set { SetProperty(ref _totalUnitsText, value); }
        }

        public bool IsStudentDataFormVisible { get { return IsForm("StudentData"); } }
        public bool IsSubjectEntryFormVisible { get { return IsForm("SubjectEntry"); } }
        public bool IsPrerequisiteFormVisible { get { return IsForm("SubjectPrerequisite"); } }
        public bool IsOfferedSubjectFormVisible { get { return IsForm("SubjectOffering"); } }
        public bool IsBlockSetupVisible { get { return IsForm("BlockSetup"); } }
        public bool IsFeeMatrixVisible { get { return FormKey.EndsWith("Matrix", StringComparison.OrdinalIgnoreCase); } }
        public bool IsValidGradesVisible { get { return IsForm("ValidGrades"); } }
        public bool IsSemesterListVisible { get { return IsForm("SemesterList") || IsForm("ChangeSemester"); } }
        public bool IsRegistrationFormVisible { get { return IsForm("Registration"); } }

        public bool IsGenericSetupVisible
        {
            get
            {
                return !IsStudentDataFormVisible &&
                       !IsSubjectEntryFormVisible &&
                       !IsPrerequisiteFormVisible &&
                       !IsOfferedSubjectFormVisible &&
                       !IsBlockSetupVisible &&
                       !IsFeeMatrixVisible &&
                       !IsValidGradesVisible &&
                       !IsSemesterListVisible &&
                       !IsRegistrationFormVisible;
            }
        }

        private bool IsForm(string key)
        {
            return string.Equals(FormKey, key, StringComparison.OrdinalIgnoreCase);
        }

        private static ObservableCollection<FormFieldViewModel> CreateFields(params string[] labels)
        {
            var fields = new ObservableCollection<FormFieldViewModel>();
            foreach (var label in labels)
            {
                fields.Add(new FormFieldViewModel(label));
            }

            return fields;
        }

        private static ObservableCollection<SimpleSetupRow> CreateGenericRows(string formKey)
        {
            var moduleName = ResolveTitle(formKey);
            return new ObservableCollection<SimpleSetupRow>
            {
                new SimpleSetupRow { Code = "001", Description = moduleName + " record", Category = "Active", Remarks = "Ready" },
                new SimpleSetupRow { Code = "002", Description = moduleName + " reference", Category = "For review", Remarks = "Editable" }
            };
        }

        private static string NormalizeFormKey(string formKey)
        {
            return string.IsNullOrWhiteSpace(formKey) ? "FileManagement" : formKey.Trim();
        }

        private static string ResolveMatrixTitle(string formKey)
        {
            switch (NormalizeFormKey(formKey))
            {
                case "MiscFeeMatrix":
                    return "Misc Fee Matrix";
                case "TuitionMatrix":
                    return "Tuition Matrix";
                case "LaboratoryMatrix":
                    return "Laboratory Matrix";
                case "CompFeeMatrix":
                    return "Comp Fee Matrix";
                case "EntranceFeeMatrix":
                    return "Entrance Fee Matrix";
                default:
                    return "Tuition Matrix";
            }
        }

        private static string ResolveTitle(string formKey)
        {
            switch (NormalizeFormKey(formKey))
            {
                case "FileManagement":
                    return "File Management";
                case "StudentData":
                    return "Student Data Form";
                case "SubjectEntry":
                    return "Subject Entry Form";
                case "SubjectPrerequisite":
                    return "Subject Pre-requisite";
                case "ProgramMajor":
                    return "Program/Major";
                case "SubjectOffering":
                    return "Offered Subject Entry Form";
                case "BlockSetup":
                    return "Block Sections Setup";
                case "Registration":
                    return "Subject Registration Form";
                case "GradingModule":
                    return "Grading Module";
                case "Cashier":
                    return "Cashier";
                case "SemesterList":
                    return "Semester List Setup";
                case "ChangeSemester":
                    return "Change Semester";
                case "College":
                    return "College";
                case "Department":
                    return "Department";
                case "FacultyData":
                    return "Faculty Data";
                case "FacultyEntry":
                    return "Faculty Entry";
                case "BlockList":
                    return "Block List";
                case "Scholarship":
                    return "Scholarship";
                case "Fees":
                    return "Fees";
                case "ValidGrades":
                    return "Valid Grades Setup";
                case "ClassRooms":
                    return "Class Rooms";
                case "MiscFeeMatrix":
                case "TuitionMatrix":
                case "LaboratoryMatrix":
                case "CompFeeMatrix":
                case "EntranceFeeMatrix":
                    return ResolveMatrixTitle(formKey);
                default:
                    return "Administrative Setup";
            }
        }

        private static string ResolveDescription(string formKey)
        {
            switch (NormalizeFormKey(formKey))
            {
                case "StudentData":
                    return "Encode complete student profile fields including personal data, other info, and footer registration tags.";
                case "SubjectEntry":
                    return "Maintain subject codes, TOR display code, description, units, hours, credits, lab type, and GPA inclusion.";
                case "SubjectOffering":
                    return "Prepare semester subject offerings with section, block, faculty load, department, college, and lecture schedule.";
                case "BlockSetup":
                    return "Maintain block codes and linked subject schedules.";
                case "Registration":
                    return "Enrollment window for user access toggles, student header, enrolled subjects, action buttons, and total units.";
                case "ValidGrades":
                    return "Configure grades, equivalents, completion behavior, passing flags, and remarks.";
                case "SemesterList":
                case "ChangeSemester":
                    return "Maintain school year and semester controls used by COR and enrollment workflows.";
                default:
                    if (NormalizeFormKey(formKey).EndsWith("Matrix", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Maintain program-based fee amount per unit by school year, semester, and night-class flag.";
                    }

                    return "Reference encoding screen connected from the main menu hierarchy.";
            }
        }
    }

    public sealed class FormFieldViewModel : ViewModelBase
    {
        private string _value;

        public FormFieldViewModel(string label)
            : this(label, string.Empty, 190, false)
        {
        }

        public FormFieldViewModel(string label, string value, double width, bool isMultiline)
        {
            Label = label ?? string.Empty;
            _value = value ?? string.Empty;
            Width = width;
            IsMultiline = isMultiline;
        }

        public string Label { get; private set; }
        public double Width { get; private set; }
        public bool IsMultiline { get; private set; }

        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }

    public sealed class AccessToggleViewModel : ViewModelBase
    {
        private bool _enabled;

        public AccessToggleViewModel(string label, bool enabled)
        {
            Label = label ?? string.Empty;
            _enabled = enabled;
        }

        public string Label { get; private set; }

        public bool Enabled
        {
            get { return _enabled; }
            set { SetProperty(ref _enabled, value); }
        }
    }

    public sealed class LectureScheduleRow
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public string Room { get; set; }
    }

    public sealed class BlockCodeItem
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string DisplayName { get { return string.IsNullOrWhiteSpace(Description) ? Code : Code + " - " + Description; } }
    }

    public sealed class BlockScheduleRow
    {
        public string Subcode { get; set; }
        public string Section { get; set; }
        public string Days { get; set; }
        public string Time { get; set; }
    }

    public sealed class FeeMatrixRow
    {
        public string ProgCode { get; set; }
        public bool NightClass { get; set; }
        public string AmountPerUnit { get; set; }
        public string SY { get; set; }
        public string Semester { get; set; }
    }

    public sealed class ValidGradeRow
    {
        public string Grade { get; set; }
        public string GradeEquiv { get; set; }
        public bool AllowCompletionGrade { get; set; }
        public bool NeedCompletion { get; set; }
        public bool PassingGrade { get; set; }
        public string Remarks { get; set; }
    }

    public sealed class SemesterSetupRow
    {
        public string SY { get; set; }
        public string Semester { get; set; }
        public bool Control { get; set; }
        public bool Current { get; set; }
        public bool Block { get; set; }
        public string CorRemarks { get; set; }
    }

    public sealed class EnrolledSubjectRow
    {
        public string CourseNo { get; set; }
        public string Sctn { get; set; }
        public string DescriptiveTitle { get; set; }
        public string Time { get; set; }
        public string Days { get; set; }
        public string Room { get; set; }
        public string Lec { get; set; }
        public string Lab { get; set; }
        public string Grade { get; set; }
        public string Compl { get; set; }
    }

    public sealed class PrerequisiteRow
    {
        public string SubjectCode { get; set; }
        public string Description { get; set; }
        public string Prerequisite { get; set; }
    }

    public sealed class SimpleSetupRow
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Remarks { get; set; }
    }
}
