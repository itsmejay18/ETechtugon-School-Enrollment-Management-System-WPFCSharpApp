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
    public sealed class StudentControl : BaseUserControl
    {
        private readonly StudentService _studentService;

        private GroupBox _gb;
        private TextBox _txtStudentNumber;
        private TextBox _txtFirstName;
        private TextBox _txtLastName;
        private TextBox _txtMiddleName;
        private ComboBox _cmbGender;
        private DateTimePicker _dtBirthDate;
        private TextBox _txtEmail;
        private TextBox _txtPhone;
        private TextBox _txtAddress;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;

        private TextBox _txtSearch;
        private Button _btnRefresh;
        private DataGridView _grid;

        private int _editingStudentId;

        public StudentControl(StudentService studentService)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            InitializeComponent();
            NewRecord();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _gb = new GroupBox
            {
                Text = "Student Information",
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

            var lblStudentNumber = MakeLabel("Student No.");
            _txtStudentNumber = MakeTextBox(readOnly: true);

            var lblFirstName = MakeLabel("First Name *");
            _txtFirstName = MakeTextBox();

            var lblLastName = MakeLabel("Last Name *");
            _txtLastName = MakeTextBox();

            var lblMiddleName = MakeLabel("Middle Name");
            _txtMiddleName = MakeTextBox();

            var lblGender = MakeLabel("Gender");
            _cmbGender = new ComboBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill };
            ThemeManager.StyleComboBox(_cmbGender);
            _cmbGender.Items.AddRange(new object[] { "", "Male", "Female", "Other" });

            var lblBirthDate = MakeLabel("Birth Date");
            _dtBirthDate = new DateTimePicker { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            var lblEmail = MakeLabel("Email");
            _txtEmail = MakeTextBox();

            var lblPhone = MakeLabel("Phone");
            _txtPhone = MakeTextBox();

            var lblAddress = MakeLabel("Address");
            _txtAddress = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
            ThemeManager.StyleInput(_txtAddress);

            // Row 0
            formLayout.Controls.Add(lblStudentNumber, 0, 0);
            formLayout.Controls.Add(_txtStudentNumber, 1, 0);
            formLayout.Controls.Add(lblGender, 2, 0);
            formLayout.Controls.Add(_cmbGender, 3, 0);

            // Row 1
            formLayout.Controls.Add(lblFirstName, 0, 1);
            formLayout.Controls.Add(_txtFirstName, 1, 1);
            formLayout.Controls.Add(lblLastName, 2, 1);
            formLayout.Controls.Add(_txtLastName, 3, 1);

            // Row 2
            formLayout.Controls.Add(lblMiddleName, 0, 2);
            formLayout.Controls.Add(_txtMiddleName, 1, 2);
            formLayout.Controls.Add(lblBirthDate, 2, 2);
            formLayout.Controls.Add(_dtBirthDate, 3, 2);

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
            _editingStudentId = 0;
            _txtStudentNumber.Text = _studentService.GetNextStudentNumber();
            _txtFirstName.Text = string.Empty;
            _txtLastName.Text = string.Empty;
            _txtMiddleName.Text = string.Empty;
            _cmbGender.SelectedIndex = 0;
            _dtBirthDate.Checked = false;
            _txtEmail.Text = string.Empty;
            _txtPhone.Text = string.Empty;
            _txtAddress.Text = string.Empty;

            _btnDelete.Enabled = false;
            _btnSave.Text = "Save";
        }

        private void LoadGrid()
        {
            try
            {
                UseWaitCursor = true;
                var dt = _studentService.GetStudents(_txtSearch.Text);
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
            _grid.Columns.Clear();
            _grid.DataSource = null;

            _grid.DataSource = dt;

            // Hide internal id.
            if (_grid.Columns["StudentId"] != null)
            {
                _grid.Columns["StudentId"].Visible = false;
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
            var idObj = row.Cells["StudentId"].Value;
            if (idObj == null) return;

            var studentId = Convert.ToInt32(idObj);

            if (string.Equals(col.Name, "EditAction", StringComparison.OrdinalIgnoreCase))
            {
                LoadFromRow(row);
                _editingStudentId = studentId;
                _btnDelete.Enabled = true;
                _btnSave.Text = "Update";
            }
            else if (string.Equals(col.Name, "DeleteAction", StringComparison.OrdinalIgnoreCase))
            {
                if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Student") == DialogResult.OK)
                {
                    _studentService.Delete(studentId);
                    LoadGrid();
                    NewRecord();
                }
            }
        }

        private void LoadFromRow(DataGridViewRow row)
        {
            _txtStudentNumber.Text = Convert.ToString(row.Cells["StudentNumber"].Value);
            _txtFirstName.Text = Convert.ToString(row.Cells["FirstName"].Value);
            _txtLastName.Text = Convert.ToString(row.Cells["LastName"].Value);
            _txtMiddleName.Text = Convert.ToString(row.Cells["MiddleName"].Value);
            _cmbGender.SelectedItem = Convert.ToString(row.Cells["Gender"].Value);

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
        }

        private void Save()
        {
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
                NewRecord();
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

        private void DeleteCurrent()
        {
            if (_editingStudentId == 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Student") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _studentService.Delete(_editingStudentId);
                LoadGrid();
                NewRecord();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("StudentControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
