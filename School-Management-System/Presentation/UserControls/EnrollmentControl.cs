using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class EnrollmentControl : BaseUserControl
    {
        private readonly EnrollmentService _enrollmentService;
        private readonly StudentService _studentService;
        private readonly CurriculumService _curriculumService;
        private readonly CourseService _courseService;
        private readonly LookupService _lookupService;

        private Panel _stepBar;
        private Label _step1Lbl;
        private Label _step2Lbl;
        private Label _step3Lbl;

        private Panel _container;
        private Panel _pStep1;
        private Panel _pStep2;
        private Panel _pStep3;

        // Step 1
        private TextBox _txtStudentSearch;
        private DataGridView _gridStudents;
        private Label _lblSelectedStudent;
        private Button _btnNext1;

        // Step 2
        private ComboBox _cmbCourse;
        private ComboBox _cmbAcademicYear;
        private ComboBox _cmbYearLevel;
        private ComboBox _cmbSemester;
        private Label _lblCurriculumStatus;
        private ListView _lvSubjects;
        private Label _lblTotalUnits;
        private Button _btnBack2;
        private Button _btnNext2;

        // Step 3
        private Label _lblSummary;
        private Button _btnBack3;
        private Button _btnSave;
        private Button _btnPrint;

        private int _selectedStudentId;
        private string _selectedStudentNumber;
        private string _selectedStudentName;
        private int? _curriculumId;
        private string _enrollmentNumber;

        private readonly PrintDocument _printDocument = new PrintDocument();
        private string _printText;

        public EnrollmentControl()
            : this(null, null, null, null, null)
        {
        }

        public EnrollmentControl(
            EnrollmentService enrollmentService,
            StudentService studentService,
            CurriculumService curriculumService,
            CourseService courseService,
            LookupService lookupService)
        {
            _enrollmentService = enrollmentService;
            _studentService = studentService;
            _curriculumService = curriculumService;
            _courseService = courseService;
            _lookupService = lookupService;

            InitializeComponent();
            WirePrinting();

            if (!HasRuntimeServices())
            {
                SetupDesignerPreview();
                ShowStep(1);
                return;
            }

            LoadLookups();
            LoadStudents();
            ShowStep(1);
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _stepBar = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 10, 18, 10) };
            _step1Lbl = MakeStepLabel("1. Student");
            _step2Lbl = MakeStepLabel("2. Subjects");
            _step3Lbl = MakeStepLabel("3. Confirm");

            var steps = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            steps.Controls.Add(_step1Lbl);
            steps.Controls.Add(_step2Lbl);
            steps.Controls.Add(_step3Lbl);
            _stepBar.Controls.Add(steps);

            _container = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(18) };

            Controls.Add(_container);
            Controls.Add(_stepBar);

            BuildStep1();
            BuildStep2();
            BuildStep3();

            _container.Controls.Add(_pStep3);
            _container.Controls.Add(_pStep2);
            _container.Controls.Add(_pStep1);
        }

        private void BuildStep1()
        {
            _pStep1 = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };

            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(16) };
            ThemeManager.StyleCardPanel(card);

            var searchRow = new Panel { Dock = DockStyle.Top, Height = 44 };
            var lblSearch = new Label { Text = "Search student:", AutoSize = true, Location = new Point(0, 12), Font = ThemeFonts.Label, ForeColor = ThemeColors.Text };
            _txtStudentSearch = new TextBox { Location = new Point(110, 8), Width = 320 };
            ThemeManager.StyleInput(_txtStudentSearch);
            _txtStudentSearch.TextChanged += (s, e) => LoadStudents();

            searchRow.Controls.Add(lblSearch);
            searchRow.Controls.Add(_txtStudentSearch);

            _lblSelectedStudent = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Selected: (none)",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Padding = new Padding(0, 8, 0, 0)
            };

            _gridStudents = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_gridStudents);
            _gridStudents.CellContentClick += StudentsGridCellContentClick;

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(0, 10, 0, 0) };
            _btnNext1 = new Button { Text = "Next", Width = 110, Dock = DockStyle.Right };
            ThemeManager.StyleButtonPrimary(_btnNext1);
            _btnNext1.Click += (s, e) =>
            {
                if (_selectedStudentId <= 0)
                {
                    ThemedMessageBox.ShowError(this, "Please select a student first.", "Enrollment");
                    return;
                }
                ShowStep(2);
            };
            footer.Controls.Add(_btnNext1);

            card.Controls.Add(_gridStudents);
            card.Controls.Add(footer);
            card.Controls.Add(_lblSelectedStudent);
            card.Controls.Add(searchRow);

            _pStep1.Controls.Add(card);
        }

        private void BuildStep2()
        {
            _pStep2 = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };

            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(16) };
            ThemeManager.StyleCardPanel(card);

            var selectors = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 78,
                ColumnCount = 8,
                RowCount = 2,
                Padding = new Padding(0, 0, 0, 10)
            };
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            selectors.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            selectors.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            _cmbCourse = MakeCombo();
            _cmbAcademicYear = MakeCombo();
            _cmbYearLevel = MakeCombo();
            _cmbSemester = MakeCombo();

            _cmbCourse.SelectedIndexChanged += (s, e) => LoadCurriculumSubjects();
            _cmbAcademicYear.SelectedIndexChanged += (s, e) => LoadCurriculumSubjects();
            _cmbYearLevel.SelectedIndexChanged += (s, e) => LoadCurriculumSubjects();
            _cmbSemester.SelectedIndexChanged += (s, e) => LoadCurriculumSubjects();

            selectors.Controls.Add(MakeLabel("Course"), 0, 0);
            selectors.Controls.Add(_cmbCourse, 1, 0);
            selectors.Controls.Add(MakeLabel("Acad. Yr"), 2, 0);
            selectors.Controls.Add(_cmbAcademicYear, 3, 0);
            selectors.Controls.Add(MakeLabel("Year"), 4, 0);
            selectors.Controls.Add(_cmbYearLevel, 5, 0);
            selectors.Controls.Add(MakeLabel("Sem"), 6, 0);
            selectors.Controls.Add(_cmbSemester, 7, 0);

            _lblCurriculumStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Select course/year/semester/academic year to load subjects.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText
            };

            _lvSubjects = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true
            };
            _lvSubjects.Columns.Add("Code", 120);
            _lvSubjects.Columns.Add("Subject", 520);
            _lvSubjects.Columns.Add("Units", 70);
            _lvSubjects.ItemChecked += (s, e) => UpdateUnits();

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(0, 10, 0, 0) };
            _btnBack2 = new Button { Text = "Back", Width = 110, Dock = DockStyle.Left };
            ThemeManager.StyleButtonNeutral(_btnBack2);
            _btnBack2.Click += (s, e) => ShowStep(1);

            _btnNext2 = new Button { Text = "Next", Width = 110, Dock = DockStyle.Right };
            ThemeManager.StyleButtonPrimary(_btnNext2);
            _btnNext2.Click += (s, e) =>
            {
                var details = BuildEnrollmentDetailsFromChecked();
                if (details.Count == 0)
                {
                    ThemedMessageBox.ShowError(this, "Please select at least one subject.", "Enrollment");
                    return;
                }
                BuildSummary(details);
                ShowStep(3);
            };

            _lblTotalUnits = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Text = "Total Units: 0"
            };

            footer.Controls.Add(_btnBack2);
            footer.Controls.Add(_btnNext2);
            footer.Controls.Add(_lblTotalUnits);

            card.Controls.Add(_lvSubjects);
            card.Controls.Add(footer);
            card.Controls.Add(_lblCurriculumStatus);
            card.Controls.Add(selectors);

            _pStep2.Controls.Add(card);
        }

        private void BuildStep3()
        {
            _pStep3 = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };

            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(16) };
            ThemeManager.StyleCardPanel(card);

            _lblSummary = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                Text = "Summary",
                AutoSize = false
            };

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(0, 10, 0, 0) };
            _btnBack3 = new Button { Text = "Back", Width = 110, Dock = DockStyle.Left };
            ThemeManager.StyleButtonNeutral(_btnBack3);
            _btnBack3.Click += (s, e) => ShowStep(2);

            _btnSave = new Button { Text = "Save", Width = 110, Dock = DockStyle.Right };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => SaveEnrollment();

            _btnPrint = new Button { Text = "Print", Width = 110, Dock = DockStyle.Right };
            ThemeManager.StyleButtonNeutral(_btnPrint);
            _btnPrint.Click += (s, e) => PrintSummary();

            footer.Controls.Add(_btnBack3);
            footer.Controls.Add(_btnSave);
            footer.Controls.Add(_btnPrint);

            card.Controls.Add(_lblSummary);
            card.Controls.Add(footer);
            _pStep3.Controls.Add(card);
        }

        private static Label MakeStepLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.MutedText,
                AutoSize = true,
                Margin = new Padding(0, 4, 22, 0)
            };
        }

        private static Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static ComboBox MakeCombo()
        {
            var cb = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(cb);
            return cb;
        }

        private void ShowStep(int step)
        {
            _pStep1.Visible = step == 1;
            _pStep2.Visible = step == 2;
            _pStep3.Visible = step == 3;

            _step1Lbl.ForeColor = step == 1 ? ThemeColors.Secondary : ThemeColors.MutedText;
            _step2Lbl.ForeColor = step == 2 ? ThemeColors.Secondary : ThemeColors.MutedText;
            _step3Lbl.ForeColor = step == 3 ? ThemeColors.Secondary : ThemeColors.MutedText;
        }

        private void LoadLookups()
        {
            if (_courseService == null || _lookupService == null) return;

            try
            {
                _cmbCourse.DisplayMember = "CourseName";
                _cmbCourse.ValueMember = "CourseId";
                _cmbCourse.DataSource = _courseService.GetLookupCourses();

                _cmbAcademicYear.DisplayMember = "Name";
                _cmbAcademicYear.ValueMember = "AcademicYearId";
                _cmbAcademicYear.DataSource = _lookupService.GetAcademicYears();

                _cmbYearLevel.DisplayMember = "Name";
                _cmbYearLevel.ValueMember = "YearLevelId";
                _cmbYearLevel.DataSource = _lookupService.GetYearLevels();

                _cmbSemester.DisplayMember = "Name";
                _cmbSemester.ValueMember = "SemesterId";
                _cmbSemester.DataSource = _lookupService.GetSemesters();

                if (_cmbAcademicYear.Items.Count > 0) _cmbAcademicYear.SelectedIndex = 0;
                if (_cmbYearLevel.Items.Count > 0) _cmbYearLevel.SelectedIndex = 0;
                if (_cmbSemester.Items.Count > 0) _cmbSemester.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("EnrollmentControl.LoadLookups", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void LoadStudents()
        {
            if (_studentService == null) return;

            try
            {
                var dt = _studentService.GetStudents(_txtStudentSearch.Text);
                _gridStudents.Columns.Clear();
                _gridStudents.DataSource = null;
                _gridStudents.DataSource = dt;

                if (_gridStudents.Columns["StudentId"] != null) _gridStudents.Columns["StudentId"].Visible = false;

                var selectCol = new DataGridViewButtonColumn
                {
                    Name = "SelectAction",
                    HeaderText = "",
                    Text = "Select",
                    UseColumnTextForButtonValue = true,
                    Width = 70
                };
                _gridStudents.Columns.Insert(0, selectCol);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("EnrollmentControl.LoadStudents", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void StudentsGridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_studentService == null) return;
            if (e.RowIndex < 0) return;
            var col = _gridStudents.Columns[e.ColumnIndex];
            if (col == null) return;

            if (!string.Equals(col.Name, "SelectAction", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var row = _gridStudents.Rows[e.RowIndex];
            _selectedStudentId = Convert.ToInt32(row.Cells["StudentId"].Value);
            _selectedStudentNumber = Convert.ToString(row.Cells["StudentNumber"].Value);
            var last = Convert.ToString(row.Cells["LastName"].Value);
            var first = Convert.ToString(row.Cells["FirstName"].Value);
            _selectedStudentName = (last + ", " + first).Trim(new[] { ' ', ',' });

            _lblSelectedStudent.Text = "Selected: " + _selectedStudentNumber + " - " + _selectedStudentName;
        }

        private void LoadCurriculumSubjects()
        {
            if (_curriculumService == null) return;

            try
            {
                var courseId = GetSelectedInt(_cmbCourse);
                var ayId = GetSelectedInt(_cmbAcademicYear);
                var ylId = GetSelectedInt(_cmbYearLevel);
                var semId = GetSelectedInt(_cmbSemester);

                _lvSubjects.Items.Clear();
                _curriculumId = null;

                if (courseId <= 0 || ayId <= 0 || ylId <= 0 || semId <= 0)
                {
                    _lblCurriculumStatus.Text = "Select course/year/semester/academic year to load subjects.";
                    UpdateUnits();
                    return;
                }

                _curriculumId = _curriculumService.TryGetCurriculumId(courseId, ylId, semId, ayId);
                if (!_curriculumId.HasValue)
                {
                    _lblCurriculumStatus.Text = "No curriculum found for this selection. Please set up Curriculum first.";
                    UpdateUnits();
                    return;
                }

                _lblCurriculumStatus.Text = "Curriculum loaded. Check the subjects to enroll.";

                var dt = _curriculumService.GetCurriculumSubjects(_curriculumId.Value);
                foreach (DataRow r in dt.Rows)
                {
                    var subjectId = Convert.ToInt32(r["SubjectId"]);
                    var code = Convert.ToString(r["SubjectCode"]);
                    var name = Convert.ToString(r["SubjectName"]);
                    var units = Convert.ToInt32(r["Units"]);

                    var item = new ListViewItem(code ?? string.Empty);
                    item.SubItems.Add(name ?? string.Empty);
                    item.SubItems.Add(units.ToString());
                    item.Tag = subjectId;
                    item.Checked = true; // default: enroll all curriculum subjects; user can uncheck.
                    _lvSubjects.Items.Add(item);
                }

                UpdateUnits();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("EnrollmentControl.LoadCurriculumSubjects", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void UpdateUnits()
        {
            var total = 0;
            foreach (ListViewItem item in _lvSubjects.Items)
            {
                if (!item.Checked) continue;
                if (item.SubItems.Count < 3) continue;
                int units;
                if (int.TryParse(item.SubItems[2].Text, out units))
                {
                    total += units;
                }
            }

            _lblTotalUnits.Text = "Total Units: " + total.ToString();
        }

        private List<EnrollmentDetail> BuildEnrollmentDetailsFromChecked()
        {
            var details = new List<EnrollmentDetail>();
            foreach (ListViewItem item in _lvSubjects.Items)
            {
                if (!item.Checked) continue;
                if (item.Tag == null) continue;
                if (item.SubItems.Count < 3) continue;

                int units;
                if (!int.TryParse(item.SubItems[2].Text, out units))
                {
                    units = 0;
                }

                details.Add(new EnrollmentDetail
                {
                    SubjectId = Convert.ToInt32(item.Tag),
                    Units = units
                });
            }

            return details;
        }

        private void BuildSummary(List<EnrollmentDetail> details)
        {
            _enrollmentNumber = _enrollmentService.GetNextEnrollmentNumber();

            var sb = new StringBuilder();
            sb.AppendLine("Enrollment Number: " + _enrollmentNumber);
            sb.AppendLine("Student: " + _selectedStudentNumber + " - " + _selectedStudentName);
            sb.AppendLine("Course: " + (_cmbCourse.Text ?? string.Empty));
            sb.AppendLine("Academic Year: " + (_cmbAcademicYear.Text ?? string.Empty));
            sb.AppendLine("Year Level: " + (_cmbYearLevel.Text ?? string.Empty));
            sb.AppendLine("Semester: " + (_cmbSemester.Text ?? string.Empty));
            sb.AppendLine();
            sb.AppendLine("Subjects:");

            foreach (ListViewItem item in _lvSubjects.Items)
            {
                if (!item.Checked) continue;
                sb.AppendLine("- " + item.Text + " | " + item.SubItems[1].Text + " (" + item.SubItems[2].Text + " units)");
            }

            sb.AppendLine();
            sb.AppendLine(_lblTotalUnits.Text);

            _lblSummary.Text = sb.ToString();
        }

        private void SaveEnrollment()
        {
            if (!HasRuntimeServices()) return;

            try
            {
                var courseId = GetSelectedInt(_cmbCourse);
                var ayId = GetSelectedInt(_cmbAcademicYear);
                var ylId = GetSelectedInt(_cmbYearLevel);
                var semId = GetSelectedInt(_cmbSemester);

                var details = BuildEnrollmentDetailsFromChecked();

                var enrollment = new Enrollment
                {
                    EnrollmentNumber = _enrollmentNumber ?? _enrollmentService.GetNextEnrollmentNumber(),
                    StudentId = _selectedStudentId,
                    CourseId = courseId,
                    AcademicYearId = ayId,
                    YearLevelId = ylId,
                    SemesterId = semId,
                    EnrollDate = DateTime.Now.Date,
                    Status = "Posted"
                };

                var vr = _enrollmentService.Validate(enrollment, details);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                if (ThemedMessageBox.ShowConfirm(this, "Save this enrollment?", "Confirm") != DialogResult.OK)
                {
                    return;
                }

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                _enrollmentService.Save(enrollment, details);
                ThemedMessageBox.ShowInfo(this, "Enrollment saved successfully.\n\n" + enrollment.EnrollmentNumber, "Saved");

                ResetAll();
                ShowStep(1);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("EnrollmentControl.SaveEnrollment", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnSave.Enabled = true;
            }
        }

        private static int GetSelectedInt(ComboBox combo)
        {
            if (combo == null || combo.SelectedValue == null)
            {
                return 0;
            }

            if (combo.SelectedValue is int)
            {
                return (int)combo.SelectedValue;
            }

            int parsed;
            if (int.TryParse(Convert.ToString(combo.SelectedValue), out parsed))
            {
                return parsed;
            }

            return 0;
        }

        private void ResetAll()
        {
            if (!HasRuntimeServices()) return;

            _selectedStudentId = 0;
            _selectedStudentNumber = null;
            _selectedStudentName = null;
            _enrollmentNumber = null;
            _curriculumId = null;

            _lblSelectedStudent.Text = "Selected: (none)";
            _txtStudentSearch.Text = string.Empty;
            LoadStudents();

            _lvSubjects.Items.Clear();
            UpdateUnits();
            _lblCurriculumStatus.Text = "Select course/year/semester/academic year to load subjects.";
        }

        private void WirePrinting()
        {
            _printDocument.PrintPage += (s, e) =>
            {
                var text = _printText ?? string.Empty;
                using (var font = new Font("Consolas", 10F))
                {
                    e.Graphics.DrawString(text, font, Brushes.Black, new RectangleF(40, 40, e.MarginBounds.Width, e.MarginBounds.Height));
                }
            };
        }

        private void PrintSummary()
        {
            try
            {
                _printText = _lblSummary.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(_printText))
                {
                    ThemedMessageBox.ShowError(this, "Nothing to print yet. Go to Step 3 first.", "Print");
                    return;
                }

                using (var preview = new PrintPreviewDialog())
                {
                    preview.Document = _printDocument;
                    preview.Width = 900;
                    preview.Height = 700;
                    preview.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("EnrollmentControl.PrintSummary", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private bool HasRuntimeServices()
        {
            return _enrollmentService != null &&
                   _studentService != null &&
                   _curriculumService != null &&
                   _courseService != null &&
                   _lookupService != null;
        }

        private void SetupDesignerPreview()
        {
            if (_lblSelectedStudent != null)
            {
                _lblSelectedStudent.Text = "Selected: STU-00001 - Designer, Sample";
            }

            if (_lblCurriculumStatus != null)
            {
                _lblCurriculumStatus.Text = "Designer preview mode";
            }

            if (_lvSubjects != null && _lvSubjects.Items.Count == 0)
            {
                var item = new ListViewItem("SUBJ-001");
                item.SubItems.Add("Sample Subject");
                item.SubItems.Add("3");
                item.Checked = true;
                _lvSubjects.Items.Add(item);
                UpdateUnits();
            }

            if (_lblSummary != null)
            {
                _lblSummary.Text = "Enrollment Summary Preview";
            }
        }
    }
}
