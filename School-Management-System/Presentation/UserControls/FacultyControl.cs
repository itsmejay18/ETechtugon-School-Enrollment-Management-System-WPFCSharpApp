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
    public sealed class FacultyControl : BaseUserControl
    {
        private readonly FacultyService _facultyService;

        private GroupBox _gb = new GroupBox();
        private TextBox _txtFacultyCode = new TextBox();
        private TextBox _txtFirstName = new TextBox();
        private TextBox _txtLastName = new TextBox();
        private TextBox _txtMiddleName = new TextBox();
        private TextBox _txtEmail = new TextBox();
        private TextBox _txtPhone = new TextBox();
        private TextBox _txtAddress = new TextBox();
        private DateTimePicker _dtHireDate = new DateTimePicker();

        private TextBox _txtSearch = new TextBox();
        private DataGridView _grid = new DataGridView();
        private SplitContainer _split = new SplitContainer();

        private Button _btnAdd = new Button();
        private Button _btnEdit = new Button();
        private Button _btnDelete = new Button();
        private Button _btnSave = new Button();
        private Button _btnCancel = new Button();
        private Button _btnRefresh = new Button();

        private int _editingFacultyId;
        private bool _isEditorActive;

        public FacultyControl()
            : this(null)
        {
        }

        public FacultyControl(FacultyService facultyService)
        {
            _facultyService = facultyService;
            InitializeComponent();
            SetEditorState(false);
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FacultyControl
            // 
            this.Name = "FacultyControl";
            this.Size = new System.Drawing.Size(852, 179);
            this.ResumeLayout(false);

            var toolbar = BuildToolbar();
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                SplitterDistance = 730,
                BackColor = ThemeColors.Border
            };
            LockSplitEditorPanel(_split, 420);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.CellClick += (s, e) => PreviewSelected();

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            left.Controls.Add(_grid);
            _split.Panel1.Controls.Add(left);

            _gb = new GroupBox
            {
                Text = "Faculty Details",
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

            _txtFacultyCode = MakeTextBox(readOnly: true);
            _txtFirstName = MakeTextBox();
            _txtLastName = MakeTextBox();
            _txtMiddleName = MakeTextBox();
            _txtEmail = MakeTextBox();
            _txtPhone = MakeTextBox();
            _dtHireDate = new DateTimePicker { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
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

            _gb.Controls.Add(formLayout);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            right.Controls.Add(_gb);
            _split.Panel2.Controls.Add(right);

            Controls.Clear();
            Controls.Add(_split);
            Controls.Add(toolbar);
        }

        private Panel BuildToolbar()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = ThemeColors.CardBackground, Padding = new Padding(10, 8, 10, 8) };

            _txtSearch = new TextBox { Width = 260, Location = new Point(0, 10) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 96, Location = new Point(270, 8) };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            _btnAdd = new Button { Text = "Add", Width = 86, Location = new Point(384, 8) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 86, Location = new Point(478, 8) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnDelete = new Button { Text = "Delete", Width = 86, Location = new Point(572, 8) };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            _btnSave = new Button { Text = "Save", Width = 86, Location = new Point(666, 8) };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 86, Location = new Point(760, 8) };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => CancelEdit();

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
            // If controls are not initialized yet (designer/constructor), exit safely.
            if (_txtFirstName == null) return;

            _txtFirstName.ReadOnly = !active;
            _txtLastName.ReadOnly = !active;
            _txtMiddleName.ReadOnly = !active;
            _txtEmail.ReadOnly = !active;
            _txtPhone.ReadOnly = !active;
            _txtAddress.ReadOnly = !active;
            _dtHireDate.Enabled = active;

            _btnSave.Enabled = active;
            _btnCancel.Enabled = active;
            _btnAdd.Enabled = !active;
            _btnEdit.Enabled = !active && _editingFacultyId > 0;
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
            _dtHireDate.Checked = false;
            SetEditorState(true);
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
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = dt;

            ConfigureGridColumnsForListView();

            if (_grid.Columns["FacultyId"] != null)
            {
                _grid.Columns["FacultyId"].HeaderText = "ID";
                _grid.Columns["FacultyId"].DisplayIndex = 0;
                _grid.Columns["FacultyId"].FillWeight = 16f;
                _grid.Columns["FacultyId"].MinimumWidth = 56;
            }

            if (_grid.Columns["FirstName"] != null)
            {
                _grid.Columns["FirstName"].HeaderText = "First Name";
                _grid.Columns["FirstName"].DisplayIndex = 1;
                _grid.Columns["FirstName"].FillWeight = 28f;
                _grid.Columns["FirstName"].MinimumWidth = 110;
            }

            if (_grid.Columns["LastName"] != null)
            {
                _grid.Columns["LastName"].HeaderText = "Last Name";
                _grid.Columns["LastName"].DisplayIndex = 2;
                _grid.Columns["LastName"].FillWeight = 28f;
                _grid.Columns["LastName"].MinimumWidth = 110;
            }

            if (_grid.Columns["FacultyCode"] != null)
            {
                _grid.Columns["FacultyCode"].HeaderText = "Number";
                _grid.Columns["FacultyCode"].DisplayIndex = 3;
                _grid.Columns["FacultyCode"].FillWeight = 28f;
                _grid.Columns["FacultyCode"].MinimumWidth = 130;
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
                _dtHireDate.Checked = false;
                SetEditorState(false);
            }
        }

        private void ConfigureGridColumnsForListView()
        {
            var allowed = new[] { "FacultyId", "FirstName", "LastName", "FacultyCode" };

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
                _btnSave.Enabled = true;
            }
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
    }
}

