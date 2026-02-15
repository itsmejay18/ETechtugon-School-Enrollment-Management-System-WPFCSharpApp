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
        private Panel _root;
        private Panel _card;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Label _lblDbStatus;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private CheckBox _chkRememberMe;
        private Button _btnLogin;
        private Button _btnExit;

        private DatabaseHelper _db;
        private AuthService _authService;

        public LoginForm()
        {
            Text = AppConstants.AppTitle + " - Login";
            Width = 980;
            Height = 620;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

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

            _txtUsername.Focus();
        }

        private void TryInitServices()
        {
            try
            {
                _db = DatabaseHelper.FromConfig();
                IUserData userData = new UserData(_db);
                _authService = new AuthService(userData);

                string error;
                if (!_db.TestConnection(out error))
                {
                    _lblDbStatus.Text = Messages.NoDatabaseConnection;
                    _lblDbStatus.Visible = true;
                    _btnLogin.Enabled = false;
                }
                else
                {
                    _lblDbStatus.Visible = false;
                    _btnLogin.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _lblDbStatus.Text = Messages.NoDatabaseConnection;
                _lblDbStatus.Visible = true;
                _btnLogin.Enabled = false;
                School_Management_System.DataLayer.Logging.FileLogger.LogError("LoginForm.TryInitServices", ex);
            }
        }

        private void InitializeComponent()
        {
            _root = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            Controls.Add(_root);

            _card = new Panel { Size = new Size(460, 480), BackColor = ThemeColors.CardBackground };
            ThemeManager.StyleCardPanel(_card);
            _root.Controls.Add(_card);

            _root.Resize += (s, e) => CenterCard();
            CenterCard();

            var logo = new PictureBox
            {
                Size = new Size(56, 56),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconFactory.CreateCircleIcon(ThemeColors.Secondary, "S", 56),
                Location = new Point(18, 18)
            };

            _lblTitle = new Label
            {
                Text = AppConstants.AppTitle,
                Font = ThemeFonts.Header,
                ForeColor = ThemeColors.Text,
                AutoSize = false,
                Location = new Point(84, 18),
                Size = new Size(340, 30)
            };

            _lblSubtitle = new Label
            {
                Text = "Sign in to continue",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                AutoSize = false,
                Location = new Point(84, 48),
                Size = new Size(340, 20)
            };

            _lblDbStatus = new Label
            {
                Text = Messages.NoDatabaseConnection,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.AccentDanger,
                AutoSize = false,
                Location = new Point(18, 86),
                Size = new Size(420, 32),
                Visible = false
            };

            var lblUsername = new Label { Text = "Username", Font = ThemeFonts.Label, AutoSize = true, Location = new Point(18, 132) };
            _txtUsername = new TextBox { Location = new Point(18, 154), Size = new Size(420, 28) };
            ThemeManager.StyleInput(_txtUsername);

            var lblPassword = new Label { Text = "Password", Font = ThemeFonts.Label, AutoSize = true, Location = new Point(18, 204) };
            _txtPassword = new TextBox { Location = new Point(18, 226), Size = new Size(420, 28), UseSystemPasswordChar = true };
            ThemeManager.StyleInput(_txtPassword);

            _chkShowPassword = new CheckBox
            {
                Text = "Show password",
                AutoSize = true,
                Location = new Point(18, 262)
            };
            _chkShowPassword.CheckedChanged += (s, e) => _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;

            _chkRememberMe = new CheckBox
            {
                Text = "Remember me",
                AutoSize = true,
                Location = new Point(160, 262)
            };

            _btnLogin = new Button { Text = "Login", Location = new Point(18, 312), Size = new Size(420, 38) };
            ThemeManager.StyleButtonPrimary(_btnLogin);
            _btnLogin.Height = 38;
            _btnLogin.Click += (s, e) => DoLogin();

            _btnExit = new Button { Text = "Exit", Location = new Point(18, 360), Size = new Size(420, 34) };
            ThemeManager.StyleButtonNeutral(_btnExit);
            _btnExit.Click += (s, e) => Close();

            _txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    DoLogin();
                    e.Handled = true;
                }
            };

            _card.Controls.Add(logo);
            _card.Controls.Add(_lblTitle);
            _card.Controls.Add(_lblSubtitle);
            _card.Controls.Add(_lblDbStatus);
            _card.Controls.Add(lblUsername);
            _card.Controls.Add(_txtUsername);
            _card.Controls.Add(lblPassword);
            _card.Controls.Add(_txtPassword);
            _card.Controls.Add(_chkShowPassword);
            _card.Controls.Add(_chkRememberMe);
            _card.Controls.Add(_btnLogin);
            _card.Controls.Add(_btnExit);
        }

        private void CenterCard()
        {
            if (_card == null) return;
            _card.Left = (_root.ClientSize.Width - _card.Width) / 2;
            _card.Top = (_root.ClientSize.Height - _card.Height) / 2;
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

