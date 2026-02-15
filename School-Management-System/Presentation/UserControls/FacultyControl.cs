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
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;

        private TextBox _txtSearch;
        private Button _btnRefresh;
        private DataGridView _grid;

        private int _editingFacultyId;

        public FacultyControl(FacultyService facultyService)
        {
            _facultyService = facultyService ?? throw new ArgumentNullException(nameof(facultyService));
            InitializeComponent();
            NewRecord();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _gb = new GroupBox
            {
                Text = "Faculty Information",
                Dock = DockStyle.Top,
                Height = 280,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(8, 6, 8, 6)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (var i = 0; i < 5; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            }

            var lblCode = MakeLabel("Faculty Code");
            _txtFacultyCode = MakeTextBox(readOnly: true);

            var lblFirstName = MakeLabel("First Name *");
            _txtFirstName = MakeTextBox();

            var lblLastName = MakeLabel("Last Name *");
            _txtLastName = MakeTextBox();

            var lblMiddleName = MakeLabel("Middle Name");
            _txtMiddleName = MakeTextBox();

            var lblHireDate = MakeLabel("Hire Date");
            _dtHireDate = new DateTimePicker { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            var lblEmail = MakeLabel("Email");
            _txtEmail = MakeTextBox();

            var lblPhone = MakeLabel("Phone");
            _txtPhone = MakeTextBox();

            var lblAddress = MakeLabel("Address");
            _txtAddress = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
            ThemeManager.StyleInput(_txtAddress);

            // Row 0
            formLayout.Controls.Add(lblCode, 0, 0);
            formLayout.Controls.Add(_txtFacultyCode, 1, 0);
            formLayout.Controls.Add(lblHireDate, 2, 0);
            formLayout.Controls.Add(_dtHireDate, 3, 0);

            // Row 1
            formLayout.Controls.Add(lblFirstName, 0, 1);
            formLayout.Controls.Add(_txtFirstName, 1, 1);
            formLayout.Controls.Add(lblLastName, 2, 1);
            formLayout.Controls.Add(_txtLastName, 3, 1);

            // Row 2
            formLayout.Controls.Add(lblMiddleName, 0, 2);
            formLayout.Controls.Add(_txtMiddleName, 1, 2);
            formLayout.SetColumnSpan(_txtMiddleName, 3);

            // Row 3
            formLayout.Controls.Add(lblEmail, 0, 3);
            formLayout.Controls.Add(_txtEmail, 1, 3);
            formLayout.Controls.Add(lblPhone, 2, 3);
            formLayout.Controls.Add(_txtPhone, 3, 3);

            // Row 4 (address spans)
            formLayout.Controls.Add(lblAddress, 0, 4);
            formLayout.Controls.Add(_txtAddress, 1, 4);
            formLayout.SetColumnSpan(_txtAddress, 3);

            var buttons = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8, 8, 8, 8), BackColor = ThemeColors.CardBackground };
            _btnDelete = new Button { Text = "Delete", Width = 110, Dock = DockStyle.Left };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            var right = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = 280 };
            _btnSave = new Button { Text = "Save", Width = 110 };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 110 };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => NewRecord();

            right.Controls.Add(_btnSave);
            right.Controls.Add(_btnCancel);

            buttons.Controls.Add(right);
            buttons.Controls.Add(_btnDelete);

            _gb.Controls.Add(formLayout);
            _gb.Controls.Add(buttons);

            var searchRow = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 12, 0, 8), BackColor = ThemeColors.Background };
            var lblSearch = new Label { Text = "Search:", AutoSize = true, Location = new Point(0, 16), Font = ThemeFonts.Label, ForeColor = ThemeColors.Text };
            _txtSearch = new TextBox { Location = new Point(62, 12), Width = 280 };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Location = new Point(350, 10), Width = 110 };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            searchRow.Controls.Add(lblSearch);
            searchRow.Controls.Add(_txtSearch);
            searchRow.Controls.Add(_btnRefresh);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
            _grid.CellContentClick += GridCellContentClick;

            Controls.Add(_grid);
            Controls.Add(searchRow);
            Controls.Add(_gb);
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

        private void NewRecord()
        {
            _editingFacultyId = 0;
            _txtFacultyCode.Text = _facultyService.GetNextFacultyCode();
            _txtFirstName.Text = string.Empty;
            _txtLastName.Text = string.Empty;
            _txtMiddleName.Text = string.Empty;
            _txtEmail.Text = string.Empty;
            _txtPhone.Text = string.Empty;
            _txtAddress.Text = string.Empty;
            _dtHireDate.Checked = false;

            _btnDelete.Enabled = false;
            _btnSave.Text = "Save";
        }

        private void LoadGrid()
        {
            try
            {
                UseWaitCursor = true;
                var dt = _facultyService.GetFaculty(_txtSearch.Text);
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
            _grid.Columns.Clear();
            _grid.DataSource = null;
            _grid.DataSource = dt;

            if (_grid.Columns["FacultyId"] != null)
            {
                _grid.Columns["FacultyId"].Visible = false;
            }

            AddActionButtons();
        }

        private void AddActionButtons()
        {
            var editCol = new DataGridViewButtonColumn
            {
                Name = "EditAction",
                HeaderText = "",
                Text = "Edit",
                UseColumnTextForButtonValue = true,
                Width = 70
            };

            var delCol = new DataGridViewButtonColumn
            {
                Name = "DeleteAction",
                HeaderText = "",
                Text = "Delete",
                UseColumnTextForButtonValue = true,
                Width = 70
            };

            _grid.Columns.Insert(0, editCol);
            _grid.Columns.Insert(1, delCol);
        }

        private void GridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var col = _grid.Columns[e.ColumnIndex];
            if (col == null) return;

            var row = _grid.Rows[e.RowIndex];
            var idObj = row.Cells["FacultyId"].Value;
            if (idObj == null) return;

            var facultyId = Convert.ToInt32(idObj);

            if (string.Equals(col.Name, "EditAction", StringComparison.OrdinalIgnoreCase))
            {
                _editingFacultyId = facultyId;
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

                _btnDelete.Enabled = true;
                _btnSave.Text = "Update";
            }
            else if (string.Equals(col.Name, "DeleteAction", StringComparison.OrdinalIgnoreCase))
            {
                if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Faculty") == DialogResult.OK)
                {
                    _facultyService.Delete(facultyId);
                    LoadGrid();
                    NewRecord();
                }
            }
        }

        private void Save()
        {
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
                NewRecord();
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
            if (_editingFacultyId == 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Faculty") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _facultyService.Delete(_editingFacultyId);
                LoadGrid();
                NewRecord();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("FacultyControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}

