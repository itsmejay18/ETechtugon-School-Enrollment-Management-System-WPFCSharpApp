using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class StudentControl : BaseUserControl
    {
        private readonly StudentService _studentService;
        private readonly SystemSettingService _systemSettingService;

        private GroupBox _gb;
        private GroupBox _gbEnrollment;
        private TextBox _txtStudentNumber;
        private TextBox _txtFirstName;
        private TextBox _txtLastName;
        private TextBox _txtMiddleName;
        private ComboBox _cmbGender;
        private DateTimePicker _dtBirthDate;
        private TextBox _txtEmail;
        private TextBox _txtPhone;
        private TextBox _txtAddress;
        private PictureBox _picPhoto;
        private Button _btnUploadPhoto;
        private Button _btnRemovePhoto;

        private TextBox _txtSearch;
        private DataGridView _grid;
        private SplitContainer _split;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnRefresh;

        private int _editingStudentId;
        private bool _isEditorActive;
        private byte[] _currentPhotoBytes;
        private byte[] _selectedPhotoBytes;
        private ListView _lvEnrollments;
        private int? _activeAcademicYearId;
        private int? _activeSemesterId;

        public StudentControl()
            : this(null, null)
        {
        }

        public StudentControl(StudentService studentService, SystemSettingService systemSettingService)
        {
            _studentService = studentService;
            _systemSettingService = systemSettingService;

            var activeTerm = _systemSettingService == null ? (null, null) : _systemSettingService.GetActiveTerm();
            _activeAcademicYearId = activeTerm.Item1;
            _activeSemesterId = activeTerm.Item2;

            InitializeComponent();
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
                SplitterDistance = 730,
                BackColor = ThemeColors.Border
            };
            LockSplitEditorPanel(_split, 500, 360);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
            ConfigureCompactRecordGrid(_grid);
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.CellClick += (s, e) => PreviewSelected();

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            left.Controls.Add(_grid);
            _split.Panel1.Controls.Add(left);

            _gb = new GroupBox
            {
                Text = "Student Details",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(_gb);

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(6, 4, 6, 4)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            _txtStudentNumber = MakeTextBox(readOnly: true);
            _txtFirstName = MakeTextBox();
            _txtLastName = MakeTextBox();
            _txtMiddleName = MakeTextBox();
            _cmbGender = new ComboBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill };
            ThemeManager.StyleComboBox(_cmbGender);
            _cmbGender.Items.AddRange(new object[] { "", "Male", "Female", "Other" });
            _dtBirthDate = new DateTimePicker { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            ThemeManager.StyleDatePicker(_dtBirthDate);
            _txtEmail = MakeTextBox();
            _txtPhone = MakeTextBox();
            _txtAddress = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            ThemeManager.StyleInput(_txtAddress);

            formLayout.Controls.Add(MakeLabel("Student No."), 0, 0);
            formLayout.Controls.Add(_txtStudentNumber, 1, 0);
            formLayout.Controls.Add(MakeLabel("First Name *"), 0, 1);
            formLayout.Controls.Add(_txtFirstName, 1, 1);
            formLayout.Controls.Add(MakeLabel("Last Name *"), 0, 2);
            formLayout.Controls.Add(_txtLastName, 1, 2);
            formLayout.Controls.Add(MakeLabel("Middle Name"), 0, 3);
            formLayout.Controls.Add(_txtMiddleName, 1, 3);
            formLayout.Controls.Add(MakeLabel("Gender"), 0, 4);
            formLayout.Controls.Add(_cmbGender, 1, 4);
            formLayout.Controls.Add(MakeLabel("Birth Date"), 0, 5);
            formLayout.Controls.Add(_dtBirthDate, 1, 5);
            formLayout.Controls.Add(MakeLabel("Email"), 0, 6);
            formLayout.Controls.Add(_txtEmail, 1, 6);
            formLayout.Controls.Add(MakeLabel("Phone"), 0, 7);
            formLayout.Controls.Add(_txtPhone, 1, 7);
            formLayout.Controls.Add(MakeLabel("Address"), 0, 8);
            formLayout.Controls.Add(_txtAddress, 1, 8);

            var note = new Label
            {
                Text = "Click a row to preview. Click Edit to modify.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            formLayout.Controls.Add(note, 0, 9);
            formLayout.SetColumnSpan(note, 2);

            var photoPanel = BuildPhotoPanel();

            var detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            detailsLayout.Controls.Add(formLayout, 0, 0);
            detailsLayout.Controls.Add(photoPanel, 1, 0);

            _gb.Controls.Add(detailsLayout);

            _gbEnrollment = new GroupBox
            {
                Text = "Enrolled Subjects & Grades",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(_gbEnrollment);

            _lvEnrollments = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            ThemeManager.StyleListView(_lvEnrollments);
            _lvEnrollments.Columns.Add("Enrollment #", 108);
            _lvEnrollments.Columns.Add("Subject", 220);
            _lvEnrollments.Columns.Add("Units", 54);
            _lvEnrollments.Columns.Add("Grade", 58);
            _lvEnrollments.Columns.Add("Section", 88);
            _lvEnrollments.Columns.Add("Term", 118);
            _lvEnrollments.Columns.Add("Schedule", 130);
            _gbEnrollment.Controls.Add(_lvEnrollments);

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 64));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            rightLayout.Controls.Add(_gb, 0, 0);
            rightLayout.Controls.Add(_gbEnrollment, 0, 1);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            right.Controls.Add(rightLayout);
            _split.Panel2.Controls.Add(right);

            Controls.Add(_split);
            Controls.Add(toolbar);
        }

        private Panel BuildPhotoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6, 0, 0, 0), BackColor = ThemeColors.CardBackground };

            _picPhoto = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 156,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ThemeColors.Background
            };

            _btnUploadPhoto = new Button { Text = "Upload Photo", Dock = DockStyle.Top, Height = 34 };
            ThemeManager.StyleButtonNeutral(_btnUploadPhoto);
            _btnUploadPhoto.Click += (s, e) => UploadPhoto();

            _btnRemovePhoto = new Button { Text = "Remove Photo", Dock = DockStyle.Top, Height = 34 };
            ThemeManager.StyleButtonDanger(_btnRemovePhoto);
            _btnRemovePhoto.Click += (s, e) => ClearPhoto();

            panel.Controls.Add(_btnRemovePhoto);
            panel.Controls.Add(_btnUploadPhoto);
            panel.Controls.Add(_picPhoto);

            return panel;
        }

        private Panel BuildToolbar()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 0, 10) };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10, 8, 10, 8) };
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
                Width = 54,
                Height = 30,
                ForeColor = ThemeColors.MutedText,
                Font = ThemeFonts.Label,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 4, 0)
            };

            _txtSearch = new TextBox { Width = 200, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 82, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            _btnAdd = new Button { Text = "New Student", Width = 102, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 80, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnDelete = new Button { Text = "Delete", Width = 80, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            _btnSave = new Button { Text = "Save", Width = 80, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 80 };
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

        private static TextBox MakeTextBox(bool readOnly = false)
        {
            var tb = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, ReadOnly = readOnly };
            ThemeManager.StyleInput(tb);
            return tb;
        }

        private void SetEditorState(bool active)
        {
            _isEditorActive = active;
            _txtFirstName.ReadOnly = !active;
            _txtLastName.ReadOnly = !active;
            _txtMiddleName.ReadOnly = !active;
            _txtEmail.ReadOnly = !active;
            _txtPhone.ReadOnly = !active;
            _txtAddress.ReadOnly = !active;
            _cmbGender.Enabled = active;
            _dtBirthDate.Enabled = active;
            if (_btnUploadPhoto != null) _btnUploadPhoto.Enabled = true;
            if (_btnRemovePhoto != null) _btnRemovePhoto.Enabled = active && _currentPhotoBytes != null;

            _btnSave.Enabled = active;
            _btnCancel.Enabled = active;
            _btnAdd.Enabled = !active;
            _btnEdit.Enabled = !active && _editingStudentId > 0;
            _btnDelete.Enabled = !active && _editingStudentId > 0;
        }

        private void BeginAdd()
        {
            _editingStudentId = 0;
            _txtStudentNumber.Text = _studentService == null ? "STU-00001" : _studentService.GetNextStudentNumber();
            _txtFirstName.Text = string.Empty;
            _txtLastName.Text = string.Empty;
            _txtMiddleName.Text = string.Empty;
            _cmbGender.SelectedIndex = 0;
            _dtBirthDate.Checked = false;
            _txtEmail.Text = string.Empty;
            _txtPhone.Text = string.Empty;
            _txtAddress.Text = string.Empty;
            _currentPhotoBytes = null;
            _selectedPhotoBytes = null;
            ShowPhoto((byte[])null);
            if (_lvEnrollments != null)
            {
                _lvEnrollments.Items.Clear();
            }
            SetEditorState(true);
            _txtFirstName.Focus();
        }

        private void BeginEdit()
        {
            if (_editingStudentId <= 0) return;
            SetEditorState(true);
            _txtFirstName.Focus();
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
                var dt = _studentService == null ? new DataTable() : _studentService.GetStudents(_txtSearch == null ? string.Empty : _txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("StudentControl.LoadGrid", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void BindGrid(DataTable dt)
        {
            EnsureStudentDisplayColumns(dt);
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = dt;

            ConfigureGridColumnsForListView();

            if (_grid.Columns["PhotoPath"] != null)
            {
                _grid.Columns["PhotoPath"].Visible = false;
            }

            if (_grid.Columns["CreatedAt"] != null)
            {
                _grid.Columns["CreatedAt"].Visible = false;
            }

            if (_grid.Columns["UpdatedAt"] != null)
            {
                _grid.Columns["UpdatedAt"].Visible = false;
            }

            if (_grid.Columns["StudentId"] != null)
            {
                _grid.Columns["StudentId"].HeaderText = "ID";
                _grid.Columns["StudentId"].DisplayIndex = 0;
                _grid.Columns["StudentId"].FillWeight = 14f;
                _grid.Columns["StudentId"].MinimumWidth = 54;
            }

            if (_grid.Columns["FullName"] != null)
            {
                _grid.Columns["FullName"].HeaderText = "Student";
                _grid.Columns["FullName"].DisplayIndex = 1;
                _grid.Columns["FullName"].FillWeight = 42f;
                _grid.Columns["FullName"].MinimumWidth = 180;
            }

            if (_grid.Columns["StudentNumber"] != null)
            {
                _grid.Columns["StudentNumber"].HeaderText = "Number";
                _grid.Columns["StudentNumber"].DisplayIndex = 2;
                _grid.Columns["StudentNumber"].FillWeight = 24f;
                _grid.Columns["StudentNumber"].MinimumWidth = 126;
            }

            if (_grid.Columns["Gender"] != null)
            {
                _grid.Columns["Gender"].HeaderText = "Gender";
                _grid.Columns["Gender"].DisplayIndex = 3;
                _grid.Columns["Gender"].FillWeight = 20f;
                _grid.Columns["Gender"].MinimumWidth = 78;
            }

            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (_grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                PreviewSelected();
            }
            else
            {
                _editingStudentId = 0;
                _txtStudentNumber.Text = string.Empty;
                _txtFirstName.Text = string.Empty;
                _txtLastName.Text = string.Empty;
                _txtMiddleName.Text = string.Empty;
                _cmbGender.SelectedIndex = 0;
                _dtBirthDate.Checked = false;
                _txtEmail.Text = string.Empty;
                _txtPhone.Text = string.Empty;
                _txtAddress.Text = string.Empty;
                if (_lvEnrollments != null) _lvEnrollments.Items.Clear();
                SetEditorState(false);
            }
        }

        private void ConfigureGridColumnsForListView()
        {
            var allowed = new[] { "StudentId", "FullName", "StudentNumber", "Gender" };

            foreach (DataGridViewColumn col in _grid.Columns)
            {
                var show = false;
                foreach (var name in allowed)
                {
                    if (string.Equals(col.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        show = true;
                        break;
                    }
                }

                col.Visible = show;
            }
        }

        private static void EnsureStudentDisplayColumns(DataTable dt)
        {
            if (dt == null)
            {
                return;
            }

            if (!dt.Columns.Contains("FullName"))
            {
                dt.Columns.Add("FullName", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["FullName"] = BuildPersonName(
                    Convert.ToString(row["FirstName"]),
                    Convert.ToString(row["MiddleName"]),
                    Convert.ToString(row["LastName"]));
            }
        }

        private static string BuildPersonName(string firstName, string middleName, string lastName)
        {
            var givenName = (firstName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                givenName = (givenName + " " + middleName.Trim().Substring(0, 1).ToUpperInvariant() + ".").Trim();
            }

            var familyName = (lastName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(givenName)) return familyName;
            if (string.IsNullOrWhiteSpace(familyName)) return givenName;
            return familyName + ", " + givenName;
        }

        private static void ConfigureCompactRecordGrid(DataGridView grid)
        {
            if (grid == null)
            {
                return;
            }

            grid.ColumnHeadersHeight = 36;
            grid.RowTemplate.Height = 32;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            grid.DefaultCellStyle.Font = ThemeFonts.Label;
            grid.ColumnHeadersDefaultCellStyle.Font = ThemeFonts.SubHeader;
        }

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (_grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;
            if (row.Cells["StudentId"] == null || row.Cells["StudentId"].Value == null) return;

            _editingStudentId = Convert.ToInt32(row.Cells["StudentId"].Value);
            _txtStudentNumber.Text = Convert.ToString(row.Cells["StudentNumber"].Value);
            _txtFirstName.Text = Convert.ToString(row.Cells["FirstName"].Value);
            _txtLastName.Text = Convert.ToString(row.Cells["LastName"].Value);
            _txtMiddleName.Text = Convert.ToString(row.Cells["MiddleName"].Value);
            _currentPhotoBytes = _studentService?.GetPhotoData(_editingStudentId);
            _selectedPhotoBytes = null;
            ShowPhoto(_currentPhotoBytes);

            var gender = Convert.ToString(row.Cells["Gender"].Value);
            if (!string.IsNullOrWhiteSpace(gender) && _cmbGender.Items.Contains(gender))
            {
                _cmbGender.SelectedItem = gender;
            }
            else
            {
                _cmbGender.SelectedIndex = 0;
            }

            var birth = row.Cells["BirthDate"].Value;
            if (birth == null || birth == DBNull.Value)
            {
                _dtBirthDate.Checked = false;
            }
            else
            {
                _dtBirthDate.Checked = true;
                _dtBirthDate.Value = Convert.ToDateTime(birth);
            }

            _txtEmail.Text = Convert.ToString(row.Cells["Email"].Value);
            _txtPhone.Text = Convert.ToString(row.Cells["Phone"].Value);
            _txtAddress.Text = Convert.ToString(row.Cells["Address"].Value);
            LoadEnrollmentList();
            SetEditorState(false);
        }

        private void Save()
        {
            if (_studentService == null) return;

            try
            {
                var student = new Student
                {
                    StudentId = _editingStudentId,
                    StudentNumber = (_txtStudentNumber.Text ?? string.Empty).Trim(),
                    FirstName = (_txtFirstName.Text ?? string.Empty).Trim(),
                    LastName = (_txtLastName.Text ?? string.Empty).Trim(),
                    MiddleName = (_txtMiddleName.Text ?? string.Empty).Trim(),
                    Gender = Convert.ToString(_cmbGender.SelectedItem),
                    BirthDate = _dtBirthDate.Checked ? (DateTime?)_dtBirthDate.Value.Date : null,
                    Email = (_txtEmail.Text ?? string.Empty).Trim(),
                    Phone = (_txtPhone.Text ?? string.Empty).Trim(),
                    Address = (_txtAddress.Text ?? string.Empty).Trim()
                };

                var vr = _studentService.Validate(student);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                student.PhotoData = _selectedPhotoBytes ?? _currentPhotoBytes;

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingStudentId == 0)
                {
                    _studentService.Create(student);
                    ThemedMessageBox.ShowInfo(this, "Student saved successfully.", "Saved");
                }
                else
                {
                    _studentService.Update(student);
                    ThemedMessageBox.ShowInfo(this, "Student updated successfully.", "Updated");
                }

                LoadGrid();
                _currentPhotoBytes = student.PhotoData;
                _selectedPhotoBytes = null;
                ShowPhoto(_currentPhotoBytes);
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("StudentControl.Save", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnSave.Enabled = _isEditorActive;
            }
        }

        private void LoadEnrollmentList()
        {
            if (_lvEnrollments == null) return;
            if (_studentService == null || _editingStudentId <= 0)
            {
                _lvEnrollments.Items.Clear();
                return;
            }

            var began = false;
            try
            {
                var dt = _studentService.GetProfileSubjects(_editingStudentId, _activeAcademicYearId, _activeSemesterId);
                _lvEnrollments.BeginUpdate();
                began = true;
                _lvEnrollments.Items.Clear();

                foreach (DataRow r in dt.Rows)
                {
                    var enrollmentNo = Convert.ToString(r["EnrollmentNumber"]);
                    var subjectLabel = ((Convert.ToString(r["SubjectCode"]) ?? string.Empty) + " - " + (Convert.ToString(r["SubjectName"]) ?? string.Empty)).Trim(new[] { ' ', '-' });
                    var units = Convert.ToString(r["Units"]);
                    var grade = r["Grade"] == DBNull.Value ? string.Empty : Convert.ToDecimal(r["Grade"]).ToString("0.00");
                    var section = Convert.ToString(r["SectionName"]);
                    var termParts = new[]
                    {
                        Convert.ToString(r["YearLevel"]),
                        Convert.ToString(r["Semester"]),
                        Convert.ToString(r["AcademicYear"])
                    };
                    var term = string.Join(" | ", Array.FindAll(termParts, p => !string.IsNullOrWhiteSpace(p)));
                    var schedule = BuildScheduleText(r);

                    var item = new ListViewItem(enrollmentNo ?? string.Empty);
                    item.SubItems.Add(subjectLabel);
                    item.SubItems.Add(units ?? string.Empty);
                    item.SubItems.Add(grade);
                    item.SubItems.Add(section ?? string.Empty);
                    item.SubItems.Add(term);
                    item.SubItems.Add(schedule);
                    _lvEnrollments.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("StudentControl.LoadEnrollmentList", ex);
            }
            finally
            {
                if (began)
                {
                    _lvEnrollments.EndUpdate();
                }
            }
        }

        private static string BuildScheduleText(DataRow r)
        {
            if (r == null) return string.Empty;
            var day = Convert.ToString(r["DayOfWeek"]);

            string start = null;
            string end = null;
            try
            {
                var startTime = r["StartTime"] as TimeSpan?;
                if (startTime.HasValue)
                {
                    start = startTime.Value.ToString(@"hh\\:mm");
                }
                var endTime = r["EndTime"] as TimeSpan?;
                if (endTime.HasValue)
                {
                    end = endTime.Value.ToString(@"hh\\:mm");
                }
            }
            catch
            {
                // ignore parse issues
            }

            var time = string.Empty;
            if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
            {
                time = start + "-" + end;
            }

            var room = Convert.ToString(r["Room"]);

            var parts = new[] { day, time, room };
            return string.Join(" | ", Array.FindAll(parts, p => !string.IsNullOrWhiteSpace(p)));
        }

        private void UploadPhoto()
        {
            if (!_isEditorActive)
            {
                if (_editingStudentId > 0)
                {
                    BeginEdit();
                }
                else
                {
                    BeginAdd();
                }
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Select student photo";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                _selectedPhotoBytes = PhotoStorageHelper.ReadPhotoBytes(ofd.FileName);
                ShowPhoto(_selectedPhotoBytes);
                if (_btnRemovePhoto != null)
                {
                    _btnRemovePhoto.Enabled = true;
                }
            }
        }

        private void ClearPhoto()
        {
            if (!_isEditorActive) return;
            _selectedPhotoBytes = null;
            _currentPhotoBytes = null;
            ShowPhoto((byte[])null);
            if (_btnRemovePhoto != null)
            {
                _btnRemovePhoto.Enabled = false;
            }
        }

        private void ShowPhoto(byte[] data)
        {
            PhotoStorageHelper.ShowPhotoFromBytes(_picPhoto, data);
        }

        private void DeleteCurrent()
        {
            if (_studentService == null) return;
            if (_editingStudentId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Student") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _studentService.Delete(_editingStudentId);
                LoadGrid();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("StudentControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
