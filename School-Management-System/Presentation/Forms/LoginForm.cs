using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.Common;
using School_Management_System.DataLayer;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Forms
{
    public sealed partial class LoginForm : BaseForm
    {
        private Panel _root;
        private Panel _hero;
        private Panel _authHost;
        private Panel _card;

        private TableLayoutPanel _stack;
        private Label _lblDbStatus;
        private ComboBox _cmbQuickLogin;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private CheckBox _chkRememberMe;
        private Button _btnLogin;
        private Button _btnRegister;

        private readonly List<QuickLoginPreset> _quickLoginPresets = new List<QuickLoginPreset>
        {
            new QuickLoginPreset("Admin", "admin", "admin123"),
            new QuickLoginPreset("Staff", "faculty1", "faculty123"),
            new QuickLoginPreset("Registrar", "registrar", "registrar123"),
            new QuickLoginPreset("Student", "student1", "student123")
        };

        private DatabaseHelper _db;
        private AuthService _authService;
        private ActivityLogService _activityLogService;

        public LoginForm()
        {
            Text = AppConstants.AppTitle + " - Login";
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

            _chkRememberMe.Checked = UserPreferences.RememberMe;
            var rememberedUsername = UserPreferences.RememberedUsername ?? string.Empty;
            if (_chkRememberMe.Checked &&
                !string.IsNullOrWhiteSpace(rememberedUsername) &&
                !string.Equals(rememberedUsername.Trim(), "admin", StringComparison.OrdinalIgnoreCase))
            {
                _txtUsername.Text = rememberedUsername;
                _txtUsername.SelectionStart = _txtUsername.TextLength;
            }
            else if (string.Equals(rememberedUsername.Trim(), "admin", StringComparison.OrdinalIgnoreCase))
            {
                // Do not auto-fill built-in admin account on startup.
                UserPreferences.RememberMe = false;
                UserPreferences.RememberedUsername = null;
                _chkRememberMe.Checked = false;
                _txtUsername.Text = string.Empty;
            }

            if (_cmbQuickLogin != null)
            {
                if (!string.IsNullOrWhiteSpace(_txtUsername.Text))
                {
                    SyncQuickLoginSelection(_txtUsername.Text);
                }
                else if (_cmbQuickLogin.Items.Count > 1)
                {
                    _cmbQuickLogin.SelectedIndex = 1;
                }
            }

            _txtUsername.Focus();
        }

        private void TryInitServices()
        {
            try
            {
                _db = DatabaseHelper.FromConfig();
                string error;
                if (!_db.TestConnection(out error))
                {
                    _authService = null;
                    SetConnectionState(false, BuildConnectionStateMessage(error));
                }
                else
                {
                    SchemaMigrationRunner.EnsureCurrent(_db);
                    IUserData userData = new UserData(_db);
                    IActivityLogData activityLogData = new ActivityLogData(_db);
                    _activityLogService = new ActivityLogService(activityLogData);
                    _authService = new AuthService(userData, _activityLogService);
                    SetConnectionState(true, null);
                }
            }
            catch (Exception ex)
            {
                _authService = null;
                _activityLogService = null;
                SetConnectionState(false, BuildConnectionStateMessage(ex.Message));
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.TryInitServices", ex);
            }
        }

        private void InitializeRuntimeComponent()
        {
            _root = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface };
            _root.Paint += DrawRootBackground;
            Controls.Add(_root);

            _hero = BuildHeroPanel();
            var divider = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = ThemeColors.Border };
            _authHost = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface, Padding = new Padding(46, 34, 46, 34) };

            _card = new Panel { Size = new Size(560, 520), BackColor = ThemeColors.Surface };
            _authHost.Controls.Add(_card);
            _authHost.Resize += (s, e) => CenterCard();

            BuildLoginCardContent();

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

        private void BuildLoginCardContent()
        {
            _card.Controls.Clear();
            var shell = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Surface, Padding = new Padding(10) };
            _card.Controls.Add(shell);

            _stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 15
            };
            var stack = _stack;
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));   // 0  title
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // 1  subtitle
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 2  lblQuickLogin
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // 3  cmbQuickLogin
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));    // 4  dbStatus (collapsed by default)
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 5  lblUser
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // 6  txtUsername
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 7  lblPass
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));   // 8  txtPassword
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // 9  options
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));   // 10 btnLogin
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));   // 11 forgotLink
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));   // 12 btnRegister
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));   // 13 footerNote
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // 14 spacer

            var title = new Label
            {
                Text = "Log in to School Management",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.AuthTitle,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitle = new Label
            {
                Text = "Sign in using your existing account credentials.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblDbStatus = new Label
            {
                Text = Messages.NoDatabaseConnection,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.AccentDanger,
                Dock = DockStyle.Fill,
                Visible = false,
                TextAlign = ContentAlignment.TopLeft
            };

            var lblUser = MakeLabel("Username");
            var lblPass = MakeLabel("Password");
            var lblQuickLogin = MakeLabel("Quick Login Account");

            _cmbQuickLogin = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0),
                Height = 38
            };
            ThemeManager.StyleComboBox(_cmbQuickLogin);
            InitializeQuickLoginItems();
            _cmbQuickLogin.SelectedIndexChanged += (s, e) => ApplyQuickLoginSelection();

            _txtUsername = MakeTextBox();
            _txtPassword = MakeTextBox();
            _txtPassword.UseSystemPasswordChar = true;

            _chkShowPassword = new CheckBox { Text = "Show password", AutoSize = true, Dock = DockStyle.Left, Margin = new Padding(0, 6, 0, 0) };
            _chkShowPassword.CheckedChanged += (s, e) => _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
            ThemeManager.StyleCheckBox(_chkShowPassword);

            _chkRememberMe = new CheckBox { Text = "Remember me", AutoSize = true, Dock = DockStyle.Right, Margin = new Padding(0, 6, 0, 0) };
            ThemeManager.StyleCheckBox(_chkRememberMe);

            var options = new Panel { Dock = DockStyle.Fill };
            options.Controls.Add(_chkShowPassword);
            options.Controls.Add(_chkRememberMe);

            _btnLogin = new Button { Text = "Log in", Dock = DockStyle.Fill };
            ThemeManager.StyleButtonPrimary(_btnLogin);
            _btnLogin.Margin = new Padding(0);
            _btnLogin.Click += (s, e) => DoLogin();

            var forgotLink = new LinkLabel
            {
                Text = "Forgot password?",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = ThemeFonts.Label
            };
            ThemeManager.StyleLinkLabel(forgotLink);
            forgotLink.Click += (s, e) => ShowInfo("Please contact your administrator to reset your password.", "Password Recovery");

            _btnRegister = new Button { Text = "Create new account", Dock = DockStyle.Fill };
            ThemeManager.StyleButtonNeutral(_btnRegister);
            _btnRegister.Margin = new Padding(0);
            _btnRegister.Click += (s, e) => OpenRegisterForm();
            var footerNote = new Label
            {
                Text = "Use Register to create an account, then sign in here.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0)
            };

            stack.Controls.Add(title, 0, 0);
            stack.Controls.Add(subtitle, 0, 1);
            stack.Controls.Add(lblQuickLogin, 0, 2);
            stack.Controls.Add(_cmbQuickLogin, 0, 3);
            stack.Controls.Add(_lblDbStatus, 0, 4);
            stack.Controls.Add(lblUser, 0, 5);
            stack.Controls.Add(_txtUsername, 0, 6);
            stack.Controls.Add(lblPass, 0, 7);
            stack.Controls.Add(_txtPassword, 0, 8);
            stack.Controls.Add(options, 0, 9);
            stack.Controls.Add(_btnLogin, 0, 10);
            stack.Controls.Add(forgotLink, 0, 11);
            stack.Controls.Add(_btnRegister, 0, 12);
            stack.Controls.Add(footerNote, 0, 13);
            stack.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 14);

            shell.Controls.Add(stack);

            _txtUsername.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    _txtPassword.Focus();
                    e.Handled = true;
                }
            };

            _txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    DoLogin();
                    e.Handled = true;
                }
            };
        }

        private void InitializeQuickLoginItems()
        {
            if (_cmbQuickLogin == null)
            {
                return;
            }

            _cmbQuickLogin.Items.Clear();
            _cmbQuickLogin.Items.Add("Select account...");
            for (var i = 0; i < _quickLoginPresets.Count; i++)
            {
                _cmbQuickLogin.Items.Add(_quickLoginPresets[i]);
            }
        }

        private void ApplyQuickLoginSelection()
        {
            if (_cmbQuickLogin == null)
            {
                return;
            }

            var preset = _cmbQuickLogin.SelectedItem as QuickLoginPreset;
            if (preset == null)
            {
                return;
            }

            _txtUsername.Text = preset.Username;
            _txtPassword.Text = preset.Password;
            _txtPassword.SelectionStart = _txtPassword.TextLength;
        }

        private void SyncQuickLoginSelection(string username)
        {
            if (_cmbQuickLogin == null || string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            for (var i = 0; i < _quickLoginPresets.Count; i++)
            {
                if (string.Equals(_quickLoginPresets[i].Username, username.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    _cmbQuickLogin.SelectedIndex = i + 1;
                    return;
                }
            }

            _cmbQuickLogin.SelectedIndex = 0;
        }

        private void SetConnectionState(bool isConnected, string errorMessage)
        {
            _lblDbStatus.Text = isConnected ? string.Empty : (string.IsNullOrWhiteSpace(errorMessage) ? Messages.NoDatabaseConnection : errorMessage);
            _lblDbStatus.Visible = !isConnected;
            if (_stack != null)
            {
                _stack.RowStyles[4] = new RowStyle(SizeType.Absolute, isConnected ? 0 : 36);
            }
            // Keep actions clickable so users can retry after starting DB without restarting the app.
            if (_btnLogin != null) _btnLogin.Enabled = true;
            if (_btnRegister != null) _btnRegister.Enabled = true;
        }

        private string BuildConnectionStateMessage(string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
            {
                return Messages.NoDatabaseConnection;
            }

            var message = rawError.Trim();
            var lower = message.ToLowerInvariant();

            if (lower.Contains("access denied for user") || (lower.Contains("host") && lower.Contains("not allowed to connect")))
            {
                return "Database access denied. Run DatabaseScripts\\Allow-RemoteRoot.ps1 on the MySQL server using an app user, then allow host '" +
                    Environment.MachineName + "' (or '%') and retry.";
            }

            if (message.Length > 220)
            {
                return message.Substring(0, 220) + "...";
            }

            return message;
        }

        private void OpenRegisterForm()
        {
            try
            {
                if (_db == null)
                {
                    TryInitServices();
                    if (_db == null)
                    {
                        ShowError(string.IsNullOrWhiteSpace(_lblDbStatus.Text) ? Messages.NoDatabaseConnection : _lblDbStatus.Text);
                        return;
                    }
                }

                Hide();
                using (var register = new RegisterForm(_db))
                {
                    var result = register.ShowDialog(this);
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(register.RegisteredUsername))
                    {
                        _txtUsername.Text = register.RegisteredUsername;
                        _txtPassword.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.OpenRegisterForm", ex);
                ShowError(Messages.UnexpectedError);
            }
            finally
            {
                Show();
                Activate();
                _txtPassword.Focus();
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

        private void DoLogin()
        {
            if (_authService == null)
            {
                TryInitServices();
                if (_authService == null)
                {
                    ShowError(string.IsNullOrWhiteSpace(_lblDbStatus.Text) ? Messages.NoDatabaseConnection : _lblDbStatus.Text);
                    return;
                }
            }

            var username = (_txtUsername.Text ?? string.Empty).Trim();
            var password = _txtPassword.Text ?? string.Empty;

            try
            {
                UseWaitCursor = true;
                _btnLogin.Enabled = false;

                School_Management_System.Models.User user;
                string errorMessage;
                if (!_authService.TryLogin(username, password, out user, out errorMessage))
                {
                    ShowError(errorMessage ?? Messages.InvalidCredentials, "Login Failed");
                    return;
                }

                var canRememberUser = _chkRememberMe.Checked &&
                    !string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase);
                UserPreferences.RememberMe = canRememberUser;
                UserPreferences.RememberedUsername = canRememberUser ? username : null;

                UserSession.Start(user);

                Hide();
                using (var dashboard = new DashboardForm(_db))
                {
                    dashboard.ShowDialog(this);
                }
                UserSession.End();

                _txtPassword.Text = string.Empty;
                Show();
                _txtPassword.Focus();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.DoLogin", ex);
                ShowError(Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnLogin.Enabled = true;
            }
        }

        private sealed class QuickLoginPreset
        {
            public QuickLoginPreset(string label, string username, string password)
            {
                Label = label ?? string.Empty;
                Username = username ?? string.Empty;
                Password = password ?? string.Empty;
            }

            public string Label { get; private set; }
            public string Username { get; private set; }
            public string Password { get; private set; }

            public override string ToString()
            {
                return Label;
            }
        }
    }
}
