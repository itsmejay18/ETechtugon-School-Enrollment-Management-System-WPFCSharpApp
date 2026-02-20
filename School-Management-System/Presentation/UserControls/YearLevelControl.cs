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
    public sealed class YearLevelControl : BaseUserControl
    {
        private readonly YearLevelService _yearLevelService;

        private GroupBox _gb;
        private TextBox _txtName;
        private NumericUpDown _numSort;

        private TextBox _txtSearch;
        private DataGridView _grid;
        private SplitContainer _split;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnRefresh;

        private int _editingYearLevelId;
        private bool _isEditorActive;

        public YearLevelControl()
            : this(null)
        {
        }

        public YearLevelControl(YearLevelService yearLevelService)
        {
            _yearLevelService = yearLevelService;
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
            LockSplitEditorPanel(_split, 390);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            ThemeManager.StyleDataGrid(_grid);
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.CellClick += (s, e) => PreviewSelected();

            var left = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            left.Controls.Add(_grid);
            _split.Panel1.Controls.Add(left);

            _gb = new GroupBox
            {
                Text = "Year Level Details",
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
                RowCount = 5,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            _txtName = MakeTextBox();
            _numSort = new NumericUpDown { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Minimum = 0, Maximum = 20, Value = 1 };
            ThemeManager.StyleInput(_numSort);

            layout.Controls.Add(MakeLabel("Year Level *"), 0, 0);
            layout.Controls.Add(_txtName, 1, 0);
            layout.Controls.Add(MakeLabel("Display Order"), 0, 1);
            layout.Controls.Add(_numSort, 1, 1);

            var note = new Label
            {
                Text = "Click a row to preview. Click Edit to modify.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 3);
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

        private static TextBox MakeTextBox()
        {
            var tb = new TextBox { Font = ThemeFonts.Input, Dock = DockStyle.Fill };
            ThemeManager.StyleInput(tb);
            return tb;
        }

        private void SetEditorState(bool active)
        {
            _isEditorActive = active;
            _txtName.ReadOnly = !active;
            _numSort.Enabled = active;

            _btnSave.Enabled = active;
            _btnCancel.Enabled = active;
            _btnAdd.Enabled = !active;
            _btnEdit.Enabled = !active && _editingYearLevelId > 0;
            _btnDelete.Enabled = !active && _editingYearLevelId > 0;
        }

        private void BeginAdd()
        {
            _editingYearLevelId = 0;
            _txtName.Text = string.Empty;
            _numSort.Value = 1;
            SetEditorState(true);
            _txtName.Focus();
        }

        private void BeginEdit()
        {
            if (_editingYearLevelId <= 0) return;
            SetEditorState(true);
            _txtName.Focus();
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
                var dt = _yearLevelService == null ? new DataTable() : _yearLevelService.GetYearLevels(_txtSearch == null ? string.Empty : _txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("YearLevelControl.LoadGrid", ex);
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

            if (_grid.Columns["YearLevelId"] != null) _grid.Columns["YearLevelId"].Visible = false;

            if (_grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                PreviewSelected();
            }
            else
            {
                _editingYearLevelId = 0;
                _txtName.Text = string.Empty;
                _numSort.Value = 1;
                SetEditorState(false);
            }
        }

        private void PreviewSelected()
        {
            if (_isEditorActive) return;
            if (_grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;
            if (row.Cells["YearLevelId"] == null || row.Cells["YearLevelId"].Value == null) return;

            _editingYearLevelId = Convert.ToInt32(row.Cells["YearLevelId"].Value);
            _txtName.Text = Convert.ToString(row.Cells["Name"].Value);

            decimal sortOrder;
            if (decimal.TryParse(Convert.ToString(row.Cells["SortOrder"].Value), out sortOrder))
            {
                _numSort.Value = sortOrder;
            }
            SetEditorState(false);
        }

        private void Save()
        {
            if (_yearLevelService == null) return;

            try
            {
                var yearLevel = new YearLevel
                {
                    YearLevelId = _editingYearLevelId,
                    Name = (_txtName.Text ?? string.Empty).Trim(),
                    SortOrder = Convert.ToInt32(_numSort.Value)
                };

                var vr = _yearLevelService.Validate(yearLevel);
                if (!vr.IsValid)
                {
                    ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                    return;
                }

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (_editingYearLevelId == 0)
                {
                    _yearLevelService.Create(yearLevel);
                    ThemedMessageBox.ShowInfo(this, "Year Level saved successfully.", "Saved");
                }
                else
                {
                    _yearLevelService.Update(yearLevel);
                    ThemedMessageBox.ShowInfo(this, "Year Level updated successfully.", "Updated");
                }

                LoadGrid();
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("YearLevelControl.Save", ex);
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
            if (_yearLevelService == null) return;
            if (_editingYearLevelId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, Messages.ConfirmDelete, "Delete Year Level") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _yearLevelService.Delete(_editingYearLevelId);
                LoadGrid();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("YearLevelControl.DeleteCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
