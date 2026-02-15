using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class UsersControl : BaseUserControl
    {
        private readonly UserManagementService _userService;

        private GroupBox _gb;
        private TextBox _txtUsername;
        private TextBox _txtDisplayName;
        private ComboBox _cmbRole;
        private CheckBox _chkActive;
        private TextBox _txtPassword;
        private Button _btnDelete;
        private Button _btnSave;
        private Button _btnCancel;

        private TextBox _txtSearch;
        private Button _btnRefresh;
        private DataGridView _grid;

        private int _editingUserId;

        public UsersControl(UserManagementService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            InitializeComponent();
            NewRecord();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _gb = new GroupBox
            {
                Text = "User Account",
                Dock = DockStyle.Top,
                Height = 240,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            var lblUsername = MakeLabel("Username *");
            _txtUsername = MakeTextBox();

            var lblRole = MakeLabel("Role *");
            _cmbRole = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbRole);
            _cmbRole.Items.AddRange(new object[] { AppConstants.Roles.Admin, AppConstants.Roles.Registrar, AppConstants.Roles.Faculty });
            if (_cmbRole.Items.Count > 0) _cmbRole.SelectedIndex = 0;

            var lblDisplayName = MakeLabel("Display Name");
            _txtDisplayName = MakeTextBox();

            var lblActive = MakeLabel("Active");
            _chkActive = new CheckBox { Dock = DockStyle.Left, Text = "Enabled", AutoSize = true };

            var lblPassword = MakeLabel("Password");
            _txtPassword = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input, UseSystemPasswordChar = true };
            ThemeManager.StyleInput(_txtPassword);

            var hint = new Label
            {
                Text = "Tip: For existing users, leave Password blank to keep current password.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Row 0
            layout.Controls.Add(lblUsername, 0, 0);
            layout.Controls.Add(_txtUsername, 1, 0);
            layout.Controls.Add(lblRole, 2, 0);
            layout.Controls.Add(_cmbRole, 3, 0);

            // Row 1
            layout.Controls.Add(lblDisplayName, 0, 1);
            layout.Controls.Add(_txtDisplayName, 1, 1);
            layout.SetColumnSpan(_txtDisplayName, 3);

            // Row 2
            layout.Controls.Add(lblActive, 0, 2);
            layout.Controls.Add(_chkActive, 1, 2);
            layout.Controls.Add(lblPassword, 2, 2);
            layout.Controls.Add(_txtPassword, 3, 2);

            // Row 3
            layout.Controls.Add(hint, 0, 3);
            layout.SetColumnSpan(hint, 4);

            var buttons = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8, 8, 8, 8), BackColor = ThemeColors.CardBackground };
            _btnDelete = new Button { Text = "Disable", Width = 110, Dock = DockStyle.Left };
            ThemeManager.StyleButtonDanger(_btnDelete);
            _btnDelete.Click += (s, e) => DisableCurrent();

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
            _grid.CellFormatting += GridCellFormatting;

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
            _editingUserId = 0;
            _txtUsername.Text = string.Empty;
            _txtDisplayName.Text = string.Empty;
            if (_cmbRole.Items.Count > 0) _cmbRole.SelectedIndex = 0;
            _chkActive.Checked = true;
            _txtPassword.Text = string.Empty;

            _btnDelete.Enabled = false;
            _btnSave.Text = "Save";
        }

        private void LoadGrid()
        {
            try
            {
                UseWaitCursor = true;
                var dt = _userService.GetUsers(_txtSearch.Text);
                BindGrid(dt);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("UsersControl.LoadGrid", ex);
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

            if (_grid.Columns["UserId"] != null) _grid.Columns["UserId"].Visible = false;

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

            var toggleCol = new DataGridViewButtonColumn
            {
                Name = "ToggleActiveAction",
                HeaderText = "",
                Text = "",
                UseColumnTextForButtonValue = false,
                Width = 90
            };

            _grid.Columns.Insert(0, editCol);
            _grid.Columns.Insert(1, toggleCol);
        }

        private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var col = _grid.Columns[e.ColumnIndex];
            if (col == null) return;

            if (!string.Equals(col.Name, "ToggleActiveAction", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var row = _grid.Rows[e.RowIndex];
            var activeObj = row.Cells["IsActive"].Value;
            var isActive = activeObj != null && activeObj != DBNull.Value && Convert.ToBoolean(activeObj);

            e.Value = isActive ? "Disable" : "Enable";
            e.FormattingApplied = true;
        }

        private void GridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var col = _grid.Columns[e.ColumnIndex];
            if (col == null) return;

            var row = _grid.Rows[e.RowIndex];
            var idObj = row.Cells["UserId"].Value;
            if (idObj == null) return;
            var userId = Convert.ToInt32(idObj);

            if (string.Equals(col.Name, "EditAction", StringComparison.OrdinalIgnoreCase))
            {
                _editingUserId = userId;
                _txtUsername.Text = Convert.ToString(row.Cells["Username"].Value);
                _txtDisplayName.Text = Convert.ToString(row.Cells["DisplayName"].Value);
                _cmbRole.SelectedItem = Convert.ToString(row.Cells["Role"].Value);
                _chkActive.Checked = Convert.ToBoolean(row.Cells["IsActive"].Value);
                _txtPassword.Text = string.Empty;

                _btnDelete.Enabled = true;
                _btnSave.Text = "Update";
            }
            else if (string.Equals(col.Name, "ToggleActiveAction", StringComparison.OrdinalIgnoreCase))
            {
                var activeObj = row.Cells["IsActive"].Value;
                var isActive = activeObj != null && activeObj != DBNull.Value && Convert.ToBoolean(activeObj);
                var newActive = !isActive;

                var prompt = newActive ? "Enable this user?" : "Disable this user?";
                if (ThemedMessageBox.ShowConfirm(this, prompt, "Confirm") != DialogResult.OK)
                {
                    return;
                }

                _userService.SetActive(userId, newActive);
                LoadGrid();
                if (_editingUserId == userId)
                {
                    NewRecord();
                }
            }
        }

        private void Save()
        {
            try
            {
                var username = (_txtUsername.Text ?? string.Empty).Trim();
                var password = _txtPassword.Text ?? string.Empty;
                var role = Convert.ToString(_cmbRole.SelectedItem) ?? string.Empty;
                var displayName = (_txtDisplayName.Text ?? string.Empty).Trim();
                var isActive = _chkActive.Checked;

                if (_editingUserId == 0)
                {
                    var vr = _userService.ValidateNewUser(username, password, role);
                    if (!vr.IsValid)
                    {
                        ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                        return;
                    }

                    UseWaitCursor = true;
                    _btnSave.Enabled = false;
                    _userService.Create(username, password, role, displayName, isActive);
                    ThemedMessageBox.ShowInfo(this, "User created successfully.", "Saved");
                }
                else
                {
                    var vr = _userService.ValidateUpdateUser(_editingUserId, username, role);
                    if (!vr.IsValid)
                    {
                        ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                        return;
                    }

                    UseWaitCursor = true;
                    _btnSave.Enabled = false;
                    _userService.Update(_editingUserId, username, role, displayName, isActive);

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        _userService.ResetPassword(_editingUserId, password);
                    }

                    ThemedMessageBox.ShowInfo(this, "User updated successfully.", "Updated");
                }

                LoadGrid();
                NewRecord();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("UsersControl.Save", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnSave.Enabled = true;
            }
        }

        private void DisableCurrent()
        {
            if (_editingUserId == 0) return;

            if (ThemedMessageBox.ShowConfirm(this, "Disable this user?", "Confirm") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _userService.SetActive(_editingUserId, false);
                LoadGrid();
                NewRecord();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("UsersControl.DisableCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}

