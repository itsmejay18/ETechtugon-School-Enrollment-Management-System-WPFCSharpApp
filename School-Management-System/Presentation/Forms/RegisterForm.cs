using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.DataLayer;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Forms
{
    public sealed partial class RegisterForm : BaseForm
    {
        private Panel _root;
        private Panel _hero;
        private Panel _authHost;
        private Panel _card;

        private Label _lblDbStatus;

        private TextBox _txtUsername;
        private TextBox _txtDisplayName;
        private ComboBox _cmbRole;
        private TextBox _txtPassword;
        private TextBox _txtConfirmPassword;
        private CheckBox _chkShowPassword;
        private Button _btnCreate;
        private Button _btnCancel;

        private DatabaseHelper _db;
        private UserManagementService _userManagementService;

        public string RegisteredUsername { get; private set; }

        public RegisterForm()
            : this(null)
        {
        }

        public RegisterForm(DatabaseHelper db = null)
        {
            _db = db;

            Text = AppConstants.AppTitle + " - Register";
            MinimumSize = new Size(960, 620);
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Maximized;

            InitializeComponent();
            if (IsDesignerHost())
            {
                InitializeDesignerSurface();
                return;
            }

            InitializeRuntimeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (IsDesignerHost())
            {
                return;
            }

            TryInitServices();
            _txtUsername.Focus();
        }

        private void TryInitServices()
        {
            try
            {
                if (_db == null)
                {
                    _db = DatabaseHelper.FromConfig();
                }

                string error;
                if (!_db.TestConnection(out error))
                {
                    _userManagementService = null;
                    SetConnectionState(false, string.IsNullOrWhiteSpace(error) ? Messages.NoDatabaseConnection : error);
                }
                else
                {
                    IUserManagementData userData = new UserManagementData(_db);
                    _userManagementService = new UserManagementService(userData);
                    SetConnectionState(true, null);
                }
            }
            catch (Exception ex)
            {
                _userManagementService = null;
                SetConnectionState(false, Messages.NoDatabaseConnection);
                School_Management_System.DataLayer.Logging.FileLogger.LogError("RegisterForm.TryInitServices", ex);
            }
        }

        private void InitializeRuntimeComponent()
        {
            _root = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface };
            _root.Paint += DrawRootBackground;
            Controls.Add(_root);

            _hero = BuildHeroPanel();
            var divider = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = ThemeColors.Border };
            _authHost = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface, Padding = new Padding(46, 28, 46, 28) };
            _card = new Panel { Size = new Size(760, 620), BackColor = ThemeColors.Surface };
            _authHost.Controls.Add(_card);
            _authHost.Resize += (s, e) => CenterCard();

            BuildRegisterCardContent();

            _root.Controls.Add(_authHost);
            _root.Controls.Add(divider);
            _root.Controls.Add(_hero);
            CenterCard();
        }

        private void InitializeDesignerSurface()
        {
            Controls.Clear();
            BackColor = ThemeColors.Background;
        }

        private bool IsDesignerHost()
        {
            if (IsInDesigner) return true;

            try
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                return string.Equals(processName, "devenv", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private Panel BuildHeroPanel()
        {
            var panel = new Panel { Dock = DockStyle.Left, Width = 640, BackColor = ThemeColors.Surface, Padding = new Padding(44, 34, 44, 34) };
            var visualHost = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface };
            var cardLogo = BrandAssets.CreateLogoImage();
            var logoCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 8, 16, 8),
                BackColor = Color.Transparent
            };
            var logoFrame = new Panel
            {
                Size = new Size(440, 420),
                BackColor = Color.Transparent
            };
            var heroLogo = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = cardLogo ?? IconFactory.CreateCircleIcon(ThemeColors.SidebarIcon, IconKind.School, 260),
                SizeMode = cardLogo != null ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage
            };
            logoFrame.Controls.Add(heroLogo);
            logoCanvas.Controls.Add(logoFrame);
            logoCanvas.Resize += (s, e) =>
            {
                logoFrame.Left = (logoCanvas.ClientSize.Width - logoFrame.Width) / 2;
                logoFrame.Top = (logoCanvas.ClientSize.Height - logoFrame.Height) / 2;
            };
            visualHost.Controls.Add(logoCanvas);
            panel.Controls.Add(visualHost);
            return panel;
        }

        private void BuildRegisterCardContent()
        {
            _card.Controls.Clear();
            var shell = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface, Padding = new Padding(14) };
            _card.Controls.Add(shell);

            var title = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Text = "Create your account",
                Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Enter account details to register new dashboard access.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblDbStatus = new Label
            {
                Text = Messages.NoDatabaseConnection,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.AccentDanger,
                Dock = DockStyle.Top,
                Height = 26,
                Visible = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 8,
                Height = 248,
                Padding = new Padding(0, 8, 0, 0)
            };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            _txtUsername = MakeTextBox();
            _txtDisplayName = MakeTextBox();
            _txtPassword = MakeTextBox();
            _txtPassword.UseSystemPasswordChar = true;
            _txtConfirmPassword = MakeTextBox();
            _txtConfirmPassword.UseSystemPasswordChar = true;

            _cmbRole = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Margin = new Padding(0) };
            ThemeManager.StyleComboBox(_cmbRole);
            _cmbRole.Height = 38;
            _cmbRole.Items.AddRange(new object[] { AppConstants.Roles.Registrar, AppConstants.Roles.Faculty });
            if (_cmbRole.Items.Count > 0) _cmbRole.SelectedIndex = 0;

            var lblUser = MakeLabel("Username *");
            var lblDisplay = MakeLabel("Display Name");
            var lblRole = MakeLabel("Role *");
            var lblPassword = MakeLabel("Password *");
            var lblConfirm = MakeLabel("Confirm Password *");

            const int columnGap = 12;
            var leftCellMargin = new Padding(0, 0, columnGap / 2, 0);
            var rightCellMargin = new Padding(columnGap / 2, 0, 0, 0);

            lblUser.Margin = leftCellMargin;
            _txtUsername.Margin = leftCellMargin;
            lblDisplay.Margin = rightCellMargin;
            _txtDisplayName.Margin = rightCellMargin;
            lblPassword.Margin = leftCellMargin;
            _txtPassword.Margin = leftCellMargin;
            lblConfirm.Margin = rightCellMargin;
            _txtConfirmPassword.Margin = rightCellMargin;

            form.Controls.Add(lblUser, 0, 0);
            form.Controls.Add(lblDisplay, 1, 0);
            form.Controls.Add(_txtUsername, 0, 1);
            form.Controls.Add(_txtDisplayName, 1, 1);
            form.Controls.Add(lblRole, 0, 2);
            form.Controls.Add(new Panel(), 1, 2);
            form.Controls.Add(_cmbRole, 0, 3);
            form.SetColumnSpan(_cmbRole, 2);
            form.Controls.Add(lblPassword, 0, 4);
            form.Controls.Add(lblConfirm, 1, 4);
            form.Controls.Add(_txtPassword, 0, 5);
            form.Controls.Add(_txtConfirmPassword, 1, 5);

            _chkShowPassword = new CheckBox { Text = "Show password", AutoSize = true, Dock = DockStyle.Left, Margin = new Padding(0, 6, 0, 0) };
            _chkShowPassword.CheckedChanged += (s, e) =>
            {
                var visible = _chkShowPassword.Checked;
                _txtPassword.UseSystemPasswordChar = !visible;
                _txtConfirmPassword.UseSystemPasswordChar = !visible;
            };
            form.Controls.Add(_chkShowPassword, 0, 6);
            form.SetColumnSpan(_chkShowPassword, 2);

            _btnCreate = new Button { Text = "Create account", Dock = DockStyle.Fill };
            ThemeManager.StyleButtonPrimary(_btnCreate);
            _btnCreate.Margin = new Padding(0);
            _btnCreate.Click += (s, e) => DoRegister();
            form.Controls.Add(_btnCreate, 0, 7);
            form.SetColumnSpan(_btnCreate, 2);

            var actions = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(0, 8, 0, 0) };
            _btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => Close();
            actions.Resize += (s, e) =>
            {
                _btnCancel.Left = actions.ClientSize.Width - _btnCancel.Width;
                _btnCancel.Top = Math.Max(0, (actions.ClientSize.Height - _btnCancel.Height) / 2);
            };
            actions.Controls.Add(_btnCancel);

            var note = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Text = "Admin accounts are managed from User Roles inside the dashboard.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            shell.Controls.Add(note);
            shell.Controls.Add(actions);
            shell.Controls.Add(form);
            shell.Controls.Add(_lblDbStatus);
            shell.Controls.Add(subtitle);
            shell.Controls.Add(title);

            _txtConfirmPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    DoRegister();
                    e.Handled = true;
                }
            };
        }

        private void SetConnectionState(bool isConnected, string errorMessage)
        {
            _lblDbStatus.Text = isConnected ? string.Empty : (string.IsNullOrWhiteSpace(errorMessage) ? Messages.NoDatabaseConnection : errorMessage);
            _lblDbStatus.Visible = !isConnected;
            // Keep create enabled so users can retry once DB becomes reachable.
            if (_btnCreate != null) _btnCreate.Enabled = true;
        }

        private void DoRegister()
        {
            if (_userManagementService == null)
            {
                TryInitServices();
                if (_userManagementService == null)
                {
                    ShowError(Messages.NoDatabaseConnection);
                    return;
                }
            }

            var username = (_txtUsername.Text ?? string.Empty).Trim();
            var displayName = (_txtDisplayName.Text ?? string.Empty).Trim();
            var role = Convert.ToString(_cmbRole.SelectedItem) ?? string.Empty;
            var password = _txtPassword.Text ?? string.Empty;
            var confirmPassword = _txtConfirmPassword.Text ?? string.Empty;

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ShowError("Password and confirmation password do not match.", "Validation");
                return;
            }

            try
            {
                var vr = _userManagementService.ValidateNewUser(username, password, role);
                if (!vr.IsValid)
                {
                    ShowError(vr.ToString(), "Validation");
                    return;
                }

                UseWaitCursor = true;
                _btnCreate.Enabled = false;
                _userManagementService.Create(username, password, role, displayName, true);

                RegisteredUsername = username;
                ShowInfo("Account created successfully. Please sign in.", "Registration Complete");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("RegisterForm.DoRegister", ex);
                ShowError(Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnCreate.Enabled = true;
            }
        }

        private void CenterCard()
        {
            if (_card == null || _authHost == null) return;
            _card.Left = (_authHost.ClientSize.Width - _card.Width) / 2;
            _card.Top = (_authHost.ClientSize.Height - _card.Height) / 2;
        }

        private void DrawRootBackground(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(_root.ClientRectangle, ThemeColors.Surface, ThemeColors.Background, 145f))
            {
                e.Graphics.FillRectangle(brush, _root.ClientRectangle);
            }
        }

        private static Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static TextBox MakeTextBox()
        {
            var tb = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Input,
                Margin = new Padding(0),
                AutoSize = false,
                Height = 38,
                MinimumSize = new Size(0, 38)
            };
            ThemeManager.StyleInput(tb);
            return tb;
        }
    }
}
