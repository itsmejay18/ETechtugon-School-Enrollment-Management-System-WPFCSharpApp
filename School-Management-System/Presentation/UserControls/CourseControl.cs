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
    public sealed class CourseControl : BaseUserControl
    {
        private readonly CourseService _courseService;

        private GroupBox _gb;
        private TextBox _txtCode;
        private TextBox _txtName;
        private TextBox _txtDescription;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;

        private TextBox _txtSearch;
        private Button _btnRefresh;
        private DataGridView _grid;

        private int _editingCourseId;

        public CourseControl(CourseService courseService)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            InitializeComponent();
            NewRecord();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _gb = new GroupBox
            {
                Text = "Course Information",
                Dock = DockStyle.Top,
                Height = 220,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            var lblCode = MakeLabel("Course Code *");
            _txtCode = MakeTextBox();
            var lblName = MakeLabel("Course Name *");
            _txtName = MakeTextBox();

            var lblDesc = MakeLabel("Description");
            _txtDescription = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            ThemeManager.StyleInput(_txtDescription);

            layout.Controls.Add(lblCode, 0, 0);
            layout.Controls.Add(_txtCode, 1, 0);
            layout.Controls.Add(lblName, 2, 0);
            layout.Controls.Add(_txtName, 3, 0);

            layout.Controls.Add(lblDesc, 0, 1);
            layout.Controls.Add(_txtDescription, 1, 1);
            layout.SetColumnSpan(_txtDescription, 3);

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

            _gb.Controls.Add(layout);
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

        private static TextBox MakeTextBox()
        {
            var tb = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill };
            ThemeManager.StyleInput(tb);
            return tb;
        }

        private void NewRecord()
        {
            _editingCourseId = 0;
            _txtCode.Text = string.Empty;
            _txtName.Text = string.Empty;
            _txtDescription.Text = string.Empty;

            _btnDelete.Enabled = false;
            _btnSave.Text = "Save";
        }

        private void LoadGrid()
        {
            try
            {
                UseWaitCursor = true;
                var dt = _courseService.GetCourses(_txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("CourseControl.LoadGrid", ex);
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

            if (_grid.Columns["CourseId"] != null)
            {
                _grid.Columns["CourseId"].Visible = false;
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
            var idObj = row.Cells["CourseId"].Value;
            if (idObj == null) return;
            var courseId = Convert.ToInt32(idObj);

            if (string.Equals(col.Name, "EditAction", StringComparison.OrdinalIgnoreCase))
            {
                _editingCourseId = courseId;
                _txtCode.Text = Convert.ToString(row.Cells["CourseCode"].Value);
                _txtName.Text = Convert.ToString(row.Cells["CourseName"].Value);
                _txtDescription.Text = Convert.ToString(row.Cells["Description"].Value);

                _btnDelete.Enabled = true;
                _btnSave.Text = "Update";
            }
            else if (string.Equals(col.Name, "DeleteAction", StringComparison.OrdinalIgnoreCase))
            {
                if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Course") == DialogResult.OK)
                {
                    _courseService.Delete(courseId);
                    LoadGrid();
                    NewRecord();
                }
            }
        }

        private void Save()
        {
            try
            {
                var course = new Course
                {
                    CourseId = _editingCourseId,
                    CourseCode = (_txtCode.Text ?? string.Empty).Trim(),
                    CourseName = (_txtName.Text ?? string.Empty).Trim(),
                    Description = (_txtDescription.Text ?? string.Empty).Trim()
                };

                var vr = _courseService.Validate(course);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingCourseId == 0)
                {
                    _courseService.Create(course);
                    ThemedMessageBox.ShowInfo(this, "Course saved successfully.", "Saved");
                }
                else
                {
                    _courseService.Update(course);
                    ThemedMessageBox.ShowInfo(this, "Course updated successfully.", "Updated");
                }

                LoadGrid();
                NewRecord();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("CourseControl.Save", ex);
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
            if (_editingCourseId == 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Course") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _courseService.Delete(_editingCourseId);
                LoadGrid();
                NewRecord();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("CourseControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}

