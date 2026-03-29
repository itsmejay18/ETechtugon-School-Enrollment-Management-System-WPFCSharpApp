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
    public sealed class DepartmentControl : BaseUserControl
    {
        private readonly DepartmentService _departmentService;

        private GroupBox _gb;
        private TextBox _txtCode;
        private TextBox _txtName;
        private TextBox _txtDescription;

        private TextBox _txtSearch;
        private DataGridView _grid;
        private SplitContainer _split;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnRefresh;

        private int _editingDepartmentId;
        private bool _isEditorActive;

        public DepartmentControl()
            : this(null)
        {
        }

        public DepartmentControl(DepartmentService departmentService)
        {
            _departmentService = departmentService;
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
                SplitterDistance = 700,
                BackColor = ThemeColors.Border
            };
            LockSplitEditorPanel(_split, 420);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.CellClick += (s, e) => PreviewSelected();
            _grid.SelectionChanged += (s, e) => PreviewSelected();

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            left.Controls.Add(_grid);
            _split.Panel1.Controls.Add(left);

            _gb = new GroupBox
            {
                Text = "Department Details",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(_gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            _txtCode = MakeTextBox();
            _txtName = MakeTextBox();
            _txtDescription = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            ThemeManager.StyleInput(_txtDescription);

            layout.Controls.Add(MakeLabel("Department Code *"), 0, 0);
            layout.Controls.Add(_txtCode, 1, 0);
            layout.Controls.Add(MakeLabel("Department Name *"), 0, 1);
            layout.Controls.Add(_txtName, 1, 1);
            layout.Controls.Add(MakeLabel("Description"), 0, 2);
            layout.Controls.Add(_txtDescription, 1, 2);

            var note = new Label
            {
                Text = "Click a row to preview. Click Edit to modify.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 4);
            layout.SetColumnSpan(note, 2);

            _gb.Controls.Add(layout);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            right.Controls.Add(_gb);
            _split.Panel2.Controls.Add(right);

            Controls.Add(_split);
            Controls.Add(toolbar);
        }

        private Panel BuildToolbar()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 0, 12) };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(12, 8, 12, 8) };
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
                Width = 58,
                Height = 32,
                ForeColor = ThemeColors.MutedText,
                Font = ThemeFonts.Label,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 4, 0)
            };

            _txtSearch = new TextBox { Width = 260, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 96, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadGrid();

            _btnAdd = new Button { Text = "Add", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnAdd);
            _btnAdd.Click += (s, e) => BeginAdd();

            _btnEdit = new Button { Text = "Edit", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonNeutral(_btnEdit);
            _btnEdit.Click += (s, e) => BeginEdit();

            _btnDelete = new Button { Text = "Delete", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DeleteCurrent();

            _btnSave = new Button { Text = "Save", Width = 86, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => Save();

            _btnCancel = new Button { Text = "Cancel", Width = 86 };
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

        private static TextBox MakeTextBox()
        {
            var tb = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill };
            ThemeManager.StyleInput(tb);
            return tb;
        }

        private void SetEditorState(bool active)
        {
            _isEditorActive = active;
            _txtCode.ReadOnly = !active;
            _txtName.ReadOnly = !active;
            _txtDescription.ReadOnly = !active;
            ApplyCrudButtonState(active, HasSelectedDataRow(_grid, "DepartmentId"), _btnAdd, _btnEdit, _btnDelete, _btnSave, _btnCancel);
        }

        private void BeginAdd()
        {
            _editingDepartmentId = 0;
            _txtCode.Text = string.Empty;
            _txtName.Text = string.Empty;
            _txtDescription.Text = string.Empty;
            SetEditorState(true);
            _txtCode.Focus();
        }

        private void BeginEdit()
        {
            if (_editingDepartmentId <= 0) return;
            SetEditorState(true);
            _txtCode.Focus();
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
                var dt = _departmentService == null ? new DataTable() : _departmentService.GetDepartments(_txtSearch == null ? string.Empty : _txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("DepartmentControl.LoadGrid", ex);
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

            if (_grid.Columns["DepartmentId"] != null)
            {
                _grid.Columns["DepartmentId"].Visible = false;
            }

            if (_grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                PreviewSelected();
            }
            else
            {
                _editingDepartmentId = 0;
                _txtCode.Text = string.Empty;
                _txtName.Text = string.Empty;
                _txtDescription.Text = string.Empty;
                SetEditorState(false);
            }
        }

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (!HasSelectedDataRow(_grid, "DepartmentId"))
            {
                _editingDepartmentId = 0;
                SetEditorState(false);
                return;
            }

            var row = _grid.SelectedRows[0];
            if (row.Cells["DepartmentId"] == null || row.Cells["DepartmentId"].Value == null) return;

            _editingDepartmentId = Convert.ToInt32(row.Cells["DepartmentId"].Value);
            _txtCode.Text = Convert.ToString(row.Cells["DepartmentCode"].Value);
            _txtName.Text = Convert.ToString(row.Cells["DepartmentName"].Value);
            _txtDescription.Text = Convert.ToString(row.Cells["Description"].Value);
            SetEditorState(false);
        }

        private void Save()
        {
            if (_departmentService == null) return;

            try
            {
                var department = new Department
                {
                    DepartmentId = _editingDepartmentId,
                    DepartmentCode = (_txtCode.Text ?? string.Empty).Trim(),
                    DepartmentName = (_txtName.Text ?? string.Empty).Trim(),
                    Description = (_txtDescription.Text ?? string.Empty).Trim()
                };

                var vr = _departmentService.Validate(department);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingDepartmentId == 0)
                {
                    _departmentService.Create(department);
                    ThemedMessageBox.ShowInfo(this, "Department saved successfully.", "Saved");
                }
                else
                {
                    _departmentService.Update(department);
                    ThemedMessageBox.ShowInfo(this, "Department updated successfully.", "Updated");
                }

                LoadGrid();
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("DepartmentControl.Save", ex);
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
            if (_departmentService == null) return;
            if (_editingDepartmentId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Department") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _departmentService.Delete(_editingDepartmentId);
                LoadGrid();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("DepartmentControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
