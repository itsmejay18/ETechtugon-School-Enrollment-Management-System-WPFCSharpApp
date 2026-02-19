using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.Common;
using School_Management_System.DataLayer;
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

        private Label _lblDbStatus;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private CheckBox _chkRememberMe;
        private Button _btnLogin;
        private Button _btnRegister;
        private Button _btnExit;

        private DatabaseHelper _db;
        private AuthService _authService;

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
            if (_chkRememberMe.Checked)
            {
                _txtUsername.Text = UserPreferences.RememberedUsername ?? string.Empty;
                _txtUsername.SelectionStart = _txtUsername.TextLength;
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
                    SetConnectionState(false, string.IsNullOrWhiteSpace(error) ? Messages.NoDatabaseConnection : error);
                }
                else
                {
                    IUserData userData = new UserData(_db);
                    _authService = new AuthService(userData);
                    SetConnectionState(true, null);
                }
            }
            catch (Exception ex)
            {
                _authService = null;
                SetConnectionState(false, Messages.NoDatabaseConnection);
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.TryInitServices", ex);
            }
        }

        private void InitializeRuntimeComponent()
        {
            _root = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            _root.Paint += DrawRootBackground;
            Controls.Add(_root);

            _hero = BuildHeroPanel();
            _authHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(56, 36, 56, 36) };

            _card = new Panel { Size = new Size(560, 450), BackColor = ThemeColors.CardBackground };
            ThemeManager.StyleCardPanel(_card);
            _authHost.Controls.Add(_card);
            _authHost.Resize += (s, e) => CenterCard();

            BuildLoginCardContent();

            _root.Controls.Add(_authHost);
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
            var panel = new Panel { Dock = DockStyle.Left, Width = 430, BackColor = ThemeColors.Primary, Padding = new Padding(34, 36, 34, 36) };
            panel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(panel.ClientRectangle, ColorTranslator.FromHtml("#243447"), ThemeColors.Primary, 130f))
                {
                    e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                }
            };

            var logo = new PictureBox
            {
                Size = new Size(74, 74),
                Location = new Point(34, 42),
                Image = IconFactory.CreateCircleIcon(ThemeColors.Secondary, IconKind.School, 74),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            var title = new Label
            {
                Text = "School Management\nSystem",
                Location = new Point(34, 132),
                Size = new Size(280, 72),
                Font = ThemeFonts.Header,
                ForeColor = Color.White
            };

            var subtitle = new Label
            {
                Text = "Centralize enrollment, curriculum, users, and daily operations in one streamlined workspace.",
                Location = new Point(34, 218),
                Size = new Size(332, 74),
                Font = ThemeFonts.Label,
                ForeColor = ColorTranslator.FromHtml("#D6E4F0")
            };

            var badge = new Panel { Location = new Point(34, 318), Size = new Size(316, 80), BackColor = Color.FromArgb(44, 82, 120), Padding = new Padding(12, 10, 12, 10) };
            UiHelper.ApplyRoundedCorners(badge, 6);

            var badgeIcon = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 44,
                Image = IconFactory.CreateGlyphIcon(IconKind.Trend, 22, Color.White),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            var badgeText = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Live academic insights\nwith role-based control",
                ForeColor = Color.White,
                Font = ThemeFonts.Label,
                TextAlign = ContentAlignment.MiddleLeft
            };

            badge.Controls.Add(badgeText);
            badge.Controls.Add(badgeIcon);

            panel.Controls.Add(badge);
            panel.Controls.Add(subtitle);
            panel.Controls.Add(title);
            panel.Controls.Add(logo);
            return panel;
        }

        private void BuildLoginCardContent()
        {
            var cardTop = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = ThemeColors.CardBackground, Padding = new Padding(20, 16, 20, 12) };
            var tabs = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            var btnLoginTab = new Button { Text = "Login", Width = 130, Height = 34, Enabled = false };
            ThemeManager.StyleButtonPrimary(btnLoginTab);

            _btnRegister = new Button { Text = "Register", Width = 130, Height = 34, Margin = new Padding(10, 0, 0, 0) };
            ThemeManager.StyleButtonNeutral(_btnRegister);
            _btnRegister.Click += (s, e) => OpenRegisterForm();

            tabs.Controls.Add(btnLoginTab);
            tabs.Controls.Add(_btnRegister);

            var subtitle = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "Sign in to continue to your dashboard",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cardTop.Controls.Add(subtitle);
            cardTop.Controls.Add(tabs);
            _card.Controls.Add(cardTop);

            var body = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(20, 2, 20, 20) };
            _card.Controls.Add(body);

            var statusHost = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = ThemeColors.CardBackground, Padding = new Padding(2, 4, 2, 0) };
            _lblDbStatus = new Label
            {
                Text = Messages.NoDatabaseConnection,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.AccentDanger,
                Dock = DockStyle.Fill,
                Visible = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var gb = new GroupBox
            {
                Text = "Account Credentials",
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 18, 14, 12)
            };
            ThemeManager.StyleGroupBox(gb);
            body.Controls.Add(gb);
            statusHost.Controls.Add(_lblDbStatus);
            body.Controls.Add(statusHost);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(4, 12, 4, 4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtUsername = MakeTextBox();
            _txtPassword = MakeTextBox();
            _txtPassword.UseSystemPasswordChar = true;

            layout.Controls.Add(MakeLabel("Username"), 0, 0);
            layout.Controls.Add(_txtUsername, 1, 0);
            layout.Controls.Add(MakeLabel("Password"), 0, 1);
            layout.Controls.Add(_txtPassword, 1, 1);

            var options = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Margin = new Padding(0)
            };
            _chkShowPassword = new CheckBox { Text = "Show password", AutoSize = true, Margin = new Padding(0, 6, 18, 0) };
            _chkShowPassword.CheckedChanged += (s, e) => _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
            _chkRememberMe = new CheckBox { Text = "Remember me", AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            options.Controls.Add(_chkShowPassword);
            options.Controls.Add(_chkRememberMe);
            layout.Controls.Add(options, 0, 2);
            layout.SetColumnSpan(options, 2);

            var row1 = new Panel { Dock = DockStyle.Fill };
            var right1 = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = 290 };

            _btnLogin = new Button { Text = "Login", Width = 140, Height = 34, Margin = new Padding(10, 0, 0, 0) };
            _btnLogin.Image = IconFactory.CreateGlyphIcon(IconKind.Login, 16, Color.White);
            _btnLogin.TextImageRelation = TextImageRelation.ImageBeforeText;
            ThemeManager.StyleButtonPrimary(_btnLogin);
            _btnLogin.Click += (s, e) => DoLogin();

            _btnExit = new Button { Text = "Exit", Width = 140, Height = 34 };
            _btnExit.Image = IconFactory.CreateGlyphIcon(IconKind.Logout, 16, Color.White);
            _btnExit.TextImageRelation = TextImageRelation.ImageBeforeText;
            ThemeManager.StyleButtonDanger(_btnExit);
            _btnExit.Click += (s, e) => Close();

            right1.Controls.Add(_btnLogin);
            right1.Controls.Add(_btnExit);
            row1.Controls.Add(right1);
            layout.Controls.Add(row1, 0, 3);
            layout.SetColumnSpan(row1, 2);

            var note = new Label
            {
                Text = "Switch to Register to create a new account.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.TopLeft
            };
            layout.Controls.Add(note, 0, 4);
            layout.SetColumnSpan(note, 2);

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

            gb.Controls.Add(layout);
        }

        private void SetConnectionState(bool isConnected, string errorMessage)
        {
            _lblDbStatus.Text = isConnected ? string.Empty : (string.IsNullOrWhiteSpace(errorMessage) ? Messages.NoDatabaseConnection : errorMessage);
            _lblDbStatus.Visible = !isConnected;

            if (_btnLogin != null) _btnLogin.Enabled = isConnected;
            if (_btnRegister != null) _btnRegister.Enabled = isConnected;
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
                        ShowError(Messages.NoDatabaseConnection);
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
            var tb = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
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
                    ShowError(Messages.NoDatabaseConnection);
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

                UserPreferences.RememberMe = _chkRememberMe.Checked;
                UserPreferences.RememberedUsername = _chkRememberMe.Checked ? username : null;

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
    }
}
