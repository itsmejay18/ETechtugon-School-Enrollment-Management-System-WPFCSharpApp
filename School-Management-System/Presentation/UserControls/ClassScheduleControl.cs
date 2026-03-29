using System;
using System.Collections.Generic;
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
        private SplitContainer _splitMain;

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
        private DataTable _scheduleTable;

        private MonthCalendar _calendar;
        private ListView _lvCalendarEvents;
        private Label _lblCalendarMonth;
        private Label _lblCalendarInfo;
        private DateTime _calendarMonth;
        private bool _suppressCalendarEvents;
        private readonly Dictionary<DateTime, List<GeneratedScheduleEntry>> _generatedScheduleByDate = new Dictionary<DateTime, List<GeneratedScheduleEntry>>();

        private sealed class GeneratedScheduleEntry
        {
            public string SubjectLabel { get; set; }
            public TimeSpan? StartTime { get; set; }
            public TimeSpan? EndTime { get; set; }
            public string Room { get; set; }
            public string Remarks { get; set; }
        }

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

            var header = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(0, 0, 0, 12), BackColor = ThemeColors.Background };
            var headerCard = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(12, 8, 12, 8) };
            ThemeManager.StyleCardPanel(headerCard);

            var headerStrip = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            var lblSection = new Label
            {
                Text = "Section:",
                AutoSize = false,
                Width = 58,
                Height = 32,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 6, 0)
            };
            _cmbSection = new ComboBox { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeFonts.Input, Margin = new Padding(0, 0, 10, 0) };
            ThemeManager.StyleComboBox(_cmbSection);
            _cmbSection.SelectedIndexChanged += (s, e) => OnSectionChanged();

            _btnReloadSections = new Button { Text = "Reload", Width = 96 };
            ThemeManager.StyleButtonNeutral(_btnReloadSections);
            _btnReloadSections.Click += (s, e) => LoadSections();

            headerStrip.Controls.Add(lblSection);
            headerStrip.Controls.Add(_cmbSection);
            headerStrip.Controls.Add(_btnReloadSections);
            headerCard.Controls.Add(headerStrip);
            header.Controls.Add(headerCard);

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

            _splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                BackColor = ThemeColors.Border
            };
            _splitMain.Panel1MinSize = 120;
            _splitMain.Panel2MinSize = 120;
            _splitMain.SizeChanged += (s, e) =>
            {
                EnsureCalendarPanelWidth();
                UpdateCalendarEventColumns();
            };

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(0, 0, 10, 0) };
            left.Controls.Add(_lvSchedules);
            _splitMain.Panel1.Controls.Add(left);

            var calendarPanel = BuildCalendarPanel();
            _splitMain.Panel2.Controls.Add(calendarPanel);

            var editor = BuildEditorPanel();

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 10), BackColor = ThemeColors.Background };
            body.Controls.Add(editor);
            body.Controls.Add(_splitMain);
            body.Controls.Add(header);

            Controls.Add(body);
            EnsureCalendarPanelWidth();
            if (!IsHandleCreated)
            {
                EventHandler onHandleCreated = null;
                onHandleCreated = (s, e) =>
                {
                    EnsureCalendarPanelWidth();
                    HandleCreated -= onHandleCreated;
                };
                HandleCreated += onHandleCreated;
            }
        }

        private Panel BuildEditorPanel()
        {
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 188, Padding = new Padding(0, 8, 0, 0), BackColor = ThemeColors.Background };
            var frame = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            ThemeManager.StyleCardPanel(frame);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(4, 2, 4, 2),
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
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

            var btnPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Right,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
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

            var buttonRow = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            buttonRow.Controls.Add(btnPanel);

            var frameLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            frameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            frameLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            frameLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            frameLayout.Controls.Add(layout, 0, 0);
            frameLayout.Controls.Add(buttonRow, 0, 1);

            frame.Controls.Add(frameLayout);
            panel.Controls.Add(frame);
            return panel;
        }

        private Panel BuildCalendarPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 0, 0) };
            var frame = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            ThemeManager.StyleCardPanel(frame);

            var monthNav = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                ColumnCount = 4
            };
            monthNav.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            monthNav.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            monthNav.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            monthNav.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var btnPrev = new Button { Text = "<", Dock = DockStyle.Fill };
            ThemeManager.StyleButtonNeutral(btnPrev);
            btnPrev.Click += (s, e) => ChangeCalendarMonth(-1);

            var btnNext = new Button { Text = ">", Dock = DockStyle.Fill };
            ThemeManager.StyleButtonNeutral(btnNext);
            btnNext.Click += (s, e) => ChangeCalendarMonth(1);

            var btnToday = new Button { Text = "Today", Dock = DockStyle.Fill };
            ThemeManager.StyleButtonNeutral(btnToday);
            btnToday.Click += (s, e) =>
            {
                var today = DateTime.Today;
                _calendarMonth = new DateTime(today.Year, today.Month, 1);
                SetCalendarDate(today);
                GenerateCalendarForMonth();
            };

            _lblCalendarMonth = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleRight
            };

            monthNav.Controls.Add(btnPrev, 0, 0);
            monthNav.Controls.Add(btnNext, 1, 0);
            monthNav.Controls.Add(btnToday, 2, 0);
            monthNav.Controls.Add(_lblCalendarMonth, 3, 0);

            _calendar = new MonthCalendar
            {
                Dock = DockStyle.Top,
                MaxSelectionCount = 1,
                ShowWeekNumbers = true,
                FirstDayOfWeek = Day.Monday
            };
            _calendar.DateChanged += (s, e) =>
            {
                if (_suppressCalendarEvents)
                {
                    return;
                }

                var month = new DateTime(_calendar.SelectionStart.Year, _calendar.SelectionStart.Month, 1);
                if (month != _calendarMonth)
                {
                    _calendarMonth = month;
                    GenerateCalendarForMonth();
                }
                else
                {
                    RenderCalendarForDate(_calendar.SelectionStart.Date);
                }
            };

            _lblCalendarInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lvCalendarEvents = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _lvCalendarEvents.Columns.Add("Time", 90);
            _lvCalendarEvents.Columns.Add("Subject", 220);
            _lvCalendarEvents.SizeChanged += (s, e) => UpdateCalendarEventColumns();

            frame.Controls.Add(_lvCalendarEvents);
            frame.Controls.Add(_lblCalendarInfo);
            frame.Controls.Add(_calendar);
            frame.Controls.Add(monthNav);
            panel.Controls.Add(frame);

            _calendarMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            SetCalendarDate(DateTime.Today);
            UpdateCalendarEventColumns();

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
                    _cmbSection.DisplayMember = "SectionName";
                    _cmbSection.ValueMember = "SectionId";
                    _cmbSection.DataSource = view;
                }
                else
                {
                    _cmbSection.DisplayMember = "SectionName";
                    _cmbSection.ValueMember = "SectionId";
                    _cmbSection.DataSource = dt;
                }

                if (_cmbSection.Items.Count > 0)
                {
                    _cmbSection.SelectedIndex = 0;
                    var selected = _cmbSection.SelectedValue;
                    int sectionId;
                    if (TryResolveSectionId(selected, out sectionId))
                    {
                        _selectedSectionId = sectionId;
                        LoadSubjectsForSection(sectionId);
                        LoadSchedules();
                    }
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
            int sectionId;
            if (!TryResolveSectionId(_cmbSection.SelectedValue, out sectionId) &&
                !TryResolveSectionId(_cmbSection.SelectedItem, out sectionId))
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
                _scheduleTable = null;
                _lvSchedules.Items.Clear();
                GenerateCalendarForMonth();
                return;
            }

            try
            {
                var dt = _classScheduleService.GetBySection(_selectedSectionId);
                _scheduleTable = dt;
                _lvSchedules.BeginUpdate();
                _lvSchedules.Items.Clear();

                foreach (DataRow r in dt.Rows)
                {
                    var subject = Convert.ToString(r["SubjectCode"]) + " - " + Convert.ToString(r["SubjectName"]);
                    var day = Convert.ToString(r["DayOfWeek"]);
                    var start = FormatTimeCell(r["StartTime"]);
                    var end = FormatTimeCell(r["EndTime"]);
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
                GenerateCalendarForMonth();
            }
        }

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (!HasSelectedListItem(_lvSchedules))
            {
                _editingScheduleId = 0;
                SetEditorState(false);
                return;
            }
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
            ApplyCrudButtonState(active, HasSelectedListItem(_lvSchedules), _btnAdd, _btnEdit, _btnDelete, _btnSave, _btnCancel);
        }

        private void ChangeCalendarMonth(int offset)
        {
            _calendarMonth = _calendarMonth.AddMonths(offset);
            var selectedDay = _calendar == null ? 1 : _calendar.SelectionStart.Day;
            var maxDay = DateTime.DaysInMonth(_calendarMonth.Year, _calendarMonth.Month);
            var targetDate = new DateTime(_calendarMonth.Year, _calendarMonth.Month, Math.Min(selectedDay, maxDay));
            SetCalendarDate(targetDate);
            GenerateCalendarForMonth();
        }

        private void SetCalendarDate(DateTime date)
        {
            if (_calendar == null)
            {
                return;
            }

            _suppressCalendarEvents = true;
            try
            {
                _calendar.SetDate(date.Date);
            }
            finally
            {
                _suppressCalendarEvents = false;
            }
        }

        private void GenerateCalendarForMonth()
        {
            if (_calendar == null || _lvCalendarEvents == null)
            {
                return;
            }

            _generatedScheduleByDate.Clear();

            var monthStart = new DateTime(_calendarMonth.Year, _calendarMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            _lblCalendarMonth.Text = monthStart.ToString("MMMM yyyy");

            if (_scheduleTable != null)
            {
                foreach (DataRow row in _scheduleTable.Rows)
                {
                    var days = ParseScheduleDays(Convert.ToString(row["DayOfWeek"]));
                    if (days.Count == 0)
                    {
                        continue;
                    }

                    var entry = new GeneratedScheduleEntry
                    {
                        SubjectLabel = (Convert.ToString(row["SubjectCode"]) + " - " + Convert.ToString(row["SubjectName"])).Trim(new[] { ' ', '-' }),
                        StartTime = row["StartTime"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)row["StartTime"],
                        EndTime = row["EndTime"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)row["EndTime"],
                        Room = Convert.ToString(row["Room"]),
                        Remarks = Convert.ToString(row["Remarks"])
                    };

                    foreach (var day in days)
                    {
                        for (var date = FirstDateForDay(monthStart, day); date <= monthEnd; date = date.AddDays(7))
                        {
                            List<GeneratedScheduleEntry> list;
                            if (!_generatedScheduleByDate.TryGetValue(date.Date, out list))
                            {
                                list = new List<GeneratedScheduleEntry>();
                                _generatedScheduleByDate[date.Date] = list;
                            }
                            list.Add(entry);
                        }
                    }
                }
            }

            foreach (var entries in _generatedScheduleByDate.Values)
            {
                entries.Sort((a, b) =>
                {
                    var startCompare = Nullable.Compare(a.StartTime, b.StartTime);
                    if (startCompare != 0) return startCompare;
                    return string.Compare(a.SubjectLabel, b.SubjectLabel, StringComparison.OrdinalIgnoreCase);
                });
            }

            _calendar.RemoveAllBoldedDates();
            foreach (var date in _generatedScheduleByDate.Keys)
            {
                _calendar.AddBoldedDate(date);
            }
            _calendar.UpdateBoldedDates();

            var selectedDate = _calendar.SelectionStart.Date;
            if (!_generatedScheduleByDate.ContainsKey(selectedDate) && _generatedScheduleByDate.Count > 0)
            {
                var nearestDate = _generatedScheduleByDate.Keys.OrderBy(d => d).First();
                SetCalendarDate(nearestDate);
                selectedDate = nearestDate;
            }

            RenderCalendarForDate(selectedDate);
        }

        private void RenderCalendarForDate(DateTime selectedDate)
        {
            if (_lvCalendarEvents == null || _lblCalendarInfo == null)
            {
                return;
            }

            _lvCalendarEvents.BeginUpdate();
            _lvCalendarEvents.Items.Clear();

            List<GeneratedScheduleEntry> entries;
            if (_generatedScheduleByDate.TryGetValue(selectedDate.Date, out entries) && entries.Count > 0)
            {
                foreach (var entry in entries)
                {
                    var start = entry.StartTime.HasValue ? entry.StartTime.Value.ToString(@"hh\:mm") : string.Empty;
                    var end = entry.EndTime.HasValue ? entry.EndTime.Value.ToString(@"hh\:mm") : string.Empty;
                    var time = string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end) ? string.Empty : start + "-" + end;

                    var item = new ListViewItem(time);
                    item.SubItems.Add(BuildCalendarSubjectText(entry));
                    _lvCalendarEvents.Items.Add(item);
                }

                _lblCalendarInfo.Text = entries.Count + " class(es) on " + selectedDate.ToString("dddd, MMMM d");
            }
            else
            {
                _lblCalendarInfo.Text = "No generated classes on " + selectedDate.ToString("dddd, MMMM d");
            }

            _lvCalendarEvents.EndUpdate();
        }

        private static DateTime FirstDateForDay(DateTime monthStart, DayOfWeek dayOfWeek)
        {
            var diff = ((int)dayOfWeek - (int)monthStart.DayOfWeek + 7) % 7;
            return monthStart.AddDays(diff);
        }

        private static List<DayOfWeek> ParseScheduleDays(string dayText)
        {
            var found = new HashSet<DayOfWeek>();
            if (string.IsNullOrWhiteSpace(dayText))
            {
                return found.ToList();
            }

            var value = dayText.Trim().ToLowerInvariant();

            if (value.Contains("mon")) found.Add(DayOfWeek.Monday);
            if (value.Contains("tue")) found.Add(DayOfWeek.Tuesday);
            if (value.Contains("wed")) found.Add(DayOfWeek.Wednesday);
            if (value.Contains("thu")) found.Add(DayOfWeek.Thursday);
            if (value.Contains("fri")) found.Add(DayOfWeek.Friday);
            if (value.Contains("sat")) found.Add(DayOfWeek.Saturday);
            if (value.Contains("sun")) found.Add(DayOfWeek.Sunday);

            if (found.Count == 0)
            {
                var normalized = value.Replace("/", " ").Replace(",", " ").Replace("-", " ").Replace("|", " ").Replace(";", " ");
                var parts = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    switch (part)
                    {
                        case "m":
                            found.Add(DayOfWeek.Monday);
                            break;
                        case "tu":
                        case "t":
                            found.Add(DayOfWeek.Tuesday);
                            break;
                        case "w":
                            found.Add(DayOfWeek.Wednesday);
                            break;
                        case "th":
                        case "h":
                            found.Add(DayOfWeek.Thursday);
                            break;
                        case "f":
                            found.Add(DayOfWeek.Friday);
                            break;
                        case "sa":
                            found.Add(DayOfWeek.Saturday);
                            break;
                        case "su":
                            found.Add(DayOfWeek.Sunday);
                            break;
                    }
                }
            }

            return found.OrderBy(d => d == DayOfWeek.Sunday ? 7 : (int)d).ToList();
        }

        private void EnsureCalendarPanelWidth()
        {
            if (_splitMain == null || _splitMain.IsDisposed || _splitMain.Width <= 0)
            {
                return;
            }

            const int desiredCalendarPanelWidth = 360;
            var width = _splitMain.Width;
            var safePanel2Min = width >= 860 ? 320 : (width >= 740 ? 280 : 220);
            var safePanel1Min = width >= 860 ? 420 : (width >= 740 ? 360 : 280);

            var required = safePanel1Min + safePanel2Min + _splitMain.SplitterWidth;
            if (required >= width)
            {
                safePanel1Min = 120;
                safePanel2Min = 120;
            }

            try
            {
                _splitMain.Panel1MinSize = safePanel1Min;
                _splitMain.Panel2MinSize = safePanel2Min;

                var minSplitterDistance = _splitMain.Panel1MinSize;
                var maxSplitterDistance = _splitMain.Width - _splitMain.SplitterWidth - _splitMain.Panel2MinSize;
                if (maxSplitterDistance <= 0 || maxSplitterDistance < minSplitterDistance)
                {
                    return;
                }

                var targetDistance = _splitMain.Width - _splitMain.SplitterWidth - desiredCalendarPanelWidth;
                if (targetDistance < minSplitterDistance)
                {
                    targetDistance = minSplitterDistance;
                }

                if (targetDistance > maxSplitterDistance)
                {
                    targetDistance = maxSplitterDistance;
                }

                if (targetDistance > 0 && _splitMain.SplitterDistance != targetDistance)
                {
                    _splitMain.SplitterDistance = targetDistance;
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore transient split-container layout states during initial docking.
            }
        }

        private void UpdateCalendarEventColumns()
        {
            if (_lvCalendarEvents == null || _lvCalendarEvents.Columns.Count < 2)
            {
                return;
            }

            var availableWidth = Math.Max(160, _lvCalendarEvents.ClientSize.Width - 6);
            var timeWidth = Math.Min(100, Math.Max(82, availableWidth / 3));
            var subjectWidth = Math.Max(76, availableWidth - timeWidth);

            _lvCalendarEvents.Columns[0].Width = timeWidth;
            _lvCalendarEvents.Columns[1].Width = subjectWidth;
        }

        private static string BuildCalendarSubjectText(GeneratedScheduleEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            var label = entry.SubjectLabel ?? string.Empty;
            var separatorIndex = label.IndexOf(" - ", StringComparison.Ordinal);
            if (separatorIndex >= 0 && separatorIndex + 3 < label.Length)
            {
                label = label.Substring(separatorIndex + 3);
            }

            if (!string.IsNullOrWhiteSpace(entry.Room))
            {
                label += " (" + entry.Room.Trim() + ")";
            }

            return label.Trim();
        }

        private static string FormatTimeCell(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (value is TimeSpan)
            {
                return ((TimeSpan)value).ToString(@"hh\:mm");
            }

            if (value is DateTime)
            {
                return ((DateTime)value).ToString("HH:mm");
            }

            TimeSpan parsedTimeSpan;
            if (TimeSpan.TryParse(Convert.ToString(value), out parsedTimeSpan))
            {
                return parsedTimeSpan.ToString(@"hh\:mm");
            }

            DateTime parsedDateTime;
            if (DateTime.TryParse(Convert.ToString(value), out parsedDateTime))
            {
                return parsedDateTime.ToString("HH:mm");
            }

            return string.Empty;
        }

        private static bool TryResolveSectionId(object value, out int sectionId)
        {
            sectionId = 0;
            if (value == null || value == DBNull.Value)
            {
                return false;
            }

            if (value is int)
            {
                sectionId = (int)value;
                return sectionId > 0;
            }

            if (value is long)
            {
                sectionId = Convert.ToInt32((long)value);
                return sectionId > 0;
            }

            var rowView = value as DataRowView;
            if (rowView != null && rowView.Row != null && rowView.Row.Table.Columns.Contains("SectionId"))
            {
                var raw = rowView["SectionId"];
                if (raw != null && raw != DBNull.Value && int.TryParse(Convert.ToString(raw), out sectionId))
                {
                    return sectionId > 0;
                }
            }

            return int.TryParse(Convert.ToString(value), out sectionId) && sectionId > 0;
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
                _btnSave.Enabled = _isEditorActive;
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
