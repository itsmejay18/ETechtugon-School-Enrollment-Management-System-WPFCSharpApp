using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class SettingsControl : BaseUserControl
    {
        private readonly SystemSettingService _settingsService;
        private readonly LookupService _lookupService;
        private readonly DepartmentService _departmentService;
        private readonly CourseService _courseService;
        private readonly YearLevelService _yearLevelService;
        private readonly SectionService _sectionService;
        private readonly SubjectService _subjectService;
        private readonly CurriculumService _curriculumService;
        private readonly UserManagementService _userManagementService;
        private readonly bool _isAdmin;

        private ComboBox _cmbAcademicYear;
        private ComboBox _cmbSemester;
        private Button _btnSaveTerm;

        private ComboBox _cmbConnectionMode;
        private TextBox _txtLocalHost;
        private TextBox _txtLocalPort;
        private TextBox _txtLocalDbName;
        private TextBox _txtLocalUsername;
        private TextBox _txtLocalPassword;

        private TextBox _txtWiredHost;
        private TextBox _txtWiredPort;
        private TextBox _txtWiredDbName;
        private TextBox _txtWiredUsername;
        private TextBox _txtWiredPassword;

        private TextBox _txtWirelessHost;
        private TextBox _txtWirelessPort;
        private TextBox _txtWirelessDbName;
        private TextBox _txtWirelessUsername;
        private TextBox _txtWirelessPassword;

        private Button _btnSaveConnection;
        private Button _btnTestConnection;
        private Button _btnApplyRuntime;
        private Label _lblConnectionStatus;

        private TabControl _tabs;
        private readonly Dictionary<TabPage, Func<UserControl>> _moduleFactories = new Dictionary<TabPage, Func<UserControl>>();
        private readonly HashSet<TabPage> _loadedModuleTabs = new HashSet<TabPage>();

        public SettingsControl()
            : this(null, null, null, null, null, null, null, null, null, false)
        {
        }

        public SettingsControl(SystemSettingService settingsService, LookupService lookupService)
            : this(settingsService, lookupService, null, null, null, null, null, null, null, false)
        {
        }

        public SettingsControl(
            SystemSettingService settingsService,
            LookupService lookupService,
            DepartmentService departmentService,
            CourseService courseService,
            YearLevelService yearLevelService,
            SectionService sectionService,
            SubjectService subjectService,
            CurriculumService curriculumService,
            UserManagementService userManagementService,
            bool isAdmin)
        {
            _settingsService = settingsService;
            _lookupService = lookupService;
            _departmentService = departmentService;
            _courseService = courseService;
            _yearLevelService = yearLevelService;
            _sectionService = sectionService;
            _subjectService = subjectService;
            _curriculumService = curriculumService;
            _userManagementService = userManagementService;
            _isAdmin = isAdmin;

            InitializeComponent();
            LoadLookups();
            LoadCurrentTerm();
            LoadConnectionSettings();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label
            };
            _tabs.SelectedIndexChanged += (s, e) => EnsureSelectedModuleLoaded();

            _tabs.TabPages.Add(BuildSystemTab());
            _tabs.TabPages.Add(BuildDatabaseTab());

            AddModuleTab("Departments", () => new DepartmentControl(_departmentService));
            AddModuleTab("Courses", () => new CourseControl(_courseService, _departmentService));
            AddModuleTab("Year Levels", () => new YearLevelControl(_yearLevelService));
            AddModuleTab("Sections", () => new SectionControl(_sectionService, _courseService, _lookupService, _settingsService));
            AddModuleTab("Subjects", () => new SubjectControl(_subjectService, _courseService));
            AddModuleTab("Curriculum", () => new CurriculumControl(_curriculumService, _courseService, _lookupService));

            if (_isAdmin)
            {
                AddModuleTab("User Management", () => new UsersControl(_userManagementService));
            }
            else
            {
                AddModuleTab("User Management", () => new PlaceholderControl("Only administrators can manage users and passwords."));
            }

            Controls.Add(_tabs);
        }

        private TabPage BuildSystemTab()
        {
            var page = new TabPage("System")
            {
                BackColor = ThemeColors.Background
            };

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            host.Controls.Add(BuildTermGroup());
            page.Controls.Add(host);
            return page;
        }

        private TabPage BuildDatabaseTab()
        {
            var page = new TabPage("Database")
            {
                BackColor = ThemeColors.Background
            };

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            host.Controls.Add(BuildConnectionGroup());
            page.Controls.Add(host);
            return page;
        }

        private GroupBox BuildTermGroup()
        {
            var gb = new GroupBox
            {
                Text = "Global Term Settings",
                Dock = DockStyle.Top,
                Height = 176,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            _cmbAcademicYear = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbAcademicYear);

            _cmbSemester = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbSemester);

            _btnSaveTerm = new Button { Text = "Save Active Term", Width = 170, Height = 32, Anchor = AnchorStyles.Left };
            ThemeManager.StyleButtonPrimary(_btnSaveTerm);
            _btnSaveTerm.Click += (s, e) => SaveTermSettings();

            layout.Controls.Add(MakeLabel("Current School Year"), 0, 0);
            layout.Controls.Add(_cmbAcademicYear, 1, 0);
            layout.Controls.Add(MakeLabel("Current Semester"), 0, 1);
            layout.Controls.Add(_cmbSemester, 1, 1);
            layout.Controls.Add(_btnSaveTerm, 1, 2);

            gb.Controls.Add(layout);
            return gb;
        }

        private GroupBox BuildConnectionGroup()
        {
            var gb = new GroupBox
            {
                Text = "Database Connection Profiles",
                Dock = DockStyle.Top,
                Height = 340,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 9,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(MakeHeaderLabel("Profile"), 0, 0);
            layout.Controls.Add(MakeHeaderLabel("Host / IP"), 1, 0);
            layout.Controls.Add(MakeHeaderLabel("Port"), 2, 0);
            layout.Controls.Add(MakeHeaderLabel("Database"), 3, 0);
            layout.Controls.Add(MakeHeaderLabel("Username"), 4, 0);
            layout.Controls.Add(MakeHeaderLabel("Password"), 5, 0);

            _txtLocalHost = MakeTextBox();
            _txtLocalPort = MakeTextBox();
            _txtLocalDbName = MakeTextBox();
            _txtLocalUsername = MakeTextBox();
            _txtLocalPassword = MakeTextBox(true);

            _txtWiredHost = MakeTextBox();
            _txtWiredPort = MakeTextBox();
            _txtWiredDbName = MakeTextBox();
            _txtWiredUsername = MakeTextBox();
            _txtWiredPassword = MakeTextBox(true);

            _txtWirelessHost = MakeTextBox();
            _txtWirelessPort = MakeTextBox();
            _txtWirelessDbName = MakeTextBox();
            _txtWirelessUsername = MakeTextBox();
            _txtWirelessPassword = MakeTextBox(true);

            AddProfileRow(layout, 1, "Local", _txtLocalHost, _txtLocalPort, _txtLocalDbName, _txtLocalUsername, _txtLocalPassword);
            AddProfileRow(layout, 2, "Wired", _txtWiredHost, _txtWiredPort, _txtWiredDbName, _txtWiredUsername, _txtWiredPassword);
            AddProfileRow(layout, 3, "Wireless", _txtWirelessHost, _txtWirelessPort, _txtWirelessDbName, _txtWirelessUsername, _txtWirelessPassword);

            _cmbConnectionMode = new ComboBox { Dock = DockStyle.Left, Width = 180, Font = ThemeFonts.Input, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeManager.StyleComboBox(_cmbConnectionMode);
            _cmbConnectionMode.Items.Add("Local");
            _cmbConnectionMode.Items.Add("Wired");
            _cmbConnectionMode.Items.Add("Wireless");

            layout.Controls.Add(MakeLabel("Active Mode"), 0, 4);
            layout.Controls.Add(_cmbConnectionMode, 1, 4);
            layout.SetColumnSpan(_cmbConnectionMode, 2);

            var buttonHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };

            _btnSaveConnection = new Button { Text = "Save Profile", Width = 140, Height = 32, Margin = new Padding(0, 0, 10, 0) };
            ThemeManager.StyleButtonPrimary(_btnSaveConnection);
            _btnSaveConnection.Click += (s, e) => SaveConnectionSettings();

            _btnTestConnection = new Button { Text = "Test Connection", Width = 140, Height = 32, Margin = new Padding(0, 0, 10, 0) };
            ThemeManager.StyleButtonNeutral(_btnTestConnection);
            _btnTestConnection.Click += (s, e) => TestActiveConnection();

            _btnApplyRuntime = new Button { Text = "Apply Runtime", Width = 140, Height = 32, Margin = new Padding(0) };
            ThemeManager.StyleButtonPrimary(_btnApplyRuntime);
            _btnApplyRuntime.Click += (s, e) => ApplyRuntimeMode();

            buttonHost.Controls.Add(_btnSaveConnection);
            buttonHost.Controls.Add(_btnTestConnection);
            buttonHost.Controls.Add(_btnApplyRuntime);

            layout.Controls.Add(buttonHost, 1, 5);
            layout.SetColumnSpan(buttonHost, 5);

            _lblConnectionStatus = new Label
            {
                Text = "Status: Waiting for test.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(_lblConnectionStatus, 0, 6);
            layout.SetColumnSpan(_lblConnectionStatus, 6);

            var note = new Label
            {
                Text = "Tip: use Local on same PC, Wired for LAN cable, Wireless for Wi-Fi. You can also start with --db-mode=Local|Wired|Wireless.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 7);
            layout.SetColumnSpan(note, 6);

            gb.Controls.Add(layout);
            return gb;
        }
        private static void AddProfileRow(
            TableLayoutPanel layout,
            int row,
            string profile,
            TextBox host,
            TextBox port,
            TextBox db,
            TextBox user,
            TextBox password)
        {
            layout.Controls.Add(MakeLabel(profile), 0, row);
            layout.Controls.Add(host, 1, row);
            layout.Controls.Add(port, 2, row);
            layout.Controls.Add(db, 3, row);
            layout.Controls.Add(user, 4, row);
            layout.Controls.Add(password, 5, row);
        }

        private void AddModuleTab(string title, Func<UserControl> factory)
        {
            var page = new TabPage(title ?? "Module")
            {
                BackColor = ThemeColors.Background
            };

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                Padding = new Padding(6)
            };

            page.Controls.Add(host);
            _tabs.TabPages.Add(page);
            _moduleFactories[page] = factory;
        }

        private void EnsureSelectedModuleLoaded()
        {
            if (_tabs == null)
            {
                return;
            }

            var selected = _tabs.SelectedTab;
            if (selected == null || _loadedModuleTabs.Contains(selected))
            {
                return;
            }

            Func<UserControl> factory;
            if (!_moduleFactories.TryGetValue(selected, out factory) || factory == null)
            {
                return;
            }

            try
            {
                var control = factory() ?? (UserControl)new PlaceholderControl("Module is not available.");
                control.Dock = DockStyle.Fill;

                var host = selected.Controls.Count > 0 ? selected.Controls[0] as Panel : null;
                if (host != null)
                {
                    host.Controls.Clear();
                    host.Controls.Add(control);
                }
                else
                {
                    selected.Controls.Add(control);
                }

                _loadedModuleTabs.Add(selected);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.EnsureSelectedModuleLoaded", ex);
                ThemedMessageBox.ShowError(this, "Unable to open " + selected.Text + ".", "Settings");
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

        private static Label MakeHeaderLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static TextBox MakeTextBox(bool password = false)
        {
            var textBox = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            if (password)
            {
                textBox.UseSystemPasswordChar = true;
            }

            ThemeManager.StyleInput(textBox);
            return textBox;
        }

        private void LoadLookups()
        {
            if (_lookupService == null || _cmbAcademicYear == null || _cmbSemester == null)
            {
                return;
            }

            try
            {
                _cmbAcademicYear.DisplayMember = "Name";
                _cmbAcademicYear.ValueMember = "AcademicYearId";
                _cmbAcademicYear.DataSource = _lookupService.GetAcademicYears();

                _cmbSemester.DisplayMember = "Name";
                _cmbSemester.ValueMember = "SemesterId";
                _cmbSemester.DataSource = _lookupService.GetSemesters();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadLookups", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void LoadCurrentTerm()
        {
            if (_settingsService == null || _cmbAcademicYear == null || _cmbSemester == null)
            {
                return;
            }

            try
            {
                var term = _settingsService.GetActiveTerm();
                if (term.AcademicYearId.HasValue)
                {
                    _cmbAcademicYear.SelectedValue = term.AcademicYearId.Value;
                }

                if (term.SemesterId.HasValue)
                {
                    _cmbSemester.SelectedValue = term.SemesterId.Value;
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadCurrentTerm", ex);
            }
        }

        private void LoadConnectionSettings()
        {
            var fallbackHost = ConfigurationManager.AppSettings["DbHost"] ?? "localhost";
            var fallbackPort = ConfigurationManager.AppSettings["DbPort"] ?? "3306";
            var fallbackDbName = ConfigurationManager.AppSettings["DbName"] ?? "schoolmanagementsystem";
            var fallbackUser = ConfigurationManager.AppSettings["DbUser"] ?? "root";
            var fallbackPassword = ConfigurationManager.AppSettings["DbPassword"] ?? string.Empty;
            var fallbackMode = ConfigurationManager.AppSettings["DbMode"] ?? "Local";

            try
            {
                if (_settingsService != null)
                {
                    var local = _settingsService.GetDbProfile("Local");
                    var wired = _settingsService.GetDbProfile("Wired");
                    var wireless = _settingsService.GetDbProfile("Wireless");
                    var mode = _settingsService.GetDbConnectionMode();

                    SetProfileInputs(
                        "Local",
                        string.IsNullOrWhiteSpace(local.Host) ? fallbackHost : local.Host,
                        string.IsNullOrWhiteSpace(local.Port) ? fallbackPort : local.Port,
                        string.IsNullOrWhiteSpace(local.Database) ? fallbackDbName : local.Database,
                        string.IsNullOrWhiteSpace(local.Username) ? fallbackUser : local.Username,
                        string.IsNullOrWhiteSpace(local.Password) ? fallbackPassword : local.Password);

                    SetProfileInputs(
                        "Wired",
                        string.IsNullOrWhiteSpace(wired.Host) ? fallbackHost : wired.Host,
                        string.IsNullOrWhiteSpace(wired.Port) ? fallbackPort : wired.Port,
                        string.IsNullOrWhiteSpace(wired.Database) ? fallbackDbName : wired.Database,
                        string.IsNullOrWhiteSpace(wired.Username) ? fallbackUser : wired.Username,
                        string.IsNullOrWhiteSpace(wired.Password) ? fallbackPassword : wired.Password);

                    SetProfileInputs(
                        "Wireless",
                        string.IsNullOrWhiteSpace(wireless.Host) ? (string.IsNullOrWhiteSpace(wired.Host) ? fallbackHost : wired.Host) : wireless.Host,
                        string.IsNullOrWhiteSpace(wireless.Port) ? fallbackPort : wireless.Port,
                        string.IsNullOrWhiteSpace(wireless.Database) ? fallbackDbName : wireless.Database,
                        string.IsNullOrWhiteSpace(wireless.Username) ? fallbackUser : wireless.Username,
                        string.IsNullOrWhiteSpace(wireless.Password) ? fallbackPassword : wireless.Password);

                    var selectedMode = NormalizeMode(string.IsNullOrWhiteSpace(mode) ? fallbackMode : mode);
                    _cmbConnectionMode.SelectedItem = selectedMode;
                    if (_cmbConnectionMode.SelectedIndex < 0)
                    {
                        _cmbConnectionMode.SelectedItem = "Local";
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadConnectionSettings", ex);
            }

            SetProfileInputs("Local", fallbackHost, fallbackPort, fallbackDbName, fallbackUser, fallbackPassword);
            SetProfileInputs("Wired", fallbackHost, fallbackPort, fallbackDbName, fallbackUser, fallbackPassword);
            SetProfileInputs("Wireless", fallbackHost, fallbackPort, fallbackDbName, fallbackUser, fallbackPassword);

            _cmbConnectionMode.SelectedItem = NormalizeMode(fallbackMode);
            if (_cmbConnectionMode.SelectedIndex < 0)
            {
                _cmbConnectionMode.SelectedItem = "Local";
            }
        }
        private void SaveTermSettings()
        {
            if (_settingsService == null)
            {
                return;
            }

            try
            {
                var ayId = _cmbAcademicYear.SelectedValue == null ? 0 : Convert.ToInt32(_cmbAcademicYear.SelectedValue);
                var semId = _cmbSemester.SelectedValue == null ? 0 : Convert.ToInt32(_cmbSemester.SelectedValue);

                if (ayId <= 0 || semId <= 0)
                {
                    ThemedMessageBox.ShowError(this, "Please select both School Year and Semester.", "Settings");
                    return;
                }

                _settingsService.SetActiveTerm(ayId, semId);
                ThemedMessageBox.ShowInfo(this, "Active term updated successfully.", "Settings");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.SaveTermSettings", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void SaveConnectionSettings()
        {
            if (_settingsService == null)
            {
                ThemedMessageBox.ShowError(this, "Settings service is unavailable.", "Settings");
                return;
            }

            try
            {
                ValidateProfileInput("Local");
                ValidateProfileInput("Wired");
                ValidateProfileInput("Wireless");

                SaveProfile("Local");
                SaveProfile("Wired");
                SaveProfile("Wireless");

                var mode = NormalizeMode(_cmbConnectionMode.SelectedItem == null ? null : _cmbConnectionMode.SelectedItem.ToString());
                _settingsService.SetDbConnectionMode(mode);
                ConnectionStringProvider.ResetDatabaseProfileCache();

                Environment.SetEnvironmentVariable("SMS_DB_MODE", mode, EnvironmentVariableTarget.Process);

                _lblConnectionStatus.Text = "Status: Profiles saved. Active mode set to " + mode + ".";
                _lblConnectionStatus.ForeColor = ThemeColors.Success;
                ThemedMessageBox.ShowInfo(this, "Connection profiles saved successfully.", "Database");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.SaveConnectionSettings", ex);
                _lblConnectionStatus.Text = "Status: Save failed.";
                _lblConnectionStatus.ForeColor = ThemeColors.AccentDanger;
                ThemedMessageBox.ShowError(this, ex.Message, "Database");
            }
        }

        private void TestActiveConnection()
        {
            try
            {
                var mode = NormalizeMode(_cmbConnectionMode.SelectedItem == null ? null : _cmbConnectionMode.SelectedItem.ToString());
                ValidateProfileInput(mode);

                string host;
                string port;
                string db;
                string user;
                string password;
                GetProfileInputs(mode, out host, out port, out db, out user, out password);

                var builder = new MySqlConnectionStringBuilder(ConnectionStringProvider.GetByName(AppConstants.ConnectionStringName));
                builder.Server = host;
                builder.Database = db;
                builder.UserID = user;
                builder.Password = password;

                uint parsedPort;
                if (uint.TryParse(port, out parsedPort) && parsedPort > 0)
                {
                    builder.Port = parsedPort;
                }

                using (var conn = new MySqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                }

                _lblConnectionStatus.Text = "Status: Connected to " + host + ":" + port + " (" + mode + ").";
                _lblConnectionStatus.ForeColor = ThemeColors.Success;
                ThemedMessageBox.ShowInfo(this, "Connection test successful for " + mode + " profile.", "Database");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.TestActiveConnection", ex);
                _lblConnectionStatus.Text = "Status: Connection failed.";
                _lblConnectionStatus.ForeColor = ThemeColors.AccentDanger;
                ThemedMessageBox.ShowError(this, "Connection failed.\n" + BuildConnectionHint(ex.Message), "Database");
            }
        }

        private void ApplyRuntimeMode()
        {
            var mode = NormalizeMode(_cmbConnectionMode.SelectedItem == null ? null : _cmbConnectionMode.SelectedItem.ToString());
            Environment.SetEnvironmentVariable("SMS_DB_MODE", mode, EnvironmentVariableTarget.Process);
            ConnectionStringProvider.ResetDatabaseProfileCache();
            _lblConnectionStatus.Text = "Status: Runtime mode applied (" + mode + ").";
            _lblConnectionStatus.ForeColor = ThemeColors.Secondary;
            ThemedMessageBox.ShowInfo(this, "Runtime DB mode applied: " + mode + ".", "Database");
        }

        private void SaveProfile(string mode)
        {
            string host;
            string port;
            string db;
            string user;
            string password;
            GetProfileInputs(mode, out host, out port, out db, out user, out password);
            _settingsService.SetDbProfile(mode, host, port, db, user, password);
        }

        private void ValidateProfileInput(string mode)
        {
            string host;
            string port;
            string db;
            string user;
            string password;
            GetProfileInputs(mode, out host, out port, out db, out user, out password);

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(db) || string.IsNullOrWhiteSpace(user))
            {
                throw new InvalidOperationException(mode + " profile requires Host, Database, and Username.");
            }

            int parsedPort;
            if (!int.TryParse(port, out parsedPort) || parsedPort <= 0)
            {
                throw new InvalidOperationException(mode + " profile has invalid Port.");
            }
        }

        private void SetProfileInputs(string mode, string host, string port, string db, string user, string password)
        {
            switch (NormalizeMode(mode))
            {
                case "Wireless":
                    _txtWirelessHost.Text = host ?? string.Empty;
                    _txtWirelessPort.Text = port ?? string.Empty;
                    _txtWirelessDbName.Text = db ?? string.Empty;
                    _txtWirelessUsername.Text = user ?? string.Empty;
                    _txtWirelessPassword.Text = password ?? string.Empty;
                    break;
                case "Wired":
                    _txtWiredHost.Text = host ?? string.Empty;
                    _txtWiredPort.Text = port ?? string.Empty;
                    _txtWiredDbName.Text = db ?? string.Empty;
                    _txtWiredUsername.Text = user ?? string.Empty;
                    _txtWiredPassword.Text = password ?? string.Empty;
                    break;
                default:
                    _txtLocalHost.Text = host ?? string.Empty;
                    _txtLocalPort.Text = port ?? string.Empty;
                    _txtLocalDbName.Text = db ?? string.Empty;
                    _txtLocalUsername.Text = user ?? string.Empty;
                    _txtLocalPassword.Text = password ?? string.Empty;
                    break;
            }
        }

        private void GetProfileInputs(string mode, out string host, out string port, out string db, out string user, out string password)
        {
            switch (NormalizeMode(mode))
            {
                case "Wireless":
                    host = (_txtWirelessHost.Text ?? string.Empty).Trim();
                    port = (_txtWirelessPort.Text ?? string.Empty).Trim();
                    db = (_txtWirelessDbName.Text ?? string.Empty).Trim();
                    user = (_txtWirelessUsername.Text ?? string.Empty).Trim();
                    password = _txtWirelessPassword.Text ?? string.Empty;
                    return;
                case "Wired":
                    host = (_txtWiredHost.Text ?? string.Empty).Trim();
                    port = (_txtWiredPort.Text ?? string.Empty).Trim();
                    db = (_txtWiredDbName.Text ?? string.Empty).Trim();
                    user = (_txtWiredUsername.Text ?? string.Empty).Trim();
                    password = _txtWiredPassword.Text ?? string.Empty;
                    return;
                default:
                    host = (_txtLocalHost.Text ?? string.Empty).Trim();
                    port = (_txtLocalPort.Text ?? string.Empty).Trim();
                    db = (_txtLocalDbName.Text ?? string.Empty).Trim();
                    user = (_txtLocalUsername.Text ?? string.Empty).Trim();
                    password = _txtLocalPassword.Text ?? string.Empty;
                    return;
            }
        }

        private static string NormalizeMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return "Local";
            }

            var value = mode.Trim();
            if (string.Equals(value, "wireless", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "wifi", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "wlan", StringComparison.OrdinalIgnoreCase))
            {
                return "Wireless";
            }

            if (string.Equals(value, "wired", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "network", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "lan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "ip", StringComparison.OrdinalIgnoreCase))
            {
                return "Wired";
            }

            return "Local";
        }

        private static string BuildConnectionHint(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return "Unknown error.";
            }

            var lower = error.ToLowerInvariant();
            if (lower.Contains("access denied"))
            {
                return "Access denied. Verify username/password and grant remote host permission on Windows 10 MySQL.";
            }

            if (lower.Contains("unable to connect") || lower.Contains("actively refused"))
            {
                return "Target host is unreachable/refused. Verify Host/IP and same Wi-Fi/LAN. On the Windows 10 DB host, run DatabaseScripts\\Enable-RemoteMySQL-Windows.ps1 to open bind/firewall/grants for port 3306.";
            }

            if (lower.Contains("unknown database"))
            {
                return "Database name is wrong or missing on target server.";
            }

            return error.Length > 220 ? error.Substring(0, 220) + "..." : error;
        }
    }
}
