using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Helpers;
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

        private int _editingUserId;
        private bool _isEditorActive;
        private byte[] _currentPhotoBytes;
        private byte[] _selectedPhotoBytes;

        public UsersControl()
            : this(null)
        {
        }

        public UsersControl(UserManagementService userService)
        {
            _userService = userService;
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
            LockSplitEditorPanel(_split, 560);

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
                Text = "User Account Details",
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
                RowCount = 8,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            _txtUsername = MakeTextBox();
            _txtDisplayName = MakeTextBox();
            _cmbRole = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbRole);
            _cmbRole.Items.AddRange(new object[] { AppConstants.Roles.Admin, AppConstants.Roles.Registrar, AppConstants.Roles.Faculty });
            if (_cmbRole.Items.Count > 0) _cmbRole.SelectedIndex = 0;

            _chkActive = new CheckBox { Dock = DockStyle.Left, Text = "Enabled", AutoSize = true };
            _txtPassword = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input, UseSystemPasswordChar = true };
            ThemeManager.StyleInput(_txtPassword);

            layout.Controls.Add(MakeLabel("Username *"), 0, 0);
            layout.Controls.Add(_txtUsername, 1, 0);
            layout.Controls.Add(MakeLabel("Display Name"), 0, 1);
            layout.Controls.Add(_txtDisplayName, 1, 1);
            layout.Controls.Add(MakeLabel("Role *"), 0, 2);
            layout.Controls.Add(_cmbRole, 1, 2);
            layout.Controls.Add(MakeLabel("Active"), 0, 3);
            layout.Controls.Add(_chkActive, 1, 3);
            layout.Controls.Add(MakeLabel("Password"), 0, 4);
            layout.Controls.Add(_txtPassword, 1, 4);

            var hint = new Label
            {
                Text = "For existing users, leave password blank to keep current password.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(hint, 0, 6);
            layout.SetColumnSpan(hint, 2);

            var note = new Label
            {
                Text = "Click a row to preview. Click Edit to modify.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 7);
            layout.SetColumnSpan(note, 2);

            var photoPanel = BuildPhotoPanel();
            var detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            detailsLayout.Controls.Add(layout, 0, 0);
            detailsLayout.Controls.Add(photoPanel, 1, 0);
            _gb.Controls.Add(detailsLayout);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10) };
            right.Controls.Add(_gb);
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

            _btnUploadPhoto = new Button { Text = "Upload Photo", Dock = DockStyle.Top, Height = 38 };
            ThemeManager.StyleButtonNeutral(_btnUploadPhoto);
            _btnUploadPhoto.Click += (s, e) => UploadPhoto();

            _btnRemovePhoto = new Button { Text = "Remove Photo", Dock = DockStyle.Top, Height = 38 };
            ThemeManager.StyleButtonDanger(_btnRemovePhoto);
            _btnRemovePhoto.Click += (s, e) => ClearPhoto();

            panel.Controls.Add(_btnRemovePhoto);
            panel.Controls.Add(_btnUploadPhoto);
            panel.Controls.Add(_picPhoto);
            return panel;
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

            _txtSearch = new TextBox { Width = 240, Margin = new Padding(0, 0, 8, 0) };
            ThemeManager.StyleInput(_txtSearch);
            _txtSearch.TextChanged += (s, e) => LoadGrid();

            _btnRefresh = new Button { Text = "Refresh", Width = 86, Margin = new Padding(0, 0, 8, 0) };
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
            _btnDelete.Click += (s, e) => DisableCurrent();

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
            _txtUsername.ReadOnly = !active;
            _txtDisplayName.ReadOnly = !active;
            _cmbRole.Enabled = active;
            _chkActive.Enabled = active;
            _txtPassword.ReadOnly = !active;
            if (_btnUploadPhoto != null) _btnUploadPhoto.Enabled = active;
            if (_btnRemovePhoto != null) _btnRemovePhoto.Enabled = active && (_selectedPhotoBytes != null || _currentPhotoBytes != null);
            ApplyCrudButtonState(active, HasSelectedDataRow(_grid, "UserId"), _btnAdd, _btnEdit, _btnDelete, _btnSave, _btnCancel);
        }

        private void BeginAdd()
        {
            _editingUserId = 0;
            _txtUsername.Text = string.Empty;
            _txtDisplayName.Text = string.Empty;
            _cmbRole.SelectedIndex = _cmbRole.Items.Count > 0 ? 0 : -1;
            _chkActive.Checked = true;
            _txtPassword.Text = string.Empty;
            _currentPhotoBytes = null;
            _selectedPhotoBytes = null;
            ShowPhoto((byte[])null);
            SetEditorState(true);
            _txtUsername.Focus();
        }

        private void BeginEdit()
        {
            if (_editingUserId <= 0) return;
            SetEditorState(true);
            _txtUsername.Focus();
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
                var dt = _userService == null ? new DataTable() : _userService.GetUsers(_txtSearch == null ? string.Empty : _txtSearch.Text);
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
            _grid.DataSource = null;
            _grid.Columns.Clear();
            _grid.DataSource = dt;
            ConfigureGridColumnsForListView();

            if (_grid.Columns["PhotoPath"] != null)
            {
                _grid.Columns["PhotoPath"].Visible = false;
            }

            if (_grid.Columns["UserId"] != null)
            {
                _grid.Columns["UserId"].HeaderText = "ID";
                _grid.Columns["UserId"].DisplayIndex = 0;
                _grid.Columns["UserId"].FillWeight = 14f;
                _grid.Columns["UserId"].MinimumWidth = 56;
            }

            if (_grid.Columns["Username"] != null)
            {
                _grid.Columns["Username"].HeaderText = "Username";
                _grid.Columns["Username"].DisplayIndex = 1;
                _grid.Columns["Username"].FillWeight = 26f;
                _grid.Columns["Username"].MinimumWidth = 120;
            }

            if (_grid.Columns["DisplayName"] != null)
            {
                _grid.Columns["DisplayName"].HeaderText = "Display Name";
                _grid.Columns["DisplayName"].DisplayIndex = 2;
                _grid.Columns["DisplayName"].FillWeight = 34f;
                _grid.Columns["DisplayName"].MinimumWidth = 160;
            }

            if (_grid.Columns["Role"] != null)
            {
                _grid.Columns["Role"].HeaderText = "Role";
                _grid.Columns["Role"].DisplayIndex = 3;
                _grid.Columns["Role"].FillWeight = 16f;
                _grid.Columns["Role"].MinimumWidth = 90;
            }

            if (_grid.Columns["IsActive"] != null)
            {
                _grid.Columns["IsActive"].HeaderText = "Active";
                _grid.Columns["IsActive"].DisplayIndex = 4;
                _grid.Columns["IsActive"].FillWeight = 12f;
                _grid.Columns["IsActive"].MinimumWidth = 70;
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
                _editingUserId = 0;
                _txtUsername.Text = string.Empty;
                _txtDisplayName.Text = string.Empty;
                _cmbRole.SelectedIndex = _cmbRole.Items.Count > 0 ? 0 : -1;
                _chkActive.Checked = true;
                _txtPassword.Text = string.Empty;
                _currentPhotoBytes = null;
                _selectedPhotoBytes = null;
                ShowPhoto((byte[])null);
                SetEditorState(false);
            }
        }

        private void ConfigureGridColumnsForListView()
        {
            var allowed = new[] { "UserId", "Username", "DisplayName", "Role", "IsActive" };

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
            if (!HasSelectedDataRow(_grid, "UserId"))
            {
                _editingUserId = 0;
                SetEditorState(false);
                return;
            }

            var row = _grid.SelectedRows[0];
            if (row.Cells["UserId"] == null || row.Cells["UserId"].Value == null) return;

            _editingUserId = Convert.ToInt32(row.Cells["UserId"].Value);
            _txtUsername.Text = Convert.ToString(row.Cells["Username"].Value);
            _txtDisplayName.Text = Convert.ToString(row.Cells["DisplayName"].Value);
            _currentPhotoBytes = _userService?.GetPhotoData(_editingUserId);
            _selectedPhotoBytes = null;
            ShowPhoto(_currentPhotoBytes);

            var role = Convert.ToString(row.Cells["Role"].Value);
            if (!string.IsNullOrWhiteSpace(role) && _cmbRole.Items.Contains(role))
            {
                _cmbRole.SelectedItem = role;
            }

            _chkActive.Checked = row.Cells["IsActive"] != null &&
                                 row.Cells["IsActive"].Value != null &&
                                 row.Cells["IsActive"].Value != DBNull.Value &&
                                 Convert.ToBoolean(row.Cells["IsActive"].Value);
            _txtPassword.Text = string.Empty;
            SetEditorState(false);
        }

        private void Save()
        {
            if (_userService == null) return;

            try
            {
                var username = (_txtUsername.Text ?? string.Empty).Trim();
                var password = _txtPassword.Text ?? string.Empty;
                var role = Convert.ToString(_cmbRole.SelectedItem) ?? string.Empty;
                var displayName = (_txtDisplayName.Text ?? string.Empty).Trim();
                var isActive = _chkActive.Checked;
                var isCreate = _editingUserId == 0;

                UseWaitCursor = true;
                _btnSave.Enabled = false;

                if (isCreate)
                {
                    var vr = _userService.ValidateNewUser(username, password, role);
                    if (!vr.IsValid)
                    {
                        ThemedMessageBox.ShowError(this, vr.ToString(), "Validation");
                        return;
                    }
                }

                var photoData = _selectedPhotoBytes ?? _currentPhotoBytes;

                if (isCreate)
                {
                    _userService.Create(username, password, role, displayName, isActive, photoData);
                    ThemedMessageBox.ShowInfo(this, "User created successfully.", "Saved");
                }
                else
                {
                    _userService.Update(_editingUserId, username, role, displayName, isActive, photoData);
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        _userService.ResetPassword(_editingUserId, password);
                    }
                    ThemedMessageBox.ShowInfo(this, "User updated successfully.", "Updated");
                }

                LoadGrid();
                _currentPhotoBytes = photoData;
                _selectedPhotoBytes = null;
                ShowPhoto(_currentPhotoBytes);
                SetEditorState(false);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("UsersControl.Save", ex);
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
                if (_editingUserId > 0)
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
                ofd.Title = "Select user profile photo";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                string photoError;
                if (!PhotoStorageHelper.TryReadPhotoBytes(ofd.FileName, out _selectedPhotoBytes, out photoError))
                {
                    ThemedMessageBox.ShowError(this, photoError, "Invalid Photo");
                    return;
                }

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

        private void DisableCurrent()
        {
            if (_userService == null) return;
            if (_editingUserId <= 0) return;

            if (ThemedMessageBox.ShowConfirm(this, "Disable this user?", "Confirm") != DialogResult.OK)
            {
                return;
            }

            try
            {
                _userService.SetActive(_editingUserId, false);
                LoadGrid();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("UsersControl.DisableCurrent", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
