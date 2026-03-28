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
    public sealed class FacultyControl : BaseUserControl
    {
        private readonly FacultyService _facultyService;
        private readonly ClassScheduleService _classScheduleService;

        private GroupBox _gbDetails = new GroupBox();
        private GroupBox _gbProfile = new GroupBox();
        private TextBox _txtFacultyCode = new TextBox();
        private TextBox _txtFirstName = new TextBox();
        private TextBox _txtLastName = new TextBox();
        private TextBox _txtMiddleName = new TextBox();
        private TextBox _txtEmail = new TextBox();
        private TextBox _txtPhone = new TextBox();
        private TextBox _txtAddress = new TextBox();
        private DateTimePicker _dtHireDate = new DateTimePicker();
        private PictureBox _picPhoto = new PictureBox();
        private Button _btnUploadPhoto = new Button();
        private Button _btnRemovePhoto = new Button();

        private Label _lblProfileName = new Label();
        private Label _lblProfileNumberValue = new Label();
        private Label _lblProfileStatusValue = new Label();
        private Label _lblProfileContactValue = new Label();
        private Label _lblProfileHireDateValue = new Label();
        private Label _lblProfileAssignmentsValue = new Label();
        private ListView _lvAssignments = new ListView();

        private TextBox _txtSearch = new TextBox();
        private DataGridView _grid = new DataGridView();
        private SplitContainer _split = new SplitContainer();

        private Button _btnAdd = new Button();
        private Button _btnEdit = new Button();
        private Button _btnProfile = new Button();
        private Button _btnDelete = new Button();
        private Button _btnSave = new Button();
        private Button _btnCancel = new Button();
        private Button _btnRefresh = new Button();

        private int _editingFacultyId;
        private bool _isEditorActive;
        private byte[] _currentPhotoBytes;
        private byte[] _selectedPhotoBytes;

        public FacultyControl()
            : this(null, null)
        {
        }

        public FacultyControl(FacultyService facultyService, ClassScheduleService classScheduleService)
        {
            _facultyService = facultyService;
            _classScheduleService = classScheduleService;
            InitializeComponent();
            SetEditorState(false);
            LoadGrid();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Name = "FacultyControl";
            Size = new Size(852, 179);
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
            _grid.CellDoubleClick += (s, e) => OpenProfileWindow();

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            left.Controls.Add(_grid);
            _split.Panel1.Controls.Add(left);

            BuildDetailsGroup();
            BuildProfileGroup();

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            rightLayout.Controls.Add(_gbDetails, 0, 0);
            rightLayout.Controls.Add(_gbProfile, 0, 1);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            right.Controls.Add(rightLayout);
            _split.Panel2.Controls.Add(right);

            Controls.Clear();
            Controls.Add(_split);
            Controls.Add(toolbar);

            ResumeLayout(false);
        }

        private void BuildDetailsGroup()
        {
            _gbDetails = new GroupBox
            {
                Text = "Faculty Details",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(_gbDetails);

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(6, 4, 6, 4)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            _txtFacultyCode = MakeTextBox(readOnly: true);
            _txtFirstName = MakeTextBox();
            _txtLastName = MakeTextBox();
            _txtMiddleName = MakeTextBox();
            _txtEmail = MakeTextBox();
            _txtPhone = MakeTextBox();
            _dtHireDate = new DateTimePicker { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            ThemeManager.StyleDatePicker(_dtHireDate);
            _txtAddress = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            ThemeManager.StyleInput(_txtAddress);

            formLayout.Controls.Add(MakeLabel("Faculty No."), 0, 0);
            formLayout.Controls.Add(_txtFacultyCode, 1, 0);
            formLayout.Controls.Add(MakeLabel("First Name *"), 0, 1);
            formLayout.Controls.Add(_txtFirstName, 1, 1);
            formLayout.Controls.Add(MakeLabel("Last Name *"), 0, 2);
            formLayout.Controls.Add(_txtLastName, 1, 2);
            formLayout.Controls.Add(MakeLabel("Middle Name"), 0, 3);
            formLayout.Controls.Add(_txtMiddleName, 1, 3);
            formLayout.Controls.Add(MakeLabel("Email"), 0, 4);
            formLayout.Controls.Add(_txtEmail, 1, 4);
            formLayout.Controls.Add(MakeLabel("Phone"), 0, 5);
            formLayout.Controls.Add(_txtPhone, 1, 5);
            formLayout.Controls.Add(MakeLabel("Hire Date"), 0, 6);
            formLayout.Controls.Add(_dtHireDate, 1, 6);
            formLayout.Controls.Add(MakeLabel("Address"), 0, 7);
            formLayout.Controls.Add(_txtAddress, 1, 7);

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

            _gbDetails.Controls.Add(detailsLayout);
        }

        private void BuildProfileGroup()
        {
            _gbProfile = new GroupBox
            {
                Text = "Faculty Profile",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(_gbProfile);

            var summaryCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 132,
                Padding = new Padding(14, 12, 14, 12),
                BackColor = ThemeColors.Surface
            };
            ThemeManager.StyleCardPanel(summaryCard);

            _lblProfileName = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            var summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(0, 8, 0, 0)
            };
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            summaryLayout.Controls.Add(CreateProfileValueHost("Faculty No.", out _lblProfileNumberValue), 0, 0);
            summaryLayout.Controls.Add(CreateProfileValueHost("Status", out _lblProfileStatusValue), 1, 0);
            summaryLayout.Controls.Add(CreateProfileValueHost("Contact", out _lblProfileContactValue), 0, 1);
            summaryLayout.Controls.Add(CreateProfileValueHost("Hire Date", out _lblProfileHireDateValue), 1, 1);
            var assignmentHost = CreateProfileValueHost("Assignments", out _lblProfileAssignmentsValue);
            summaryLayout.Controls.Add(assignmentHost, 0, 2);
            summaryLayout.SetColumnSpan(assignmentHost, 2);

            summaryCard.Controls.Add(summaryLayout);
            summaryCard.Controls.Add(_lblProfileName);

            var assignmentsLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Padding = new Padding(2, 6, 0, 0),
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Text = "Assigned subjects and schedule"
            };

            _lvAssignments = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            ThemeManager.StyleListView(_lvAssignments);
            _lvAssignments.Columns.Add("Subject", 165);
            _lvAssignments.Columns.Add("Section", 84);
            _lvAssignments.Columns.Add("Schedule", 128);
            _lvAssignments.Columns.Add("Room", 70);
            _lvAssignments.Columns.Add("Term", 118);

            _gbProfile.Controls.Add(_lvAssignments);
            _gbProfile.Controls.Add(assignmentsLabel);
            _gbProfile.Controls.Add(summaryCard);

            ResetProfile();
        }

        private Control CreateProfileValueHost(string caption, out Label valueLabel)
        {
            var host = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var captionLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = caption,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            valueLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            host.Controls.Add(captionLabel, 0, 0);
            host.Controls.Add(valueLabel, 1, 0);
            return host;
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

            _txtSearch = new TextBox { Width = 210, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 82, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            _btnAdd = new Button { Text = "Add", Width = 80, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 80, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnProfile = new Button { Text = "Profile", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnProfile);
            _btnProfile.Click += (s, e) => OpenProfileWindow();

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
            strip.Controls.Add(_btnProfile);
            strip.Controls.Add(_btnDelete);
            strip.Controls.Add(_btnSave);
            strip.Controls.Add(_btnCancel);
            card.Controls.Add(strip);
            toolbar.Controls.Add(card);
            return toolbar;
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
            if (_txtFirstName == null) return;

            _txtFirstName.ReadOnly = !active;
            _txtLastName.ReadOnly = !active;
            _txtMiddleName.ReadOnly = !active;
            _txtEmail.ReadOnly = !active;
            _txtPhone.ReadOnly = !active;
            _txtAddress.ReadOnly = !active;
            _dtHireDate.Enabled = active;
            if (_btnUploadPhoto != null) _btnUploadPhoto.Enabled = true;
            if (_btnRemovePhoto != null) _btnRemovePhoto.Enabled = active && (_selectedPhotoBytes != null || _currentPhotoBytes != null);

            _btnSave.Enabled = active;
            _btnCancel.Enabled = active;
            _btnAdd.Enabled = !active;
            _btnEdit.Enabled = !active && _editingFacultyId > 0;
            _btnProfile.Enabled = !active && _editingFacultyId > 0;
            _btnDelete.Enabled = !active && _editingFacultyId > 0;
        }

        private void BeginAdd()
        {
            _editingFacultyId = 0;
            _txtFacultyCode.Text = _facultyService == null ? "FAC-00001" : _facultyService.GetNextFacultyCode();
            _txtFirstName.Text = string.Empty;
            _txtLastName.Text = string.Empty;
            _txtMiddleName.Text = string.Empty;
            _txtEmail.Text = string.Empty;
            _txtPhone.Text = string.Empty;
            _txtAddress.Text = string.Empty;
            _currentPhotoBytes = null;
            _selectedPhotoBytes = null;
            ShowPhoto((byte[])null);
            _dtHireDate.Checked = false;
            SetEditorState(true);
            ResetProfile("Draft faculty profile", _txtFacultyCode.Text, "Draft", "No contact information", "Not set", "No subject assignments yet.");
            _txtFirstName.Focus();
        }

        private void BeginEdit()
        {
            if (_editingFacultyId <= 0) return;
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
                var dt = _facultyService == null ? new DataTable() : _facultyService.GetFaculty(_txtSearch == null ? string.Empty : _txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("FacultyControl.LoadGrid", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void BindGrid(DataTable dt)
        {
            EnsureFacultyDisplayColumns(dt);

            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = dt;

            ConfigureGridColumnsForListView();
            if (_grid.Columns["PhotoPath"] != null)
            {
                _grid.Columns["PhotoPath"].Visible = false;
            }

            if (_grid.Columns["FacultyId"] != null)
            {
                _grid.Columns["FacultyId"].HeaderText = "ID";
                _grid.Columns["FacultyId"].DisplayIndex = 0;
                _grid.Columns["FacultyId"].FillWeight = 14f;
                _grid.Columns["FacultyId"].MinimumWidth = 54;
            }

            if (_grid.Columns["FullName"] != null)
            {
                _grid.Columns["FullName"].HeaderText = "Faculty";
                _grid.Columns["FullName"].DisplayIndex = 1;
                _grid.Columns["FullName"].FillWeight = 38f;
                _grid.Columns["FullName"].MinimumWidth = 180;
            }

            if (_grid.Columns["FacultyCode"] != null)
            {
                _grid.Columns["FacultyCode"].HeaderText = "Number";
                _grid.Columns["FacultyCode"].DisplayIndex = 2;
                _grid.Columns["FacultyCode"].FillWeight = 20f;
                _grid.Columns["FacultyCode"].MinimumWidth = 120;
            }

            if (_grid.Columns["Email"] != null)
            {
                _grid.Columns["Email"].HeaderText = "Email";
                _grid.Columns["Email"].DisplayIndex = 3;
                _grid.Columns["Email"].FillWeight = 28f;
                _grid.Columns["Email"].MinimumWidth = 150;
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
                _editingFacultyId = 0;
                _txtFacultyCode.Text = string.Empty;
                _txtFirstName.Text = string.Empty;
                _txtLastName.Text = string.Empty;
                _txtMiddleName.Text = string.Empty;
                _txtEmail.Text = string.Empty;
                _txtPhone.Text = string.Empty;
                _txtAddress.Text = string.Empty;
                _currentPhotoBytes = null;
                _selectedPhotoBytes = null;
                ShowPhoto((byte[])null);
                _dtHireDate.Checked = false;
                SetEditorState(false);
                ResetProfile();
            }
        }

        private void ConfigureGridColumnsForListView()
        {
            var allowed = new[] { "FacultyId", "FullName", "FacultyCode", "Email" };

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

        private static void EnsureFacultyDisplayColumns(DataTable dt)
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

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (_grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;
            if (row.Cells["FacultyId"] == null || row.Cells["FacultyId"].Value == null) return;

            _editingFacultyId = Convert.ToInt32(row.Cells["FacultyId"].Value);
            _txtFacultyCode.Text = Convert.ToString(row.Cells["FacultyCode"].Value);
            _txtFirstName.Text = Convert.ToString(row.Cells["FirstName"].Value);
            _txtLastName.Text = Convert.ToString(row.Cells["LastName"].Value);
            _txtMiddleName.Text = Convert.ToString(row.Cells["MiddleName"].Value);
            _txtEmail.Text = Convert.ToString(row.Cells["Email"].Value);
            _txtPhone.Text = Convert.ToString(row.Cells["Phone"].Value);
            _txtAddress.Text = Convert.ToString(row.Cells["Address"].Value);
            _currentPhotoBytes = _facultyService?.GetPhotoData(_editingFacultyId);
            _selectedPhotoBytes = null;
            ShowPhoto(_currentPhotoBytes);

            var hire = row.Cells["HireDate"].Value;
            if (hire == null || hire == DBNull.Value)
            {
                _dtHireDate.Checked = false;
            }
            else
            {
                _dtHireDate.Checked = true;
                _dtHireDate.Value = Convert.ToDateTime(hire);
            }

            SetEditorState(false);
            LoadFacultyProfile();
        }

        private void LoadFacultyProfile()
        {
            if (_editingFacultyId <= 0)
            {
                ResetProfile();
                return;
            }

            var fullName = BuildPersonName(_txtFirstName.Text, _txtMiddleName.Text, _txtLastName.Text);
            var contactParts = Array.FindAll(
                new[]
                {
                    (_txtEmail.Text ?? string.Empty).Trim(),
                    (_txtPhone.Text ?? string.Empty).Trim()
                },
                delegate(string value) { return !string.IsNullOrWhiteSpace(value); });

            var contact = contactParts.Length == 0 ? "No contact information" : string.Join(" | ", contactParts);
            var hireDate = _dtHireDate.Checked ? _dtHireDate.Value.ToString("MMM dd, yyyy") : "Not set";

            try
            {
                var dt = _classScheduleService == null ? new DataTable() : _classScheduleService.GetByFaculty(_editingFacultyId);
                _lvAssignments.BeginUpdate();
                _lvAssignments.Items.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    var item = new ListViewItem(BuildSubjectLabel(row));
                    item.SubItems.Add(Convert.ToString(row["SectionName"]));
                    item.SubItems.Add(BuildScheduleText(row));
                    item.SubItems.Add(Convert.ToString(row["Room"]));
                    item.SubItems.Add(BuildTermText(row));
                    _lvAssignments.Items.Add(item);
                }

                var assignmentText = dt.Rows.Count == 1 ? "1 scheduled subject" : dt.Rows.Count + " scheduled subjects";
                ResetProfile(fullName, _txtFacultyCode.Text, "Active", contact, hireDate, assignmentText);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("FacultyControl.LoadFacultyProfile", ex);
                ResetProfile(fullName, _txtFacultyCode.Text, "Active", contact, hireDate, "Assignments unavailable");
            }
            finally
            {
                _lvAssignments.EndUpdate();
            }
        }

        private void ResetProfile()
        {
            ResetProfile(
                "Select a faculty record",
                "Not selected",
                "No profile",
                "No contact information",
                "Not set",
                "No subject assignments yet.");
        }

        private void ResetProfile(string name, string facultyNumber, string status, string contact, string hireDate, string assignments)
        {
            _lblProfileName.Text = string.IsNullOrWhiteSpace(name) ? "Faculty profile" : name;
            _lblProfileNumberValue.Text = string.IsNullOrWhiteSpace(facultyNumber) ? "Not assigned" : facultyNumber;
            _lblProfileStatusValue.Text = string.IsNullOrWhiteSpace(status) ? "Unknown" : status;
            _lblProfileContactValue.Text = string.IsNullOrWhiteSpace(contact) ? "No contact information" : contact;
            _lblProfileHireDateValue.Text = string.IsNullOrWhiteSpace(hireDate) ? "Not set" : hireDate;
            _lblProfileAssignmentsValue.Text = string.IsNullOrWhiteSpace(assignments) ? "No subject assignments yet." : assignments;

            if (_lvAssignments != null && string.Equals(status, "No profile", StringComparison.OrdinalIgnoreCase))
            {
                _lvAssignments.Items.Clear();
            }
        }

        private void Save()
        {
            if (_facultyService == null) return;

            try
            {
                var faculty = new Faculty
                {
                    FacultyId = _editingFacultyId,
                    FacultyCode = (_txtFacultyCode.Text ?? string.Empty).Trim(),
                    FirstName = (_txtFirstName.Text ?? string.Empty).Trim(),
                    LastName = (_txtLastName.Text ?? string.Empty).Trim(),
                    MiddleName = (_txtMiddleName.Text ?? string.Empty).Trim(),
                    Email = (_txtEmail.Text ?? string.Empty).Trim(),
                    Phone = (_txtPhone.Text ?? string.Empty).Trim(),
                    Address = (_txtAddress.Text ?? string.Empty).Trim(),
                    HireDate = _dtHireDate.Checked ? (DateTime?)_dtHireDate.Value.Date : null
                };

                var vr = _facultyService.Validate(faculty);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                faculty.PhotoData = _selectedPhotoBytes ?? _currentPhotoBytes;

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingFacultyId == 0)
                {
                    _facultyService.Create(faculty);
                    ThemedMessageBox.ShowInfo(this, "Faculty saved successfully.", "Saved");
                }
                else
                {
                    _facultyService.Update(faculty);
                    ThemedMessageBox.ShowInfo(this, "Faculty updated successfully.", "Updated");
                }

                LoadGrid();
                _currentPhotoBytes = faculty.PhotoData;
                _selectedPhotoBytes = null;
                ShowPhoto(_currentPhotoBytes);
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("FacultyControl.Save", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnSave.Enabled = _isEditorActive;
            }
        }

        private void UploadPhoto()
        {
            if (!_isEditorActive)
            {
                if (_editingFacultyId > 0)
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
                ofd.Title = "Select faculty photo";
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
            if (_facultyService == null) return;
            if (_editingFacultyId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Faculty") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _facultyService.Delete(_editingFacultyId);
                LoadGrid();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("FacultyControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void OpenProfileWindow()
        {
            if (_isEditorActive || _editingFacultyId <= 0)
            {
                return;
            }

            try
            {
                var faculty = new Faculty
                {
                    FacultyId = _editingFacultyId,
                    FacultyCode = _txtFacultyCode.Text,
                    FirstName = _txtFirstName.Text,
                    LastName = _txtLastName.Text,
                    MiddleName = _txtMiddleName.Text,
                    Email = _txtEmail.Text,
                    Phone = _txtPhone.Text,
                    Address = _txtAddress.Text,
                    HireDate = _dtHireDate.Checked ? (DateTime?)_dtHireDate.Value.Date : null,
                    IsActive = true
                };

                var photoData = _selectedPhotoBytes ?? _currentPhotoBytes;
                var assignments = _classScheduleService == null ? new DataTable() : _classScheduleService.GetByFaculty(_editingFacultyId);

                using (var profileForm = new FacultyProfileForm(faculty, photoData, assignments))
                {
                    profileForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("FacultyControl.OpenProfileWindow", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
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

        private static string BuildSubjectLabel(DataRow row)
        {
            if (row == null) return string.Empty;
            var code = Convert.ToString(row["SubjectCode"]);
            var name = Convert.ToString(row["SubjectName"]);
            return ((code ?? string.Empty) + " - " + (name ?? string.Empty)).Trim(new[] { ' ', '-' });
        }

        private static string BuildScheduleText(DataRow row)
        {
            if (row == null) return string.Empty;

            var day = Convert.ToString(row["DayOfWeek"]);
            string start = null;
            string end = null;

            try
            {
                var startTime = row["StartTime"] as TimeSpan?;
                if (startTime.HasValue)
                {
                    start = startTime.Value.ToString(@"hh\:mm");
                }

                var endTime = row["EndTime"] as TimeSpan?;
                if (endTime.HasValue)
                {
                    end = endTime.Value.ToString(@"hh\:mm");
                }
            }
            catch
            {
                // Ignore invalid time formatting.
            }

            var time = string.Empty;
            if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
            {
                time = start + "-" + end;
            }

            return string.Join(" | ", Array.FindAll(new[] { day, time }, delegate(string value)
            {
                return !string.IsNullOrWhiteSpace(value);
            }));
        }

        private static string BuildTermText(DataRow row)
        {
            if (row == null) return string.Empty;

            return string.Join(" | ", Array.FindAll(
                new[]
                {
                    Convert.ToString(row["Semester"]),
                    Convert.ToString(row["AcademicYear"])
                },
                delegate(string value) { return !string.IsNullOrWhiteSpace(value); }));
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
    }
}
