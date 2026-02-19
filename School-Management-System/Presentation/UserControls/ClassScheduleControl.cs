using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class ClassScheduleControl : BaseUserControl
    {
        private readonly ClassScheduleService _classScheduleService;
        private readonly SectionService _sectionService;
        private readonly CurriculumService _curriculumService;
        private readonly SystemSettingService _systemSettingService;

        private ComboBox _cmbSection;
        private Button _btnReloadSections;
        private ListView _lvSchedules;

        private ComboBox _cmbSubject;
        private TextBox _txtDay;
        private DateTimePicker _dtStart;
        private DateTimePicker _dtEnd;
        private TextBox _txtRoom;
        private TextBox _txtRemarks;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;

        private int _selectedSectionId;
        private int _editingScheduleId;
        private bool _isEditorActive;
        private DataTable _sectionsTable;
        private DataTable _subjectsTable;
        private int? _activeAcademicYearId;
        private int? _activeSemesterId;

        public ClassScheduleControl()
            : this(null, null, null, null)
        {
        }

        public ClassScheduleControl(
            ClassScheduleService classScheduleService,
            SectionService sectionService,
            CurriculumService curriculumService,
            SystemSettingService systemSettingService)
        {
            _classScheduleService = classScheduleService;
            _sectionService = sectionService;
            _curriculumService = curriculumService;
            _systemSettingService = systemSettingService;

            var activeTerm = _systemSettingService == null ? (null, null) : _systemSettingService.GetActiveTerm();
            _activeAcademicYearId = activeTerm.Item1;
            _activeSemesterId = activeTerm.Item2;

            InitializeComponent();
            LoadSections();
            SetEditorState(false);
            LoadSchedules();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            var header = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 8), BackColor = ThemeColors.CardBackground };
            var lblSection = new Label { Text = "Section:", AutoSize = true, Location = new Point(0, 14), Font = ThemeFonts.Label, ForeColor = ThemeColors.Text };
            _cmbSection = new ComboBox { Width = 340, Location = new Point(68, 10), DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbSection);
            _cmbSection.SelectedIndexChanged += (s, e) => OnSectionChanged();

            _btnReloadSections = new Button { Text = "Reload", Width = 86, Location = new Point(418, 8) };
            ThemeManager.StyleButtonNeutral(_btnReloadSections);
            _btnReloadSections.Click += (s, e) => LoadSections();

            header.Controls.Add(lblSection);
            header.Controls.Add(_cmbSection);
            header.Controls.Add(_btnReloadSections);

            _lvSchedules = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _lvSchedules.Columns.Add("Subject", 200);
            _lvSchedules.Columns.Add("Day(s)", 110);
            _lvSchedules.Columns.Add("Time", 120);
            _lvSchedules.Columns.Add("Room", 100);
            _lvSchedules.Columns.Add("Remarks", 180);
            _lvSchedules.ItemSelectionChanged += (s, e) => PreviewSelected();

            var editor = BuildEditorPanel();

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 10), BackColor = ThemeColors.Background };
            body.Controls.Add(editor);
            body.Controls.Add(_lvSchedules);
            body.Controls.Add(header);

            Controls.Add(body);
        }

        private Panel BuildEditorPanel()
        {
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 150, Padding = new Padding(0, 10, 0, 0), BackColor = ThemeColors.Background };
            var frame = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            ThemeManager.StyleCardPanel(frame);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            _cmbSubject = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbSubject);
            _txtDay = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleInput(_txtDay);
            _dtStart = new DateTimePicker { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            _dtEnd = new DateTimePicker { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            _txtRoom = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleInput(_txtRoom);
            _txtRemarks = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleInput(_txtRemarks);

            layout.Controls.Add(MakeLabel("Subject"), 0, 0);
            layout.Controls.Add(_cmbSubject, 1, 0);
            layout.Controls.Add(MakeLabel("Day(s)"), 2, 0);
            layout.Controls.Add(_txtDay, 3, 0);

            layout.Controls.Add(MakeLabel("Start Time"), 0, 1);
            layout.Controls.Add(_dtStart, 1, 1);
            layout.Controls.Add(MakeLabel("End Time"), 2, 1);
            layout.Controls.Add(_dtEnd, 3, 1);

            layout.Controls.Add(MakeLabel("Room"), 0, 2);
            layout.Controls.Add(_txtRoom, 1, 2);
            layout.Controls.Add(MakeLabel("Remarks"), 2, 2);
            layout.Controls.Add(_txtRemarks, 3, 2);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 320, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 4, 0, 0) };
            _btnAdd = new Button { Text = "Add", Width = 72, Margin = new Padding(6, 0, 0, 0) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 72, Margin = new Padding(6, 0, 0, 0) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnDelete = new Button { Text = "Delete", Width = 72, Margin = new Padding(6, 0, 0, 0) };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            _btnSave = new Button { Text = "Save", Width = 72, Margin = new Padding(6, 0, 0, 0) };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 72, Margin = new Padding(6, 0, 0, 0) };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => CancelEdit();

            btnPanel.Controls.Add(_btnAdd);
            btnPanel.Controls.Add(_btnEdit);
            btnPanel.Controls.Add(_btnDelete);
            btnPanel.Controls.Add(_btnSave);
            btnPanel.Controls.Add(_btnCancel);

            frame.Controls.Add(btnPanel);
            frame.Controls.Add(layout);
            panel.Controls.Add(frame);
            return panel;
        }

        private static Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void LoadSections()
        {
            if (_sectionService == null)
            {
                return;
            }

            try
            {
                var dt = _sectionService.GetSections(string.Empty);
                _sectionsTable = dt;

                // filter by active term when available
                if (_activeAcademicYearId.HasValue || _activeSemesterId.HasValue)
                {
                    var view = dt.DefaultView;
                    var filters = new System.Collections.Generic.List<string>();
                    if (_activeAcademicYearId.HasValue) filters.Add("AcademicYearId = " + _activeAcademicYearId.Value);
                    if (_activeSemesterId.HasValue) filters.Add("SemesterId = " + _activeSemesterId.Value);
                    if (filters.Count > 0) view.RowFilter = string.Join(" AND ", filters);
                    _cmbSection.DataSource = view;
                }
                else
                {
                    _cmbSection.DataSource = dt;
                }

                _cmbSection.DisplayMember = "SectionName";
                _cmbSection.ValueMember = "SectionId";

                if (_cmbSection.Items.Count > 0)
                {
                    _cmbSection.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("ClassScheduleControl.LoadSections", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void OnSectionChanged()
        {
            if (_cmbSection.SelectedValue == null) return;
            int sectionId;
            if (!int.TryParse(Convert.ToString(_cmbSection.SelectedValue), out sectionId))
            {
                return;
            }

            _selectedSectionId = sectionId;
            LoadSubjectsForSection(sectionId);
            LoadSchedules();
        }

        private void LoadSubjectsForSection(int sectionId)
        {
            if (_sectionsTable == null || _curriculumService == null) return;

            var row = _sectionsTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["SectionId"]) == sectionId);
            if (row == null)
            {
                return;
            }

            var courseId = Convert.ToInt32(row["CourseId"]);
            var yearLevelId = Convert.ToInt32(row["YearLevelId"]);
            var ayId = Convert.ToInt32(row["AcademicYearId"]);
            var semId = Convert.ToInt32(row["SemesterId"]);

            DataTable subjects;
            var curriculumId = _curriculumService.TryGetCurriculumId(courseId, yearLevelId, semId, ayId);
            if (curriculumId.HasValue)
            {
                subjects = _curriculumService.GetCurriculumSubjects(curriculumId.Value);
            }
            else
            {
                subjects = _curriculumService.GetSubjectsForCourse(courseId);
            }

            _subjectsTable = subjects;
            _cmbSubject.DisplayMember = "SubjectName";
            _cmbSubject.ValueMember = "SubjectId";
            _cmbSubject.DataSource = subjects;
        }

        private void LoadSchedules()
        {
            if (_classScheduleService == null || _selectedSectionId <= 0)
            {
                _lvSchedules.Items.Clear();
                return;
            }

            try
            {
                var dt = _classScheduleService.GetBySection(_selectedSectionId);
                _lvSchedules.BeginUpdate();
                _lvSchedules.Items.Clear();

                foreach (DataRow r in dt.Rows)
                {
                    var subject = Convert.ToString(r["SubjectCode"]) + " - " + Convert.ToString(r["SubjectName"]);
                    var day = Convert.ToString(r["DayOfWeek"]);
                    var start = r["StartTime"] is TimeSpan ? ((TimeSpan)r["StartTime"]).ToString(@"hh\\:mm") : string.Empty;
                    var end = r["EndTime"] is TimeSpan ? ((TimeSpan)r["EndTime"]).ToString(@"hh\\:mm") : string.Empty;
                    var time = string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end) ? string.Empty : start + "-" + end;
                    var room = Convert.ToString(r["Room"]);
                    var remarks = Convert.ToString(r["Remarks"]);

                    var item = new ListViewItem(subject.Trim(new[] { ' ', '-' }));
                    item.SubItems.Add(day ?? string.Empty);
                    item.SubItems.Add(time);
                    item.SubItems.Add(room ?? string.Empty);
                    item.SubItems.Add(remarks ?? string.Empty);
                    item.Tag = Convert.ToInt32(r["ClassScheduleId"]);
                    _lvSchedules.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("ClassScheduleControl.LoadSchedules", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                _lvSchedules.EndUpdate();
            }
        }

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (_lvSchedules.SelectedItems.Count == 0) return;
            var item = _lvSchedules.SelectedItems[0];
            if (item.Tag == null) return;

            _editingScheduleId = Convert.ToInt32(item.Tag);

            // Try to populate fields from schedule list; best effort
            _cmbSubject.Text = item.SubItems[0].Text;
            _txtDay.Text = item.SubItems[1].Text;

            var timeText = item.SubItems.Count > 2 ? item.SubItems[2].Text : string.Empty;
            if (!string.IsNullOrWhiteSpace(timeText) && timeText.Contains("-"))
            {
                var parts = timeText.Split('-');
                DateTime parsed;
                if (DateTime.TryParse(parts[0], out parsed)) _dtStart.Value = parsed;
                if (parts.Length > 1 && DateTime.TryParse(parts[1], out parsed)) _dtEnd.Value = parsed;
            }

            _txtRoom.Text = item.SubItems.Count > 3 ? item.SubItems[3].Text : string.Empty;
            _txtRemarks.Text = item.SubItems.Count > 4 ? item.SubItems[4].Text : string.Empty;
            SetEditorState(false);
        }

        private void BeginAdd()
        {
            _editingScheduleId = 0;
            _cmbSubject.SelectedIndex = _cmbSubject.Items.Count > 0 ? 0 : -1;
            _txtDay.Text = "Mon/Wed";
            _dtStart.Value = DateTime.Today.AddHours(8);
            _dtEnd.Value = DateTime.Today.AddHours(9);
            _txtRoom.Text = string.Empty;
            _txtRemarks.Text = string.Empty;
            SetEditorState(true);
        }

        private void BeginEdit()
        {
            if (_editingScheduleId <= 0) return;
            SetEditorState(true);
        }

        private void CancelEdit()
        {
            SetEditorState(false);
            PreviewSelected();
        }

        private void SetEditorState(bool active)
        {
            _isEditorActive = active;
            _cmbSubject.Enabled = active;
            _txtDay.ReadOnly = !active;
            _dtStart.Enabled = active;
            _dtEnd.Enabled = active;
            _txtRoom.ReadOnly = !active;
            _txtRemarks.ReadOnly = !active;

            _btnSave.Enabled = active;
            _btnCancel.Enabled = active;
            _btnAdd.Enabled = !active;
            _btnEdit.Enabled = !active && _editingScheduleId > 0;
            _btnDelete.Enabled = !active && _editingScheduleId > 0;
        }

        private void Save()
        {
            if (_classScheduleService == null) return;
            if (_selectedSectionId <= 0) return;

            try
            {
                var schedule = new ClassSchedule
                {
                    ClassScheduleId = _editingScheduleId,
                    SectionId = _selectedSectionId,
                    SubjectId = _cmbSubject.SelectedValue == null ? 0 : Convert.ToInt32(_cmbSubject.SelectedValue),
                    DayOfWeek = (_txtDay.Text ?? string.Empty).Trim(),
                    StartTime = _dtStart.Value.TimeOfDay,
                    EndTime = _dtEnd.Value.TimeOfDay,
                    Room = (_txtRoom.Text ?? string.Empty).Trim(),
                    Remarks = (_txtRemarks.Text ?? string.Empty).Trim()
                };

                // Use section's academic year/semester for schedule rows
                var sectionRow = _sectionsTable == null ? null : _sectionsTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["SectionId"]) == _selectedSectionId);
                if (sectionRow != null)
                {
                    schedule.AcademicYearId = Convert.ToInt32(sectionRow["AcademicYearId"]);
                    schedule.SemesterId = Convert.ToInt32(sectionRow["SemesterId"]);
                }
                else
                {
                    schedule.AcademicYearId = _activeAcademicYearId ?? 0;
                    schedule.SemesterId = _activeSemesterId ?? 0;
                }

                var vr = _classScheduleService.Validate(schedule);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingScheduleId == 0)
                {
                    _classScheduleService.Create(schedule);
                    ThemedMessageBox.ShowInfo(this, "Schedule saved successfully.", "Saved");
                }
                else
                {
                    _classScheduleService.Update(schedule);
                    ThemedMessageBox.ShowInfo(this, "Schedule updated successfully.", "Updated");
                }

                LoadSchedules();
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("ClassScheduleControl.Save", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnSave.Enabled = true;
            }
        }

        private void DeleteCurrent()
        {
            if (_classScheduleService == null) return;
            if (_editingScheduleId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Schedule") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _classScheduleService.Delete(_editingScheduleId);
                LoadSchedules();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("ClassScheduleControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
