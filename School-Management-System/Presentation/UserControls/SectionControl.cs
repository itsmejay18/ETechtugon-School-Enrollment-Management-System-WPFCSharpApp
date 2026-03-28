using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class SectionControl : BaseUserControl
    {
        private readonly SectionService _sectionService;
        private readonly CourseService _courseService;
        private readonly LookupService _lookupService;
        private readonly SystemSettingService _systemSettingService;

        private GroupBox _gb;
        private TextBox _txtName;
        private ComboBox _cmbCourse;
        private ComboBox _cmbYearLevel;
        private ComboBox _cmbAcademicYear;
        private ComboBox _cmbSemester;
        private NumericUpDown _numCapacity;

        private TextBox _txtSearch;
        private DataGridView _grid;
        private SplitContainer _split;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnRefresh;

        private int _editingSectionId;
        private bool _isEditorActive;
        private int? _activeAcademicYearId;
        private int? _activeSemesterId;

        public SectionControl()
            : this(null, null, null, null)
        {
        }

        public SectionControl(SectionService sectionService, CourseService courseService, LookupService lookupService, SystemSettingService systemSettingService)
        {
            _sectionService = sectionService;
            _courseService = courseService;
            _lookupService = lookupService;
            _systemSettingService = systemSettingService;

            var activeTerm = _systemSettingService == null ? (null, null) : _systemSettingService.GetActiveTerm();
            _activeAcademicYearId = activeTerm.Item1;
            _activeSemesterId = activeTerm.Item2;

            InitializeComponent();
            LoadLookups();
            SetEditorState(false);
            LoadGrid();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            var toolbar = BuildToolbar();
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                SplitterDistance = 700,
                BackColor = ThemeColors.Border
            };
            LockSplitEditorPanel(_split, 440);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.CellClick += (s, e) => PreviewSelected();

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            left.Controls.Add(_grid);
            _split.Panel1.Controls.Add(left);

            _gb = new GroupBox
            {
                Text = "Section Details",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(_gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // name
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // course
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // year level
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // academic year
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // semester
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // capacity
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            _txtName = MakeTextBox();
            _cmbCourse = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbCourse);
            _cmbYearLevel = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbYearLevel);
            _cmbAcademicYear = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbAcademicYear);
            _cmbSemester = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbSemester);
            _numCapacity = new NumericUpDown { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Minimum = 0, Maximum = 200, Value = 40 };
            ThemeManager.StyleInput(_numCapacity);

            layout.Controls.Add(MakeLabel("Section Name *"), 0, 0);
            layout.Controls.Add(_txtName, 1, 0);
            layout.Controls.Add(MakeLabel("Course *"), 0, 1);
            layout.Controls.Add(_cmbCourse, 1, 1);
            layout.Controls.Add(MakeLabel("Year Level *"), 0, 2);
            layout.Controls.Add(_cmbYearLevel, 1, 2);
            layout.Controls.Add(MakeLabel("Academic Year *"), 0, 3);
            layout.Controls.Add(_cmbAcademicYear, 1, 3);
            layout.Controls.Add(MakeLabel("Semester *"), 0, 4);
            layout.Controls.Add(_cmbSemester, 1, 4);
            layout.Controls.Add(MakeLabel("Capacity"), 0, 5);
            layout.Controls.Add(_numCapacity, 1, 5);

            var note = new Label
            {
                Text = "Sections are tied to course/year/semester/AY. Set global term in Settings.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 7);
            layout.SetColumnSpan(note, 2);

            _gb.Controls.Add(layout);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            right.Controls.Add(_gb);
            _split.Panel2.Controls.Add(right);

            Controls.Add(_split);
            Controls.Add(toolbar);
        }

        private Panel BuildToolbar()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 0, 12) };
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

            var lblSearch = new Label
            {
                Text = "Search:",
                AutoSize = false,
                Width = 58,
                Height = 32,
                ForeColor = ThemeColors.MutedText,
                Font = ThemeFonts.Label,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 4, 0)
            };

            _txtSearch = new TextBox { Width = 260, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 96, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            _btnAdd = new Button { Text = "Add", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnDelete = new Button { Text = "Delete", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            _btnSave = new Button { Text = "Save", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 86 };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => CancelEdit();

            strip.Controls.Add(lblSearch);
            strip.Controls.Add(_txtSearch);
            strip.Controls.Add(_btnRefresh);
            strip.Controls.Add(_btnAdd);
            strip.Controls.Add(_btnEdit);
            strip.Controls.Add(_btnDelete);
            strip.Controls.Add(_btnSave);
            strip.Controls.Add(_btnCancel);
            card.Controls.Add(strip);
            toolbar.Controls.Add(card);
            return toolbar;
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

        private static TextBox MakeTextBox()
        {
            var tb = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill };
            ThemeManager.StyleInput(tb);
            return tb;
        }

        private void LoadLookups()
        {
            if (_courseService == null || _lookupService == null) return;

            try
            {
                _cmbCourse.DisplayMember = "CourseName";
                _cmbCourse.ValueMember = "CourseId";
                _cmbCourse.DataSource = _courseService.GetLookupCourses();

                _cmbYearLevel.DisplayMember = "Name";
                _cmbYearLevel.ValueMember = "YearLevelId";
                _cmbYearLevel.DataSource = _lookupService.GetYearLevels();

                _cmbAcademicYear.DisplayMember = "Name";
                _cmbAcademicYear.ValueMember = "AcademicYearId";
                _cmbAcademicYear.DataSource = _lookupService.GetAcademicYears();

                _cmbSemester.DisplayMember = "Name";
                _cmbSemester.ValueMember = "SemesterId";
                _cmbSemester.DataSource = _lookupService.GetSemesters();

                ApplyActiveTermDefaults();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SectionControl.LoadLookups", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void ApplyActiveTermDefaults()
        {
            if (_activeAcademicYearId.HasValue)
            {
                _cmbAcademicYear.SelectedValue = _activeAcademicYearId.Value;
            }

            if (_activeSemesterId.HasValue)
            {
                _cmbSemester.SelectedValue = _activeSemesterId.Value;
            }
        }

        private void SetEditorState(bool active)
        {
            _isEditorActive = active;
            _txtName.ReadOnly = !active;
            _cmbCourse.Enabled = active;
            _cmbYearLevel.Enabled = active;
            _cmbAcademicYear.Enabled = active;
            _cmbSemester.Enabled = active;
            _numCapacity.Enabled = active;

            _btnSave.Enabled = active;
            _btnCancel.Enabled = active;
            _btnAdd.Enabled = !active;
            _btnEdit.Enabled = !active && _editingSectionId > 0;
            _btnDelete.Enabled = !active && _editingSectionId > 0;
        }

        private void BeginAdd()
        {
            _editingSectionId = 0;
            _txtName.Text = string.Empty;
            _numCapacity.Value = 40;
            ApplyActiveTermDefaults();
            SetEditorState(true);
            _txtName.Focus();
        }

        private void BeginEdit()
        {
            if (_editingSectionId <= 0) return;
            SetEditorState(true);
            _txtName.Focus();
        }

        private void CancelEdit()
        {
            SetEditorState(false);
            PreviewSelected();
        }

        private void LoadGrid()
        {
            try
            {
                UseWaitCursor = true;
                var dt = _sectionService == null ? new DataTable() : _sectionService.GetSections(_txtSearch == null ? string.Empty : _txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SectionControl.LoadGrid", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void BindGrid(DataTable dt)
        {
            _grid.DataSource = null;
            _grid.Columns.Clear();

            _grid.DataSource = dt;
            if (_grid.Columns["SectionId"] != null) _grid.Columns["SectionId"].Visible = false;
            if (_grid.Columns["CourseId"] != null) _grid.Columns["CourseId"].Visible = false;
            if (_grid.Columns["YearLevelId"] != null) _grid.Columns["YearLevelId"].Visible = false;
            if (_grid.Columns["AcademicYearId"] != null) _grid.Columns["AcademicYearId"].Visible = false;
            if (_grid.Columns["SemesterId"] != null) _grid.Columns["SemesterId"].Visible = false;

            if (_grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                PreviewSelected();
            }
            else
            {
                _editingSectionId = 0;
                _txtName.Text = string.Empty;
                _numCapacity.Value = 40;
                ApplyActiveTermDefaults();
                SetEditorState(false);
            }
        }

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (_grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;
            if (row.Cells["SectionId"] == null || row.Cells["SectionId"].Value == null) return;

            _editingSectionId = Convert.ToInt32(row.Cells["SectionId"].Value);
            _txtName.Text = Convert.ToString(row.Cells["SectionName"].Value);

            if (row.Cells["CourseId"] != null) _cmbCourse.SelectedValue = Convert.ToInt32(row.Cells["CourseId"].Value);
            if (row.Cells["YearLevelId"] != null) _cmbYearLevel.SelectedValue = Convert.ToInt32(row.Cells["YearLevelId"].Value);
            if (row.Cells["AcademicYearId"] != null) _cmbAcademicYear.SelectedValue = Convert.ToInt32(row.Cells["AcademicYearId"].Value);
            if (row.Cells["SemesterId"] != null) _cmbSemester.SelectedValue = Convert.ToInt32(row.Cells["SemesterId"].Value);

            decimal cap;
            if (decimal.TryParse(Convert.ToString(row.Cells["Capacity"].Value), out cap))
            {
                _numCapacity.Value = cap;
            }

            SetEditorState(false);
        }

        private void Save()
        {
            if (_sectionService == null) return;

            try
            {
                var section = new Section
                {
                    SectionId = _editingSectionId,
                    SectionName = (_txtName.Text ?? string.Empty).Trim(),
                    CourseId = _cmbCourse.SelectedValue == null ? 0 : Convert.ToInt32(_cmbCourse.SelectedValue),
                    YearLevelId = _cmbYearLevel.SelectedValue == null ? 0 : Convert.ToInt32(_cmbYearLevel.SelectedValue),
                    AcademicYearId = _cmbAcademicYear.SelectedValue == null ? 0 : Convert.ToInt32(_cmbAcademicYear.SelectedValue),
                    SemesterId = _cmbSemester.SelectedValue == null ? 0 : Convert.ToInt32(_cmbSemester.SelectedValue),
                    Capacity = Convert.ToInt32(_numCapacity.Value)
                };

                var vr = _sectionService.Validate(section);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingSectionId == 0)
                {
                    _sectionService.Create(section);
                    ThemedMessageBox.ShowInfo(this, "Section saved successfully.", "Saved");
                }
                else
                {
                    _sectionService.Update(section);
                    ThemedMessageBox.ShowInfo(this, "Section updated successfully.", "Updated");
                }

                LoadGrid();
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SectionControl.Save", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnSave.Enabled = _isEditorActive;
            }
        }

        private void DeleteCurrent()
        {
            if (_sectionService == null) return;
            if (_editingSectionId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Section") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _sectionService.Delete(_editingSectionId);
                LoadGrid();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SectionControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
