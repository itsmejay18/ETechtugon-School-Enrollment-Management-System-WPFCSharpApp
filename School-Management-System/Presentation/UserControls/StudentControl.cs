using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
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
        private string _currentPhotoPath;
        private string _selectedPhotoPath;
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
            LockSplitEditorPanel(_split, 460);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
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
                Padding = new Padding(8, 6, 8, 6)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
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
            formLayout.Controls.Add(MakeLabel("Address"), 0, 7);
            formLayout.Controls.Add(_txtAddress, 1, 7);
            formLayout.Controls.Add(MakeLabel("Phone"), 0, 8);
            formLayout.Controls.Add(_txtPhone, 1, 8);

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
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
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
            _lvEnrollments.Columns.Add("Enrollment #", 120);
            _lvEnrollments.Columns.Add("Subject", 240);
            _lvEnrollments.Columns.Add("Units", 60);
            _lvEnrollments.Columns.Add("Grade", 70);
            _lvEnrollments.Columns.Add("Section", 100);
            _lvEnrollments.Columns.Add("Term", 120);
            _lvEnrollments.Columns.Add("Schedule", 140);
            _gbEnrollment.Controls.Add(_lvEnrollments);

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
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
                Height = 180,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ThemeColors.Background
            };

            _btnUploadPhoto = new Button { Text = "Upload Photo", Dock = DockStyle.Top, Height = 36 };
            ThemeManager.StyleButtonNeutral(_btnUploadPhoto);
            _btnUploadPhoto.Click += (s, e) => UploadPhoto();

            _btnRemovePhoto = new Button { Text = "Remove Photo", Dock = DockStyle.Top, Height = 32 };
            ThemeManager.StyleButtonDanger(_btnRemovePhoto);
            _btnRemovePhoto.Click += (s, e) => ClearPhoto();

            panel.Controls.Add(_btnRemovePhoto);
            panel.Controls.Add(_btnUploadPhoto);
            panel.Controls.Add(_picPhoto);

            return panel;
        }

        private Panel BuildToolbar()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = ThemeColors.CardBackground, Padding = new Padding(10, 8, 10, 8) };

            var lblSearch = new Label { Text = "Last Name:", AutoSize = true, Location = new Point(0, 14), ForeColor = ThemeColors.MutedText, Font = ThemeFonts.Label };

            _txtSearch = new TextBox { Width = 220, Location = new Point(80, 10) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 86, Location = new Point(310, 8) };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            _btnAdd = new Button { Text = "New Student", Width = 110, Location = new Point(406, 8) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 86, Location = new Point(528, 8) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnDelete = new Button { Text = "Delete", Width = 86, Location = new Point(622, 8) };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            _btnSave = new Button { Text = "Save", Width = 86, Location = new Point(716, 8) };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 86, Location = new Point(810, 8) };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => CancelEdit();

            toolbar.Controls.Add(lblSearch);
            toolbar.Controls.Add(_txtSearch);
            toolbar.Controls.Add(_btnRefresh);
            toolbar.Controls.Add(_btnAdd);
            toolbar.Controls.Add(_btnEdit);
            toolbar.Controls.Add(_btnDelete);
            toolbar.Controls.Add(_btnSave);
            toolbar.Controls.Add(_btnCancel);
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
            if (_btnUploadPhoto != null) _btnUploadPhoto.Enabled = active;
            if (_btnRemovePhoto != null) _btnRemovePhoto.Enabled = active && !string.IsNullOrWhiteSpace(_currentPhotoPath);

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
            _currentPhotoPath = null;
            _selectedPhotoPath = null;
            ShowPhoto(null);
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
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = dt;

            if (_grid.Columns["StudentId"] != null)
            {
                _grid.Columns["StudentId"].Visible = false;
            }
            if (_grid.Columns["PhotoPath"] != null)
            {
                _grid.Columns["PhotoPath"].Visible = false;
            }

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
            _currentPhotoPath = row.Cells["PhotoPath"] == null ? null : Convert.ToString(row.Cells["PhotoPath"].Value);
            _selectedPhotoPath = null;
            ShowPhoto(_currentPhotoPath);

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

                var photoPathToSave = _currentPhotoPath;
                if (!string.IsNullOrWhiteSpace(_selectedPhotoPath))
                {
                    photoPathToSave = PersistPhoto(_selectedPhotoPath, student.StudentNumber);
                }
                student.PhotoPath = photoPathToSave;

                var vr = _studentService.Validate(student);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

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
                _currentPhotoPath = photoPathToSave;
                _selectedPhotoPath = null;
                ShowPhoto(_currentPhotoPath);
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
                _btnSave.Enabled = true;
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
            if (!_isEditorActive) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Select student photo";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                _selectedPhotoPath = ofd.FileName;
                ShowPhoto(_selectedPhotoPath);
                if (_btnRemovePhoto != null)
                {
                    _btnRemovePhoto.Enabled = true;
                }
            }
        }

        private void ClearPhoto()
        {
            if (!_isEditorActive) return;
            _selectedPhotoPath = null;
            _currentPhotoPath = null;
            ShowPhoto(null);
            if (_btnRemovePhoto != null)
            {
                _btnRemovePhoto.Enabled = false;
            }
        }

        private void ShowPhoto(string path)
        {
            if (_picPhoto == null) return;
            try
            {
                _picPhoto.Image = null;
                var fullPath = ResolvePhotoPath(path);
                if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                {
                    return;
                }

                using (var img = Image.FromFile(fullPath))
                {
                    _picPhoto.Image = new Bitmap(img);
                }
            }
            catch
            {
                _picPhoto.Image = null;
            }
        }

        private string ResolvePhotoPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        private string PersistPhoto(string sourcePath, string studentNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                {
                    return _currentPhotoPath;
                }

                var photosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos");
                if (!Directory.Exists(photosDir))
                {
                    Directory.CreateDirectory(photosDir);
                }

                var ext = Path.GetExtension(sourcePath);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    ext = ".jpg";
                }

                var safeName = string.IsNullOrWhiteSpace(studentNumber) ? "student" : studentNumber.Replace(" ", "_");
                var destFile = Path.Combine(photosDir, safeName + ext);
                File.Copy(sourcePath, destFile, true);

                // store relative path so it survives folder moves
                var relative = Path.Combine("Photos", safeName + ext);
                return relative;
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("StudentControl.PersistPhoto", ex);
                ThemedMessageBox.ShowError(this, "Unable to save photo. Please choose a different image.", "Photo");
                return _currentPhotoPath;
            }
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
