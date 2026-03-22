using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Forms
{
    public sealed class ConnectionProfileForm : BaseForm
    {
        private readonly IDictionary<string, ConnectionProfile> _profiles =
            new Dictionary<string, ConnectionProfile>(StringComparer.OrdinalIgnoreCase);

        private Button _btnModeOnline;
        private Button _btnModeLocal;
        private Button _btnModeNetwork;
        private Label _lblModeTitle;
        private Label _lblModeCaption;
        private Label _lblStatusTitle;
        private Label _lblStatus;
        private Panel _statusPanel;
        private Button _btnConnect;
        private Button _btnCancel;
        private string _currentMode;

        public ConnectionProfileForm()
        {
            Text = AppConstants.AppTitle + " - Database Connection";
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ShowInTaskbar = false;
            MinimumSize = new Size(1024, 680);
            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Maximized;

            BuildProfiles();
            BuildLayout();
            LoadInitialMode();
        }

        protected override void OnShown(EventArgs e)
        {
            ApplyFullscreenWorkingArea();
            base.OnShown(e);
        }

        private void BuildProfiles()
        {
            _profiles["Local"] = new ConnectionProfile
            {
                DisplayName = "Local",
                Mode = "Local",
                Host = ReadAppSetting("DbHost"),
                Port = ReadAppSetting("DbPort", "3306"),
                Database = ReadAppSetting("DbName", "schoolmanagementsystem"),
                Username = ReadAppSetting("DbUser"),
                Password = ReadAppSetting("DbPassword")
            };

            _profiles["Wired"] = new ConnectionProfile
            {
                DisplayName = "Network",
                Mode = "Wired",
                Host = ReadAppSetting("DbHostWired", ReadAppSetting("DbHostNetwork", ReadAppSetting("DbHost"))),
                Port = ReadAppSetting("DbPortWired", ReadAppSetting("DbPortNetwork", ReadAppSetting("DbPort", "3306"))),
                Database = ReadAppSetting("DbNameWired", ReadAppSetting("DbNameNetwork", ReadAppSetting("DbName", "schoolmanagementsystem"))),
                Username = ReadAppSetting("DbUserWired", ReadAppSetting("DbUser")),
                Password = ReadAppSetting("DbPasswordWired", ReadAppSetting("DbPassword"))
            };

            _profiles["Online"] = new ConnectionProfile
            {
                DisplayName = "Online",
                Mode = "Online",
                Host = ReadAppSetting("DbHostOnline", ReadAppSetting("DbHost")),
                Port = ReadAppSetting("DbPortOnline", ReadAppSetting("DbPort", "3306")),
                Database = ReadAppSetting("DbNameOnline", ReadAppSetting("DbName", "schoolmanagementsystem")),
                Username = ReadAppSetting("DbUserOnline", ReadAppSetting("DbUser")),
                Password = ReadAppSetting("DbPasswordOnline", ReadAppSetting("DbPassword"))
            };

            var startupMode = NormalizeMode(ReadEnvironmentOrAppSetting("SMS_DB_MODE", "DbMode"));
            var activeProfile = GetProfile(startupMode);
            if (activeProfile != null)
            {
                var envHost = Environment.GetEnvironmentVariable("SMS_DB_HOST");
                var envPort = Environment.GetEnvironmentVariable("SMS_DB_PORT");
                var envDatabase = Environment.GetEnvironmentVariable("SMS_DB_NAME");
                var envUser = Environment.GetEnvironmentVariable("SMS_DB_USER");
                var envPassword = Environment.GetEnvironmentVariable("SMS_DB_PASSWORD");

                if (!string.IsNullOrWhiteSpace(envHost)) activeProfile.Host = envHost.Trim();
                if (!string.IsNullOrWhiteSpace(envPort)) activeProfile.Port = envPort.Trim();
                if (!string.IsNullOrWhiteSpace(envDatabase)) activeProfile.Database = envDatabase.Trim();
                if (!string.IsNullOrWhiteSpace(envUser)) activeProfile.Username = envUser.Trim();
                if (!string.IsNullOrWhiteSpace(envPassword)) activeProfile.Password = envPassword;
            }
        }

        private void BuildLayout()
        {
            SuspendLayout();

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28),
                BackColor = ThemeColors.Background,
                AutoScroll = true
            };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleCardPanel(card);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Primary,
                Padding = new Padding(26),
                AutoScroll = true
            };

            var leftStack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 7,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            leftStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));

            var leftCaption = new Label
            {
                Dock = DockStyle.Fill,
                Text = "DATABASE STARTUP",
                Font = ThemeFonts.CaptionStrong,
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var leftHeader = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Choose the server the app should use.",
                Font = ThemeFonts.Header,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var leftBody = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Online is already filled with your Hostinger settings. Switch to Local or Network only when you need another MySQL server.",
                Font = ThemeFonts.Label,
                ForeColor = Color.FromArgb(210, 255, 255, 255),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 4, 10, 0)
            };
            var profileCaption = new Label
            {
                Dock = DockStyle.Fill,
                Text = "CONNECTION PROFILES",
                Font = ThemeFonts.CaptionStrong,
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var modeList = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            modeList.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            modeList.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            modeList.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            modeList.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));

            _btnModeOnline = CreateModeButton("Online", "Hostinger internet database", "Online");
            _btnModeLocal = CreateModeButton("Local", "Database on this computer", "Local");
            _btnModeNetwork = CreateModeButton("Network", "LAN or Wi-Fi MySQL server", "Wired");
            modeList.Controls.Add(_btnModeOnline, 0, 0);
            modeList.Controls.Add(_btnModeLocal, 0, 1);
            modeList.Controls.Add(_btnModeNetwork, 0, 2);

            var selectedCaption = new Label
            {
                Dock = DockStyle.Fill,
                Text = "SELECTED PROFILE",
                Font = ThemeFonts.CaptionStrong,
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var selectedPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 255, 255, 255),
                Padding = new Padding(16)
            };
            _lblModeTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = ThemeFonts.SubHeader,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _lblModeCaption = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                TextAlign = ContentAlignment.TopLeft
            };
            selectedPanel.Controls.Add(_lblModeCaption);
            selectedPanel.Controls.Add(_lblModeTitle);

            leftStack.Controls.Add(leftCaption, 0, 0);
            leftStack.Controls.Add(leftHeader, 0, 1);
            leftStack.Controls.Add(leftBody, 0, 2);
            leftStack.Controls.Add(profileCaption, 0, 3);
            leftStack.Controls.Add(modeList, 0, 4);
            leftStack.Controls.Add(selectedCaption, 0, 5);
            leftStack.Controls.Add(selectedPanel, 0, 6);
            leftPanel.Controls.Add(leftStack);

            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 24, 24, 24),
                BackColor = Color.Transparent
            };

            var rightStack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0)
            };
            rightStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
            rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 320F));

            var rightContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var actionHost = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 0)
            };

            var header = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Database Connection",
                Font = ThemeFonts.AuthTitle,
                ForeColor = ThemeColors.Primary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var subHeader = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Select which saved server profile to use before continuing to the login screen.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.TopLeft
            };

            var highlightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(22, ThemeColors.Secondary.R, ThemeColors.Secondary.G, ThemeColors.Secondary.B),
                Padding = new Padding(16, 14, 16, 14),
                Margin = new Padding(0, 6, 0, 0)
            };
            var highlightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            highlightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            highlightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            highlightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var highlightCaption = new Label
            {
                Dock = DockStyle.Fill,
                Text = "RECOMMENDED",
                Font = ThemeFonts.CaptionStrong,
                ForeColor = ThemeColors.Secondary,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var highlightBody = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Connection details are now managed in Settings. This startup screen only chooses which saved profile the app should use.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            highlightLayout.Controls.Add(highlightCaption, 0, 0);
            highlightLayout.Controls.Add(highlightBody, 0, 1);
            highlightPanel.Controls.Add(highlightLayout);

            var summaryCard = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.Transparent,
                MinimumSize = new Size(0, 320)
            };
            ThemeManager.StyleCardPanel(summaryCard);

            var summaryStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            summaryStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            summaryStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            summaryStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            summaryStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            summaryStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));

            var summaryTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Startup Server Selection",
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var summarySubtitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Pick a saved profile here. Edit host, port, database, username, or password later in Settings.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var summaryLead = new Label
            {
                Dock = DockStyle.Fill,
                Text = "The selected profile will be applied for this run of the app. If you need to update database credentials or server addresses, open Settings after login.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.TopLeft
            };
            var summaryNotes = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Profiles:\nOnline = Hostinger / internet database\nLocal = MySQL on this same computer\nNetwork = another MySQL server on LAN or Wi-Fi\n\nThe app will verify the selected profile when you click Continue.",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.TopLeft
            };

            summaryStack.Controls.Add(summaryTitle, 0, 0);
            summaryStack.Controls.Add(summarySubtitle, 0, 1);
            summaryStack.Controls.Add(summaryLead, 0, 2);
            summaryStack.Controls.Add(summaryNotes, 0, 3);
            summaryCard.Controls.Add(summaryStack);

            _statusPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = ThemeColors.SurfaceAlt,
                Margin = new Padding(0, 14, 0, 0),
                Padding = new Padding(16, 14, 16, 12)
            };
            var statusLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _lblStatusTitle = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.CaptionStrong,
                ForeColor = ThemeColors.MutedText,
                Text = "Connection Tip",
                TextAlign = ContentAlignment.MiddleLeft
            };
            _lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.TopLeft,
                Text = "Tip: Online connects directly to Hostinger using the prefilled values."
            };
            statusLayout.Controls.Add(_lblStatusTitle, 0, 0);
            statusLayout.Controls.Add(_lblStatus, 0, 1);
            _statusPanel.Controls.Add(statusLayout);

            var actionRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0)
            };
            actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 146F));
            actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124F));
            actionRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _btnConnect = new Button
            {
                Text = "Continue",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 0)
            };
            ThemeManager.StyleButtonPrimary(_btnConnect);
            _btnConnect.Click += (s, e) => ContinueWithSelectedConnection();

            _btnCancel = new Button
            {
                Text = "Cancel",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0)
            };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            AcceptButton = _btnConnect;
            CancelButton = _btnCancel;

            actionRow.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 0, 0);
            actionRow.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 1, 0);
            actionRow.Controls.Add(_btnCancel, 2, 0);
            actionRow.Controls.Add(_btnConnect, 3, 0);

            rightStack.Controls.Add(header, 0, 0);
            rightStack.Controls.Add(subHeader, 0, 1);
            rightStack.Controls.Add(highlightPanel, 0, 2);
            rightStack.Controls.Add(summaryCard, 0, 3);
            rightContentPanel.Controls.Add(_statusPanel);
            actionHost.Controls.Add(actionRow);
            rightContentPanel.Controls.Add(rightStack);
            rightPanel.Controls.Add(rightContentPanel);
            rightPanel.Controls.Add(actionHost);

            shell.Controls.Add(leftPanel, 0, 0);
            shell.Controls.Add(rightPanel, 1, 0);
            card.Controls.Add(shell);
            root.Controls.Add(card);
            Controls.Add(root);

            ResumeLayout(true);
        }

        private void LoadInitialMode()
        {
            var mode = NormalizeMode(ReadEnvironmentOrAppSetting("SMS_DB_MODE", "DbMode"));
            if (string.IsNullOrWhiteSpace(mode))
            {
                mode = "Online";
            }

            ChangeMode(mode);
        }

        private void ApplyFullscreenWorkingArea()
        {
            var screen = Screen.FromControl(this);
            if (screen == null)
            {
                return;
            }

            MaximizedBounds = screen.WorkingArea;
            Bounds = screen.WorkingArea;
            WindowState = FormWindowState.Maximized;
        }

        private void ChangeMode(string mode)
        {
            mode = NormalizeMode(mode);
            _currentMode = mode;
            UpdateModeButtons(mode);
            UpdateModeSummary(mode);

            if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Online is prefilled for Hostinger and will connect directly if the values are correct.", ThemeColors.Secondary);
            }
            else if (string.Equals(mode, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Network mode is for LAN/Wi-Fi IP connections inside your local network.", ThemeColors.MutedText);
            }
            else
            {
                SetStatus("Local mode uses the database server running on the same PC.", ThemeColors.MutedText);
            }
        }

        private void ContinueWithSelectedConnection()
        {
            if (!TestCurrentConnection(true))
            {
                return;
            }

            ApplyRuntimeSelection();
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool TestCurrentConnection(bool silentOnSuccess)
        {
            try
            {
                var profile = GetProfile(_currentMode);
                ValidateProfile(profile);
                var builder = BuildConnectionString(profile);

                using (var connection = new MySqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                }

                SetStatus("Connected to " + profile.Host + ":" + profile.Port + " using " + GetProfile(_currentMode).DisplayName + ".", ThemeColors.Success);
                if (!silentOnSuccess)
                {
                    ShowInfo("Connection test successful.", "Database Connection");
                }

                return true;
            }
            catch (Exception ex)
            {
                SetStatus(BuildConnectionHint(ex.Message), ThemeColors.AccentDanger);
                if (!silentOnSuccess)
                {
                    ShowError("Connection failed.\n" + BuildConnectionHint(ex.Message), "Database Connection");
                }

                return false;
            }
        }

        private void ApplyRuntimeSelection()
        {
            var profile = GetProfile(_currentMode);

            Environment.SetEnvironmentVariable("SMS_DB_MODE", profile.Mode, EnvironmentVariableTarget.Process);
            SetProcessVariable("SMS_DB_HOST", profile.Host);
            SetProcessVariable("SMS_DB_PORT", profile.Port);
            SetProcessVariable("SMS_DB_NAME", profile.Database);
            SetProcessVariable("SMS_DB_USER", profile.Username);
            SetProcessVariable("SMS_DB_PASSWORD", profile.Password);

            if (string.Equals(profile.Mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                SetProcessVariable("SMS_DB_SSL_MODE", ReadAppSetting("DbSslModeOnline", "Required"));
            }
            else
            {
                SetProcessVariable("SMS_DB_SSL_MODE", null);
            }

            SetProcessVariable("SMS_DB_SSL_CA_PATH", null);
            ConnectionStringProvider.ResetDatabaseProfileCache();
        }

        private static void ValidateProfile(ConnectionProfile profile)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("Connection profile not found.");
            }

            if (string.IsNullOrWhiteSpace(profile.Host))
            {
                throw new InvalidOperationException("Host is required.");
            }

            if (string.IsNullOrWhiteSpace(profile.Port))
            {
                throw new InvalidOperationException("Port is required.");
            }

            uint port;
            if (!uint.TryParse(profile.Port.Trim(), out port) || port == 0)
            {
                throw new InvalidOperationException("Port must be a valid number.");
            }

            if (string.IsNullOrWhiteSpace(profile.Database))
            {
                throw new InvalidOperationException("Database name is required.");
            }

            if (string.IsNullOrWhiteSpace(profile.Username))
            {
                throw new InvalidOperationException("Username is required.");
            }
        }

        private MySqlConnectionStringBuilder BuildConnectionString(ConnectionProfile profile)
        {
            uint port;
            var builder = new MySqlConnectionStringBuilder
            {
                Server = (profile.Host ?? string.Empty).Trim(),
                Database = (profile.Database ?? string.Empty).Trim(),
                UserID = (profile.Username ?? string.Empty).Trim(),
                Password = profile.Password ?? string.Empty,
                AllowPublicKeyRetrieval = true
            };

            if (uint.TryParse(profile.Port, out port) && port > 0)
            {
                builder.Port = port;
            }

            builder.SslMode = string.Equals(profile.Mode, "Online", StringComparison.OrdinalIgnoreCase)
                ? MySqlSslMode.Required
                : MySqlSslMode.Preferred;

            return builder;
        }

        private ConnectionProfile GetProfile(string mode)
        {
            ConnectionProfile profile;
            return _profiles.TryGetValue(NormalizeMode(mode), out profile) ? profile : null;
        }

        private Button CreateModeButton(string title, string caption, string mode)
        {
            var button = new Button
            {
                Text = title + Environment.NewLine + caption,
                Dock = DockStyle.Fill,
                Height = 64,
                Margin = new Padding(0, 0, 0, 6),
                Tag = mode
            };
            ThemeManager.StyleButtonNeutral(button);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 14, 0);
            button.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
            button.Click += (s, e) => ChangeMode(Convert.ToString(button.Tag));
            return button;
        }

        private void UpdateModeButtons(string mode)
        {
            SetModeButtonState(_btnModeOnline, string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase));
            SetModeButtonState(_btnModeLocal, string.Equals(mode, "Local", StringComparison.OrdinalIgnoreCase));
            SetModeButtonState(_btnModeNetwork, string.Equals(mode, "Wired", StringComparison.OrdinalIgnoreCase));
        }

        private static void SetModeButtonState(Button button, bool isActive)
        {
            if (button == null)
            {
                return;
            }

            if (isActive)
            {
                ThemeManager.StyleButtonPrimary(button);
            }
            else
            {
                ThemeManager.StyleButtonNeutral(button);
            }

            button.Dock = DockStyle.Fill;
            button.Height = 64;
            button.Margin = new Padding(0, 0, 0, 6);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 14, 0);
            button.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
        }

        private void UpdateModeSummary(string mode)
        {
            if (_lblModeTitle == null || _lblModeCaption == null)
            {
                return;
            }

            if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                _lblModeTitle.Text = "Online / Hostinger";
                _lblModeCaption.Text = "Direct internet connection with the prefilled Hostinger server, database, username, and password.";
                return;
            }

            if (string.Equals(mode, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                _lblModeTitle.Text = "Network / LAN";
                _lblModeCaption.Text = "Use this for another MySQL server on the same network. Replace the host with the server IP or machine name.";
                return;
            }

            _lblModeTitle.Text = "Local Machine";
            _lblModeCaption.Text = "Use this when MySQL is installed on the same PC as the application and the database is hosted locally.";
        }

        private void SetStatus(string message, Color color)
        {
            _lblStatus.Text = message ?? string.Empty;
            _lblStatus.ForeColor = color;
            if (_lblStatusTitle != null)
            {
                _lblStatusTitle.ForeColor = color;
                _lblStatusTitle.Text = BuildStatusTitle(color);
            }

            if (_statusPanel != null)
            {
                _statusPanel.BackColor = Color.FromArgb(22, color.R, color.G, color.B);
            }
        }

        private static string BuildStatusTitle(Color color)
        {
            if (color.ToArgb() == ThemeColors.Success.ToArgb())
            {
                return "Connection Successful";
            }

            if (color.ToArgb() == ThemeColors.AccentDanger.ToArgb())
            {
                return "Connection Issue";
            }

            if (color.ToArgb() == ThemeColors.Secondary.ToArgb())
            {
                return "Recommended Profile";
            }

            return "Connection Tip";
        }

        private static void SetProcessVariable(string name, string value)
        {
            Environment.SetEnvironmentVariable(
                name,
                string.IsNullOrWhiteSpace(value) ? null : value,
                EnvironmentVariableTarget.Process);
        }

        private static string NormalizeMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return "Local";
            }

            var value = mode.Trim();
            if (string.Equals(value, "wired", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "network", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "lan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "ip", StringComparison.OrdinalIgnoreCase))
            {
                return "Wired";
            }

            if (string.Equals(value, "online", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "hostinger", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "cloud", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "internet", StringComparison.OrdinalIgnoreCase))
            {
                return "Online";
            }

            return "Local";
        }

        private static string BuildConnectionHint(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return "Unknown connection error.";
            }

            var lower = error.ToLowerInvariant();
            if (lower.Contains("access denied"))
            {
                return "Access denied. Verify username/password and remote host grants.";
            }

            if (lower.Contains("unable to connect") || lower.Contains("actively refused"))
            {
                return "Host unreachable or refused. Check the host, port, and server availability.";
            }

            if (lower.Contains("unknown database"))
            {
                return "Database name is wrong or missing on the target server.";
            }

            return error.Length > 220 ? error.Substring(0, 220) + "..." : error;
        }

        private static string ReadEnvironmentOrAppSetting(string envVarName, string appSettingKey)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue.Trim();
            }

            return ReadAppSetting(appSettingKey);
        }

        private static string ReadAppSetting(string key, string fallbackValue = "")
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallbackValue : value.Trim();
        }

        private sealed class ConnectionProfile
        {
            public string DisplayName { get; set; }
            public string Mode { get; set; }
            public string Host { get; set; }
            public string Port { get; set; }
            public string Database { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
