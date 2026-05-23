using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
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
        private Label _lblConnectionProfileTitle;
        private Label _lblConnectionProfileNote;
        private Label _lblQuickLogin;
        private ComboBox _cmbQuickLogin;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private CheckBox _chkRememberMe;
        private Button _btnLogin;
        private Button _btnRegister;

        private readonly List<QuickLoginPreset> _quickLoginPresets = BuildQuickLoginPresets();

        private DatabaseHelper _db;
        private AuthService _authService;
        private ActivityLogService _activityLogService;
        private int _serviceInitVersion;

        public LoginForm()
        {
            Text = "Login";
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

            BeginInvoke(new Action(InitializeAfterFirstPaint));
        }

        private void InitializeAfterFirstPaint()
        {
            BeginServiceInitialization();

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

            if (_cmbQuickLogin != null && _cmbQuickLogin.Visible && _cmbQuickLogin.Items.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(_txtUsername.Text))
                {
                    SyncQuickLoginSelection(_txtUsername.Text);
                }
                else
                {
                    _cmbQuickLogin.SelectedIndex = 0;
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

        private async void BeginServiceInitialization()
        {
            var version = ++_serviceInitVersion;
            SetConnectionState(false, "Checking database connection...");
            if (_btnLogin != null) _btnLogin.Enabled = false;
            if (_btnRegister != null) _btnRegister.Enabled = false;

            var result = await Task.Run(() => InitializeServicesCore());
            if (version != _serviceInitVersion || IsDisposed)
            {
                return;
            }

            _db = result.Database;
            _authService = result.AuthService;
            _activityLogService = result.ActivityLogService;
            SetConnectionState(result.IsConnected, result.ErrorMessage);
        }

        private static ServiceInitializationResult InitializeServicesCore()
        {
            try
            {
                var db = DatabaseHelper.FromConfig();
                string error;
                if (!db.TestConnection(out error))
                {
                    return ServiceInitializationResult.Failed(BuildStaticConnectionStateMessage(error));
                }

                SchemaMigrationRunner.EnsureCurrent(db);
                IUserData userData = new UserData(db);
                IActivityLogData activityLogData = new ActivityLogData(db);
                var activityLogService = new ActivityLogService(activityLogData);
                var authService = new AuthService(userData, activityLogService);
                return ServiceInitializationResult.Success(db, authService, activityLogService);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.TryInitServices", ex);
                return ServiceInitializationResult.Failed(BuildStaticConnectionStateMessage(ex.Message));
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
            var cardLogo = BrandAssets.CreateLogoMarkImage();
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
                RowCount = 17
            };
            var stack = _stack;
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));   // 0  title
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // 1  subtitle
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 2  lblConnectionMode
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));   // 3  connection profile row
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 4  lblQuickLogin
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // 5  cmbQuickLogin
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));    // 6  dbStatus (collapsed by default)
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 7  lblUser
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // 8  txtUsername
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // 9  lblPass
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));   // 10 txtPassword
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // 11 options
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));   // 12 btnLogin
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));   // 13 forgotLink
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));   // 14 btnRegister
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));   // 15 footerNote
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // 16 spacer

            var title = new Label
            {
                Text = "Log in",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.AuthTitle,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitle = new Label
            {
                Text = "Sign in using your existing account credentials. Database profiles are managed in Settings.",
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

            var lblConnectionMode = MakeLabel("Database Profile");
            var lblUser = MakeLabel("Username");
            var lblPass = MakeLabel("Password");
            _lblQuickLogin = MakeLabel("Quick Login Account");

            var connectionModeRow = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.SurfaceAlt,
                Margin = new Padding(0),
                Padding = new Padding(12, 6, 12, 6)
            };
            UiHelper.ApplyRoundedCorners(connectionModeRow, 6);

            _lblConnectionProfileTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 16,
                Font = ThemeFonts.CaptionStrong,
                ForeColor = ThemeColors.Secondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblConnectionProfileNote = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            connectionModeRow.Controls.Add(_lblConnectionProfileNote);
            connectionModeRow.Controls.Add(_lblConnectionProfileTitle);

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
            stack.Controls.Add(lblConnectionMode, 0, 2);
            stack.Controls.Add(connectionModeRow, 0, 3);
            stack.Controls.Add(_lblQuickLogin, 0, 4);
            stack.Controls.Add(_cmbQuickLogin, 0, 5);
            stack.Controls.Add(_lblDbStatus, 0, 6);
            stack.Controls.Add(lblUser, 0, 7);
            stack.Controls.Add(_txtUsername, 0, 8);
            stack.Controls.Add(lblPass, 0, 9);
            stack.Controls.Add(_txtPassword, 0, 10);
            stack.Controls.Add(options, 0, 11);
            stack.Controls.Add(_btnLogin, 0, 12);
            stack.Controls.Add(forgotLink, 0, 13);
            stack.Controls.Add(_btnRegister, 0, 14);
            stack.Controls.Add(footerNote, 0, 15);
            stack.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 16);

            shell.Controls.Add(stack);

            ApplyQuickLoginVisibility();
            LoadConnectionProfileSummary();

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
            if (_quickLoginPresets.Count == 0)
            {
                return;
            }

            _cmbQuickLogin.Items.Add("Select account...");
            for (var i = 0; i < _quickLoginPresets.Count; i++)
            {
                _cmbQuickLogin.Items.Add(_quickLoginPresets[i]);
            }
        }

        private void ApplyQuickLoginVisibility()
        {
            var enabled = _quickLoginPresets.Count > 0;

            if (_lblQuickLogin != null)
            {
                _lblQuickLogin.Visible = enabled;
            }

            if (_cmbQuickLogin != null)
            {
                _cmbQuickLogin.Visible = enabled;
            }

            if (_stack != null)
            {
                _stack.RowStyles[4] = new RowStyle(SizeType.Absolute, enabled ? 22 : 0);
                _stack.RowStyles[5] = new RowStyle(SizeType.Absolute, enabled ? 44 : 0);
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
                _stack.RowStyles[6] = new RowStyle(SizeType.Absolute, isConnected ? 0 : 36);
            }
            // Keep actions clickable so users can retry after starting DB without restarting the app.
            if (_btnLogin != null) _btnLogin.Enabled = isConnected;
            if (_btnRegister != null) _btnRegister.Enabled = true;
        }

        private void LoadConnectionProfileSummary()
        {
            if (_lblConnectionProfileTitle == null || _lblConnectionProfileNote == null)
            {
                return;
            }

            var mode = ConnectionModeHelper.GetCurrentMode("Online");
            _lblConnectionProfileTitle.Text = "Active Profile: " + ConnectionModeHelper.GetDisplayName(mode);
            _lblConnectionProfileNote.Text = ConnectionModeHelper.GetLoginSummary(mode);
        }

        private string BuildConnectionStateMessage(string rawError)
        {
            return BuildStaticConnectionStateMessage(rawError);
        }

        private static string BuildStaticConnectionStateMessage(string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
            {
                return Messages.NoDatabaseConnection;
            }

            var message = rawError.Trim();
            var lower = message.ToLowerInvariant();

            if (lower.Contains("access denied for user") || (lower.Contains("host") && lower.Contains("not allowed to connect")))
            {
                return "Database access denied. Ask an administrator to review the saved database profile in Settings > Database, then verify the MySQL host grants for '" +
                    Environment.MachineName + "'.";
            }

            if (lower.Contains("reading from the stream has failed") ||
                lower.Contains("unable to read data from the transport connection") ||
                lower.Contains("connected party did not properly respond") ||
                lower.Contains("host has failed to respond") ||
                lower.Contains("unable to connect") ||
                lower.Contains("actively refused"))
            {
                return "Cannot reach the saved database profile. Ask an administrator to verify the server address, port, network/internet access, and firewall settings.";
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

        private static List<QuickLoginPreset> BuildQuickLoginPresets()
        {
            var presets = new List<QuickLoginPreset>();
            foreach (var account in AppRuntimeSettings.GetDemoQuickLoginAccounts())
            {
                presets.Add(new QuickLoginPreset(account.Label, account.Username, account.Password));
            }

            return presets;
        }

        private sealed class ServiceInitializationResult
        {
            public bool IsConnected { get; private set; }
            public string ErrorMessage { get; private set; }
            public DatabaseHelper Database { get; private set; }
            public AuthService AuthService { get; private set; }
            public ActivityLogService ActivityLogService { get; private set; }

            public static ServiceInitializationResult Success(DatabaseHelper database, AuthService authService, ActivityLogService activityLogService)
            {
                return new ServiceInitializationResult
                {
                    IsConnected = true,
                    Database = database,
                    AuthService = authService,
                    ActivityLogService = activityLogService,
                    ErrorMessage = null
                };
            }

            public static ServiceInitializationResult Failed(string errorMessage)
            {
                return new ServiceInitializationResult
                {
                    IsConnected = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
