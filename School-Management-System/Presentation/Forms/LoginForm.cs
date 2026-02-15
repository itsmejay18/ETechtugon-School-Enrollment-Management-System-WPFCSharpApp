using System;
using System.Drawing;
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
    public sealed class LoginForm : BaseForm
    {
        private Panel _sidebar;
        private Panel _content;
        private Panel _header;
        private Panel _body;
        private Panel _card;
        private Panel _viewHost;
        private Panel _loginPanel;
        private Panel _registerPanel;

        private Button _btnNavLogin;
        private Button _btnNavRegister;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Label _lblDbStatus;

        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private CheckBox _chkRememberMe;
        private Button _btnLogin;
        private Button _btnCreateAccount;
        private Button _btnExit;

        private TextBox _txtRegUsername;
        private TextBox _txtRegDisplayName;
        private ComboBox _cmbRegRole;
        private TextBox _txtRegPassword;
        private TextBox _txtRegConfirmPassword;
        private CheckBox _chkRegShowPassword;

        private DatabaseHelper _db;
        private AuthService _authService;
        private UserManagementService _userManagementService;
        private Button _activeNavButton;

        public LoginForm()
        {
            Text = AppConstants.AppTitle + " - Access";
            Width = 1100;
            Height = 700;
            MinimumSize = new Size(1024, 640);

            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            TryInitServices();

            _chkRememberMe.Checked = UserPreferences.RememberMe;
            if (_chkRememberMe.Checked)
            {
                _txtUsername.Text = UserPreferences.RememberedUsername ?? string.Empty;
                _txtUsername.SelectionStart = _txtUsername.TextLength;
            }

            ShowAuthView(false);
        }

        private void TryInitServices()
        {
            try
            {
                _db = DatabaseHelper.FromConfig();
                IUserData userData = new UserData(_db);
                _authService = new AuthService(userData);
                IUserManagementData userManagementData = new UserManagementData(_db);
                _userManagementService = new UserManagementService(userManagementData);

                string error;
                if (!_db.TestConnection(out error))
                {
                    SetConnectionState(false, string.IsNullOrWhiteSpace(error) ? Messages.NoDatabaseConnection : error);
                }
                else
                {
                    SetConnectionState(true, null);
                }
            }
            catch (Exception ex)
            {
                SetConnectionState(false, Messages.NoDatabaseConnection);
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.TryInitServices", ex);
            }
        }

        private void InitializeComponent()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = ThemeColors.SidebarBackground };
            _content = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            Controls.Add(_content);
            Controls.Add(_sidebar);

            BuildSidebar();
            BuildContent();
        }

        private void BuildSidebar()
        {
            var brand = new Panel { Dock = DockStyle.Top, Height = 76, Padding = new Padding(12), BackColor = ThemeColors.SidebarBackground };
            var brandIcon = new PictureBox
            {
                Size = new Size(44, 44),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconFactory.CreateCircleIcon(ThemeColors.Secondary, "S", 44),
                Location = new Point(12, 14)
            };
            var brandText = new Label
            {
                Text = "School\nManagement",
                ForeColor = Color.White,
                Font = ThemeFonts.Sidebar,
                AutoSize = false,
                Location = new Point(62, 14),
                Size = new Size(160, 44)
            };
            brand.Controls.Add(brandIcon);
            brand.Controls.Add(brandText);
            _sidebar.Controls.Add(brand);

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 6, 0, 6),
                BackColor = ThemeColors.SidebarBackground
            };

            _btnNavLogin = new Button { Text = "  Login", Width = _sidebar.Width - 2, Height = 42 };
            _btnNavLogin.Image = IconFactory.CreateCircleIcon(ThemeColors.Success, "L", 24);
            StyleAuthSidebarButton(_btnNavLogin);
            _btnNavLogin.Click += (s, e) => ShowAuthView(false);

            _btnNavRegister = new Button { Text = "  Register", Width = _sidebar.Width - 2, Height = 42 };
            _btnNavRegister.Image = IconFactory.CreateCircleIcon(ThemeColors.Secondary, "R", 24);
            StyleAuthSidebarButton(_btnNavRegister);
            _btnNavRegister.Click += (s, e) => ShowAuthView(true);

            nav.Controls.Add(_btnNavLogin);
            nav.Controls.Add(_btnNavRegister);
            _sidebar.Controls.Add(nav);

            var exitPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12, 10, 12, 10), BackColor = ThemeColors.SidebarBackground };
            _btnExit = new Button { Text = "  Exit", Dock = DockStyle.Fill };
            _btnExit.Image = IconFactory.CreateCircleIcon(ThemeColors.AccentDanger, "X", 24);
            ThemeManager.StyleSidebarButton(_btnExit);
            _btnExit.Click += (s, e) => Close();
            exitPanel.Controls.Add(_btnExit);
            _sidebar.Controls.Add(exitPanel);
        }

        private void BuildContent()
        {
            _header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 12, 18, 12) };
            _lblTitle = new Label
            {
                Dock = DockStyle.Left,
                Width = 280,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Text = "User Login",
                TextAlign = ContentAlignment.MiddleLeft
            };
            _lblSubtitle = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Text = "Sign in to continue to dashboard",
                TextAlign = ContentAlignment.MiddleRight
            };
            _header.Controls.Add(_lblSubtitle);
            _header.Controls.Add(_lblTitle);
            _content.Controls.Add(_header);

            _body = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(18) };
            _content.Controls.Add(_body);

            _card = new Panel { Size = new Size(580, 460), BackColor = ThemeColors.CardBackground };
            ThemeManager.StyleCardPanel(_card);
            _body.Controls.Add(_card);

            _body.Resize += (s, e) => CenterCard();
            CenterCard();

            var statusHost = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 12, 18, 0) };
            _lblDbStatus = new Label
            {
                Text = Messages.NoDatabaseConnection,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.AccentDanger,
                Dock = DockStyle.Fill,
                Visible = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusHost.Controls.Add(_lblDbStatus);

            _viewHost = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground };
            _card.Controls.Add(_viewHost);
            _card.Controls.Add(statusHost);

            _loginPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 12, 18, 18) };
            _registerPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 12, 18, 18) };
            _viewHost.Controls.Add(_registerPanel);
            _viewHost.Controls.Add(_loginPanel);

            BuildLoginView();
            BuildRegisterView();
            ShowAuthView(false);
        }

        private void BuildLoginView()
        {
            var gb = new GroupBox
            {
                Text = "Login Credentials",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(14, 20, 14, 12),
                BackColor = ThemeColors.CardBackground
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(4, 8, 4, 4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
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
            _chkShowPassword = new CheckBox { Text = "Show password", AutoSize = true, Margin = new Padding(0, 6, 20, 0) };
            _chkShowPassword.CheckedChanged += (s, e) => _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
            _chkRememberMe = new CheckBox { Text = "Remember me", AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            options.Controls.Add(_chkShowPassword);
            options.Controls.Add(_chkRememberMe);
            layout.Controls.Add(options, 0, 2);
            layout.SetColumnSpan(options, 2);

            var btnRow = new Panel { Dock = DockStyle.Fill };
            var right = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = 290 };

            _btnLogin = new Button { Text = "Login", Width = 130, Height = 34, Margin = new Padding(8, 0, 0, 0) };
            ThemeManager.StyleButtonPrimary(_btnLogin);
            _btnLogin.Click += (s, e) => DoLogin();

            var btnGoRegister = new Button { Text = "Register", Width = 140, Height = 34 };
            ThemeManager.StyleButtonNeutral(btnGoRegister);
            btnGoRegister.Click += (s, e) => ShowAuthView(true);

            right.Controls.Add(_btnLogin);
            right.Controls.Add(btnGoRegister);
            btnRow.Controls.Add(right);
            layout.Controls.Add(btnRow, 0, 3);
            layout.SetColumnSpan(btnRow, 2);

            var note = new Label
            {
                Text = "Use your assigned account to access the dashboard.",
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
            _loginPanel.Controls.Add(gb);
        }

        private void BuildRegisterView()
        {
            var gb = new GroupBox
            {
                Text = "Register Account",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(14, 20, 14, 12),
                BackColor = ThemeColors.CardBackground
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(4, 8, 4, 4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtRegUsername = MakeTextBox();
            _txtRegDisplayName = MakeTextBox();
            _txtRegPassword = MakeTextBox();
            _txtRegPassword.UseSystemPasswordChar = true;
            _txtRegConfirmPassword = MakeTextBox();
            _txtRegConfirmPassword.UseSystemPasswordChar = true;

            _cmbRegRole = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbRegRole);
            _cmbRegRole.Items.AddRange(new object[] { AppConstants.Roles.Registrar, AppConstants.Roles.Faculty });
            if (_cmbRegRole.Items.Count > 0) _cmbRegRole.SelectedIndex = 0;

            layout.Controls.Add(MakeLabel("Username *"), 0, 0);
            layout.Controls.Add(_txtRegUsername, 1, 0);
            layout.Controls.Add(MakeLabel("Display Name"), 0, 1);
            layout.Controls.Add(_txtRegDisplayName, 1, 1);
            layout.Controls.Add(MakeLabel("Role *"), 0, 2);
            layout.Controls.Add(_cmbRegRole, 1, 2);
            layout.Controls.Add(MakeLabel("Password *"), 0, 3);
            layout.Controls.Add(_txtRegPassword, 1, 3);
            layout.Controls.Add(MakeLabel("Confirm *"), 0, 4);
            layout.Controls.Add(_txtRegConfirmPassword, 1, 4);

            _chkRegShowPassword = new CheckBox { Text = "Show password", AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            _chkRegShowPassword.CheckedChanged += (s, e) =>
            {
                var visible = _chkRegShowPassword.Checked;
                _txtRegPassword.UseSystemPasswordChar = !visible;
                _txtRegConfirmPassword.UseSystemPasswordChar = !visible;
            };
            layout.Controls.Add(_chkRegShowPassword, 1, 5);

            var buttonRow = new Panel { Dock = DockStyle.Fill };
            var right = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = 350 };

            _btnCreateAccount = new Button { Text = "Create Account", Width = 150, Height = 34, Margin = new Padding(8, 0, 0, 0) };
            ThemeManager.StyleButtonPrimary(_btnCreateAccount);
            _btnCreateAccount.Click += (s, e) => DoRegister();

            var btnBackToLogin = new Button { Text = "Back to Login", Width = 150, Height = 34 };
            ThemeManager.StyleButtonNeutral(btnBackToLogin);
            btnBackToLogin.Click += (s, e) => ShowAuthView(false);

            right.Controls.Add(_btnCreateAccount);
            right.Controls.Add(btnBackToLogin);
            buttonRow.Controls.Add(right);
            layout.Controls.Add(buttonRow, 0, 6);
            layout.SetColumnSpan(buttonRow, 2);

            var note = new Label
            {
                Text = "Admin accounts are managed from the Users module.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.TopLeft
            };
            layout.Controls.Add(note, 0, 7);
            layout.SetColumnSpan(note, 2);

            _txtRegConfirmPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    DoRegister();
                    e.Handled = true;
                }
            };

            gb.Controls.Add(layout);
            _registerPanel.Controls.Add(gb);
        }

        private void CenterCard()
        {
            if (_card == null || _body == null) return;
            _card.Left = (_body.ClientSize.Width - _card.Width) / 2;
            _card.Top = (_body.ClientSize.Height - _card.Height) / 2;
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

        private void StyleAuthSidebarButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = ThemeColors.SidebarBackground;
            button.ForeColor = Color.White;
            button.Font = ThemeFonts.Sidebar;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 12, 0);
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (s, e) =>
            {
                if (!ReferenceEquals(button, _activeNavButton))
                {
                    button.BackColor = ThemeColors.SidebarHover;
                }
            };

            button.MouseLeave += (s, e) => ApplyNavState();
        }

        private void ApplyNavState()
        {
            if (_btnNavLogin != null)
            {
                _btnNavLogin.BackColor = ReferenceEquals(_activeNavButton, _btnNavLogin) ? ThemeColors.SidebarHover : ThemeColors.SidebarBackground;
            }

            if (_btnNavRegister != null)
            {
                _btnNavRegister.BackColor = ReferenceEquals(_activeNavButton, _btnNavRegister) ? ThemeColors.SidebarHover : ThemeColors.SidebarBackground;
            }
        }

        private void ShowAuthView(bool showRegister)
        {
            if (_loginPanel == null || _registerPanel == null) return;

            _loginPanel.Visible = !showRegister;
            _registerPanel.Visible = showRegister;

            _activeNavButton = showRegister ? _btnNavRegister : _btnNavLogin;
            ApplyNavState();

            if (showRegister)
            {
                _lblTitle.Text = "Register Account";
                _lblSubtitle.Text = "Create an account to access the dashboard";
                _txtRegUsername.Focus();
            }
            else
            {
                _lblTitle.Text = "User Login";
                _lblSubtitle.Text = "Sign in to continue to dashboard";
                _txtUsername.Focus();
            }
        }

        private void SetConnectionState(bool isConnected, string errorMessage)
        {
            _lblDbStatus.Text = isConnected ? string.Empty : (string.IsNullOrWhiteSpace(errorMessage) ? Messages.NoDatabaseConnection : errorMessage);
            _lblDbStatus.Visible = !isConnected;

            if (_btnLogin != null) _btnLogin.Enabled = isConnected;
            if (_btnCreateAccount != null) _btnCreateAccount.Enabled = isConnected;
            if (_btnNavRegister != null) _btnNavRegister.Enabled = isConnected;
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

            var username = (_txtRegUsername.Text ?? string.Empty).Trim();
            var displayName = (_txtRegDisplayName.Text ?? string.Empty).Trim();
            var role = Convert.ToString(_cmbRegRole.SelectedItem) ?? string.Empty;
            var password = _txtRegPassword.Text ?? string.Empty;
            var confirmPassword = _txtRegConfirmPassword.Text ?? string.Empty;

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
                _btnCreateAccount.Enabled = false;
                _userManagementService.Create(username, password, role, displayName, true);

                ShowInfo("Account created successfully. You can now sign in.", "Registration Complete");

                _txtUsername.Text = username;
                _txtPassword.Text = string.Empty;
                _txtRegPassword.Text = string.Empty;
                _txtRegConfirmPassword.Text = string.Empty;
                _chkRegShowPassword.Checked = false;

                ShowAuthView(false);
                _txtPassword.Focus();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.DoRegister", ex);
                ShowError(Messages.UnexpectedError);
            }
            finally
            {
                UseWaitCursor = false;
                _btnCreateAccount.Enabled = true;
            }
        }
    }
}
