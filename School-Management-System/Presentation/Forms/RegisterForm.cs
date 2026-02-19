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
        private Button _btnBackLogin;
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
            _root = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            _root.Paint += DrawRootBackground;
            Controls.Add(_root);

            _hero = BuildHeroPanel();
            _authHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(56, 36, 56, 36) };

            _card = new Panel { Size = new Size(610, 510), BackColor = ThemeColors.CardBackground };
            ThemeManager.StyleCardPanel(_card);
            _authHost.Controls.Add(_card);
            _authHost.Resize += (s, e) => CenterCard();

            BuildRegisterCardContent();

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
                using (var brush = new LinearGradientBrush(panel.ClientRectangle, ColorTranslator.FromHtml("#1E2E3F"), ThemeColors.Primary, 140f))
                {
                    e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                }
            };

            var logo = new PictureBox
            {
                Size = new Size(74, 74),
                Location = new Point(34, 42),
                Image = IconFactory.CreateCircleIcon(ThemeColors.Success, IconKind.Register, 74),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            var title = new Label
            {
                Text = "Create a New\nUser Account",
                Location = new Point(34, 132),
                Size = new Size(310, 72),
                Font = ThemeFonts.Header,
                ForeColor = Color.White
            };

            var subtitle = new Label
            {
                Text = "Add registrar or faculty users to start managing academics with secured role access.",
                Location = new Point(34, 218),
                Size = new Size(332, 74),
                Font = ThemeFonts.Label,
                ForeColor = ColorTranslator.FromHtml("#D6E4F0")
            };

            var badge = new Panel { Location = new Point(34, 318), Size = new Size(316, 84), BackColor = Color.FromArgb(47, 108, 140), Padding = new Padding(12, 10, 12, 10) };
            UiHelper.ApplyRoundedCorners(badge, 6);

            var badgeIcon = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 44,
                Image = IconFactory.CreateGlyphIcon(IconKind.Users, 22, Color.White),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            var badgeText = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Admin accounts stay in\nUsers module policies",
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

        private void BuildRegisterCardContent()
        {
            var cardTop = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = ThemeColors.CardBackground, Padding = new Padding(20, 16, 20, 12) };
            var tabs = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            _btnBackLogin = new Button { Text = "Login", Width = 130, Height = 34 };
            ThemeManager.StyleButtonNeutral(_btnBackLogin);
            _btnBackLogin.Click += (s, e) => Close();

            var btnRegisterTab = new Button { Text = "Register", Width = 130, Height = 34, Enabled = false, Margin = new Padding(10, 0, 0, 0) };
            ThemeManager.StyleButtonPrimary(btnRegisterTab);

            tabs.Controls.Add(_btnBackLogin);
            tabs.Controls.Add(btnRegisterTab);

            var subtitle = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "Create account credentials for dashboard access",
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
                Text = "Registration Details",
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
                RowCount = 8,
                Padding = new Padding(4, 12, 4, 4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtUsername = MakeTextBox();
            _txtDisplayName = MakeTextBox();
            _txtPassword = MakeTextBox();
            _txtPassword.UseSystemPasswordChar = true;
            _txtConfirmPassword = MakeTextBox();
            _txtConfirmPassword.UseSystemPasswordChar = true;

            _cmbRole = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbRole);
            _cmbRole.Items.AddRange(new object[] { AppConstants.Roles.Registrar, AppConstants.Roles.Faculty });
            if (_cmbRole.Items.Count > 0) _cmbRole.SelectedIndex = 0;

            layout.Controls.Add(MakeLabel("Username *"), 0, 0);
            layout.Controls.Add(_txtUsername, 1, 0);
            layout.Controls.Add(MakeLabel("Display Name"), 0, 1);
            layout.Controls.Add(_txtDisplayName, 1, 1);
            layout.Controls.Add(MakeLabel("Role *"), 0, 2);
            layout.Controls.Add(_cmbRole, 1, 2);
            layout.Controls.Add(MakeLabel("Password *"), 0, 3);
            layout.Controls.Add(_txtPassword, 1, 3);
            layout.Controls.Add(MakeLabel("Confirm Password *"), 0, 4);
            layout.Controls.Add(_txtConfirmPassword, 1, 4);

            _chkShowPassword = new CheckBox { Text = "Show password", AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            _chkShowPassword.CheckedChanged += (s, e) =>
            {
                var visible = _chkShowPassword.Checked;
                _txtPassword.UseSystemPasswordChar = !visible;
                _txtConfirmPassword.UseSystemPasswordChar = !visible;
            };
            layout.Controls.Add(_chkShowPassword, 1, 5);

            var actions = new Panel { Dock = DockStyle.Fill };
            var right = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = 300 };

            _btnCreate = new Button { Text = "Create Account", Width = 150, Height = 34, Margin = new Padding(10, 0, 0, 0) };
            _btnCreate.Image = IconFactory.CreateGlyphIcon(IconKind.Register, 16, Color.White);
            _btnCreate.TextImageRelation = TextImageRelation.ImageBeforeText;
            ThemeManager.StyleButtonPrimary(_btnCreate);
            _btnCreate.Click += (s, e) => DoRegister();

            _btnCancel = new Button { Text = "Cancel", Width = 140, Height = 34 };
            _btnCancel.Image = IconFactory.CreateGlyphIcon(IconKind.Logout, 16, Color.White);
            _btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            ThemeManager.StyleButtonDanger(_btnCancel);
            _btnCancel.Click += (s, e) => Close();

            right.Controls.Add(_btnCreate);
            right.Controls.Add(_btnCancel);
            actions.Controls.Add(right);
            layout.Controls.Add(actions, 0, 6);
            layout.SetColumnSpan(actions, 2);

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

            gb.Controls.Add(layout);

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

            if (_btnCreate != null) _btnCreate.Enabled = isConnected;
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
            var tb = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleInput(tb);
            return tb;
        }
    }
}
