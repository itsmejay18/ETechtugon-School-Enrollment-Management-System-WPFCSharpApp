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

        private GroupBox _gb;
        private TextBox _txtFacultyCode;
        private TextBox _txtFirstName;
        private TextBox _txtLastName;
        private TextBox _txtMiddleName;
        private TextBox _txtEmail;
        private TextBox _txtPhone;
        private TextBox _txtAddress;
        private DateTimePicker _dtHireDate;

        private TextBox _txtSearch;
        private DataGridView _grid;
        private SplitContainer _split;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnRefresh;

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
            this.Size = new System.Drawing.Size(852, 496);
            this.ResumeLayout(false);

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

            if (_grid.Columns["FacultyId"] != null)
            {
                _grid.Columns["FacultyId"].Visible = false;
            }

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
