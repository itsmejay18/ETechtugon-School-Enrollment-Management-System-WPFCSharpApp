using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class CurriculumControl : BaseUserControl
    {
        private readonly CurriculumService _curriculumService;
        private readonly CourseService _courseService;
        private readonly LookupService _lookupService;

        private GroupBox _gb;
        private ComboBox _cmbCourse;
        private ComboBox _cmbAcademicYear;
        private ComboBox _cmbYearLevel;
        private ComboBox _cmbSemester;
        private Label _lblCurriculumName;
        private Label _lblTotalUnits;
        private Button _btnCheckAll;
        private Button _btnUncheckAll;

        private ListView _lvSubjects;

        private int _curriculumId;
        private bool _loading;

        public CurriculumControl()
            : this(null, null, null)
        {
        }

        public CurriculumControl(CurriculumService curriculumService, CourseService courseService, LookupService lookupService)
        {
            _curriculumService = curriculumService;
            _courseService = courseService;
            _lookupService = lookupService;

            InitializeComponent();
            if (_curriculumService == null || _courseService == null || _lookupService == null)
            {
                _lblCurriculumName.Text = "Curriculum: Designer Preview";
                _lvSubjects.Items.Add(new ListViewItem(new[] { "SUBJ-001", "Sample Subject", "3" }));
                UpdateTotalUnits();
                return;
            }

            LoadLookups();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _gb = new GroupBox
            {
                Text = "Curriculum Setup",
                Dock = DockStyle.Top,
                Height = 170,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 3,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            _cmbCourse = MakeCombo();
            _cmbAcademicYear = MakeCombo();
            _cmbYearLevel = MakeCombo();
            _cmbSemester = MakeCombo();

            _cmbCourse.SelectedIndexChanged += (s, e) => Reload();
            _cmbAcademicYear.SelectedIndexChanged += (s, e) => Reload();
            _cmbYearLevel.SelectedIndexChanged += (s, e) => Reload();
            _cmbSemester.SelectedIndexChanged += (s, e) => Reload();

            layout.Controls.Add(MakeLabel("Course *"), 0, 0);
            layout.Controls.Add(_cmbCourse, 1, 0);
            layout.Controls.Add(MakeLabel("Acad. Year *"), 2, 0);
            layout.Controls.Add(_cmbAcademicYear, 3, 0);
            layout.Controls.Add(MakeLabel("Year Level *"), 4, 0);
            layout.Controls.Add(_cmbYearLevel, 5, 0);

            layout.Controls.Add(MakeLabel("Semester *"), 0, 1);
            layout.Controls.Add(_cmbSemester, 1, 1);

            _lblCurriculumName = new Label
            {
                Text = "Curriculum: -",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblTotalUnits = new Label
            {
                Text = "Total Units: 0",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(_lblCurriculumName, 0, 2);
            layout.SetColumnSpan(_lblCurriculumName, 4);
            layout.Controls.Add(_lblTotalUnits, 4, 2);
            layout.SetColumnSpan(_lblTotalUnits, 2);

            _gb.Controls.Add(layout);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(0, 0, 0, 12), BackColor = ThemeColors.Background };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(12, 8, 12, 8) };
            ThemeManager.StyleCardPanel(card);
            var strip = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            _btnCheckAll = new Button { Text = "Check All", Width = 110, Margin = new Padding(0, 0, 10, 0) };
            ThemeManager.StyleButtonNeutral(_btnCheckAll);
            _btnCheckAll.Click += (s, e) => SetAllChecks(true);

            _btnUncheckAll = new Button { Text = "Uncheck All", Width = 110 };
            ThemeManager.StyleButtonNeutral(_btnUncheckAll);
            _btnUncheckAll.Click += (s, e) => SetAllChecks(false);

            strip.Controls.Add(_btnCheckAll);
            strip.Controls.Add(_btnUncheckAll);
            card.Controls.Add(strip);
            toolbar.Controls.Add(card);

            _lvSubjects = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true
            };
            _lvSubjects.Columns.Add("Code", 120);
            _lvSubjects.Columns.Add("Subject", 480);
            _lvSubjects.Columns.Add("Units", 70);
            _lvSubjects.ItemChecked += SubjectsItemChecked;

            Controls.Add(_lvSubjects);
            Controls.Add(toolbar);
            Controls.Add(_gb);
        }

        private static ComboBox MakeCombo()
        {
            var cb = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(cb);
            return cb;
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

        private void LoadLookups()
        {
            if (_courseService == null || _lookupService == null) return;

            try
            {
                _loading = true;

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

                // Best-effort default selections.
                if (_cmbAcademicYear.Items.Count > 0) _cmbAcademicYear.SelectedIndex = 0;
                if (_cmbYearLevel.Items.Count > 0) _cmbYearLevel.SelectedIndex = 0;
                if (_cmbSemester.Items.Count > 0) _cmbSemester.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("CurriculumControl.LoadLookups", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                _loading = false;
            }

            Reload();
        }

        private void Reload()
        {
            if (_loading) return;
            if (_curriculumService == null || _courseService == null || _lookupService == null) return;

            var beganUpdate = false;
            try
            {
                var courseId = GetSelectedInt(_cmbCourse);
                var ayId = GetSelectedInt(_cmbAcademicYear);
                var ylId = GetSelectedInt(_cmbYearLevel);
                var semId = GetSelectedInt(_cmbSemester);

                if (courseId <= 0 || ayId <= 0 || ylId <= 0 || semId <= 0)
                {
                    _curriculumId = 0;
                    _lblCurriculumName.Text = "Curriculum: -";
                    _lvSubjects.Items.Clear();
                    _lblTotalUnits.Text = "Total Units: 0";
                    return;
                }

                var name = BuildCurriculumName();
                _lblCurriculumName.Text = "Curriculum: " + name;

                _curriculumId = _curriculumService.EnsureCurriculum(name, courseId, ylId, semId, ayId);

                var allSubjects = _curriculumService.GetSubjectsForCourse(courseId) ?? new DataTable();
                var selected = _curriculumService.GetCurriculumSubjects(_curriculumId) ?? new DataTable();
                var selectedIds = new HashSet<int>();
                foreach (DataRow r in selected.Rows)
                {
                    if (r == null || r["SubjectId"] == DBNull.Value) continue;
                    selectedIds.Add(Convert.ToInt32(r["SubjectId"]));
                }

                _loading = true;
                _lvSubjects.BeginUpdate();
                beganUpdate = true;
                _lvSubjects.Items.Clear();

                foreach (DataRow r in allSubjects.Rows)
                {
                    if (r == null) continue;
                    if (!allSubjects.Columns.Contains("SubjectId")) continue;
                    if (!allSubjects.Columns.Contains("Units")) continue;

                    var subjectId = Convert.ToInt32(r["SubjectId"]);
                    var code = allSubjects.Columns.Contains("SubjectCode") ? Convert.ToString(r["SubjectCode"]) : string.Empty;
                    var subject = allSubjects.Columns.Contains("SubjectName") ? Convert.ToString(r["SubjectName"]) : string.Empty;
                    var units = Convert.ToInt32(r["Units"]);

                    var item = new ListViewItem(code ?? string.Empty);
                    item.SubItems.Add(subject ?? string.Empty);
                    item.SubItems.Add(units.ToString());
                    item.Tag = subjectId;
                    item.Checked = selectedIds.Contains(subjectId);
                    _lvSubjects.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("CurriculumControl.Reload", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                if (beganUpdate)
                {
                    _lvSubjects.EndUpdate();
                }
                _loading = false;
                UpdateTotalUnits();
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

        private string BuildCurriculumName()
        {
            var courseText = GetComboText(_cmbCourse);
            var ayText = GetComboText(_cmbAcademicYear);
            var ylText = GetComboText(_cmbYearLevel);
            var semText = GetComboText(_cmbSemester);

            return (courseText + " - " + ylText + " - " + semText + " - " + ayText).Trim();
        }

        private static string GetComboText(ComboBox combo)
        {
            if (combo == null) return string.Empty;
            var drv = combo.SelectedItem as DataRowView;
            if (drv != null)
            {
                // Prefer "CourseCode - CourseName" when available.
                if (drv.DataView.Table.Columns.Contains("CourseCode") && drv.DataView.Table.Columns.Contains("CourseName"))
                {
                    var code = Convert.ToString(drv["CourseCode"]);
                    var name = Convert.ToString(drv["CourseName"]);
                    if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
                    {
                        return code.Trim() + " - " + name.Trim();
                    }
                }
            }

            return Convert.ToString(combo.Text) ?? string.Empty;
        }

        private void SubjectsItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_curriculumService == null) return;
            if (_loading) return;
            if (_curriculumId <= 0) return;
            if (e.Item == null) return;
            if (e.Item.Tag == null) return;

            try
            {
                var subjectId = Convert.ToInt32(e.Item.Tag);
                _curriculumService.SetSubjectIncluded(_curriculumId, subjectId, e.Item.Checked);
                UpdateTotalUnits();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("CurriculumControl.SubjectsItemChecked", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void SetAllChecks(bool check)
        {
            if (_curriculumService == null) return;
            if (_curriculumId <= 0) return;

            _loading = true;
            _lvSubjects.BeginUpdate();
            try
            {
                foreach (ListViewItem item in _lvSubjects.Items)
                {
                    if (item == null) continue;
                    if (item.Checked == check) continue;
                    item.Checked = check;
                }
            }
            finally
            {
                _lvSubjects.EndUpdate();
                _loading = false;
            }

            // Persist changes.
            foreach (ListViewItem item in _lvSubjects.Items)
            {
                if (item == null) continue;
                if (item.Tag == null) continue;
                var subjectId = Convert.ToInt32(item.Tag);
                try
                {
                    _curriculumService.SetSubjectIncluded(_curriculumId, subjectId, item.Checked);
                }
                catch (Exception ex)
                {
                    School_Management_System.DataLayer.Logging.FileLogger.LogError("CurriculumControl.SetAllChecks", ex);
                }
            }

            UpdateTotalUnits();
        }

        private void UpdateTotalUnits()
        {
            if (_lvSubjects == null || _lvSubjects.IsDisposed || _lblTotalUnits == null || _lblTotalUnits.IsDisposed)
            {
                return;
            }

            var total = 0;
            for (var i = 0; i < _lvSubjects.Items.Count; i++)
            {
                var item = _lvSubjects.Items[i];
                if (item == null) continue;
                if (!item.Checked) continue;

                var subItems = item.SubItems;
                if (subItems == null || subItems.Count < 3) continue;

                var unitsCell = subItems[2];
                if (unitsCell == null) continue;

                int units;
                if (int.TryParse(unitsCell.Text, out units))
                {
                    total += units;
                }
            }

            _lblTotalUnits.Text = "Total Units: " + total.ToString();
        }
    }
}
