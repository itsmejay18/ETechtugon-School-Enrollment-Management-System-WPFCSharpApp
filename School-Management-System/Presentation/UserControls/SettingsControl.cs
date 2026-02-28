using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
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
        private TextBox _txtNetworkHost;
        private TextBox _txtNetworkPort;
        private TextBox _txtNetworkDbName;
        private Button _btnSaveConnection;

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

            var tabGeneral = BuildGeneralTab();
            _tabs.TabPages.Add(tabGeneral);

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

        private TabPage BuildGeneralTab()
        {
            var page = new TabPage("General")
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

            var termGroup = BuildTermGroup();
            var connectionGroup = BuildConnectionGroup();

            host.Controls.Add(connectionGroup);
            host.Controls.Add(termGroup);
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
                Height = 250,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground,
                Margin = new Padding(0, 12, 0, 0)
            };
            ThemeManager.StyleGroupBox(gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var hdrProfile = MakeHeaderLabel("Profile");
            var hdrHost = MakeHeaderLabel("Host");
            var hdrPort = MakeHeaderLabel("Port");
            var hdrDb = MakeHeaderLabel("Database");

            _txtLocalHost = MakeTextBox();
            _txtLocalPort = MakeTextBox();
            _txtLocalDbName = MakeTextBox();

            _txtNetworkHost = MakeTextBox();
            _txtNetworkPort = MakeTextBox();
            _txtNetworkDbName = MakeTextBox();

            _cmbConnectionMode = new ComboBox { Dock = DockStyle.Left, Width = 160, Font = ThemeFonts.Input, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeManager.StyleComboBox(_cmbConnectionMode);
            _cmbConnectionMode.Items.Add("Local");
            _cmbConnectionMode.Items.Add("Network");

            _btnSaveConnection = new Button { Text = "Save Connection Settings", Width = 220, Height = 32, Anchor = AnchorStyles.Left };
            ThemeManager.StyleButtonPrimary(_btnSaveConnection);
            _btnSaveConnection.Click += (s, e) => SaveConnectionSettings();

            var note = new Label
            {
                Text = "Tip: switch using Settings or command line (--db-mode=Local|Network). Open modules again to apply updates.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(hdrProfile, 0, 0);
            layout.Controls.Add(hdrHost, 1, 0);
            layout.Controls.Add(hdrPort, 2, 0);
            layout.Controls.Add(hdrDb, 3, 0);

            layout.Controls.Add(MakeLabel("Local"), 0, 1);
            layout.Controls.Add(_txtLocalHost, 1, 1);
            layout.Controls.Add(_txtLocalPort, 2, 1);
            layout.Controls.Add(_txtLocalDbName, 3, 1);

            layout.Controls.Add(MakeLabel("Network"), 0, 2);
            layout.Controls.Add(_txtNetworkHost, 1, 2);
            layout.Controls.Add(_txtNetworkPort, 2, 2);
            layout.Controls.Add(_txtNetworkDbName, 3, 2);

            layout.Controls.Add(MakeLabel("Active Mode"), 0, 3);
            layout.Controls.Add(_cmbConnectionMode, 1, 3);
            layout.SetColumnSpan(_cmbConnectionMode, 2);

            layout.Controls.Add(_btnSaveConnection, 1, 4);
            layout.SetColumnSpan(_btnSaveConnection, 2);

            layout.Controls.Add(note, 0, 5);
            layout.SetColumnSpan(note, 4);

            gb.Controls.Add(layout);
            return gb;
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
            if (selected == null)
            {
                return;
            }

            if (_loadedModuleTabs.Contains(selected))
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
                var control = factory();
                if (control == null)
                {
                    control = new PlaceholderControl("Module is not available.");
                }

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

        private static TextBox MakeTextBox()
        {
            var textBox = new TextBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleInput(textBox);
            return textBox;
        }

        private void LoadLookups()
        {
            if (_lookupService == null)
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
            if (_settingsService == null)
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
            var fallbackMode = ConfigurationManager.AppSettings["DbMode"] ?? "Local";

            try
            {
                if (_settingsService != null)
                {
                    var localProfile = _settingsService.GetDbProfile("Local");
                    var networkProfile = _settingsService.GetDbProfile("Network");
                    var mode = _settingsService.GetDbConnectionMode();

                    _txtLocalHost.Text = string.IsNullOrWhiteSpace(localProfile.Host) ? fallbackHost : localProfile.Host;
                    _txtLocalPort.Text = string.IsNullOrWhiteSpace(localProfile.Port) ? fallbackPort : localProfile.Port;
                    _txtLocalDbName.Text = string.IsNullOrWhiteSpace(localProfile.Database) ? fallbackDbName : localProfile.Database;

                    _txtNetworkHost.Text = string.IsNullOrWhiteSpace(networkProfile.Host) ? fallbackHost : networkProfile.Host;
                    _txtNetworkPort.Text = string.IsNullOrWhiteSpace(networkProfile.Port) ? fallbackPort : networkProfile.Port;
                    _txtNetworkDbName.Text = string.IsNullOrWhiteSpace(networkProfile.Database) ? fallbackDbName : networkProfile.Database;

                    var selectedMode = string.IsNullOrWhiteSpace(mode) ? fallbackMode : mode;
                    _cmbConnectionMode.SelectedItem = NormalizeMode(selectedMode);
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

            _txtLocalHost.Text = fallbackHost;
            _txtLocalPort.Text = fallbackPort;
            _txtLocalDbName.Text = fallbackDbName;
            _txtNetworkHost.Text = fallbackHost;
            _txtNetworkPort.Text = fallbackPort;
            _txtNetworkDbName.Text = fallbackDbName;
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
                var localHost = (_txtLocalHost.Text ?? string.Empty).Trim();
                var localPort = (_txtLocalPort.Text ?? string.Empty).Trim();
                var localDbName = (_txtLocalDbName.Text ?? string.Empty).Trim();

                var networkHost = (_txtNetworkHost.Text ?? string.Empty).Trim();
                var networkPort = (_txtNetworkPort.Text ?? string.Empty).Trim();
                var networkDbName = (_txtNetworkDbName.Text ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(localHost) || string.IsNullOrWhiteSpace(localDbName) ||
                    string.IsNullOrWhiteSpace(networkHost) || string.IsNullOrWhiteSpace(networkDbName))
                {
                    ThemedMessageBox.ShowError(this, "Host and database fields are required for Local and Network profiles.", "Settings");
                    return;
                }

                int localPortValue;
                int networkPortValue;
                if (!int.TryParse(localPort, out localPortValue) || localPortValue <= 0 ||
                    !int.TryParse(networkPort, out networkPortValue) || networkPortValue <= 0)
                {
                    ThemedMessageBox.ShowError(this, "Ports must be valid positive numbers.", "Settings");
                    return;
                }

                var mode = NormalizeMode(_cmbConnectionMode.SelectedItem == null ? null : _cmbConnectionMode.SelectedItem.ToString());
                if (string.IsNullOrWhiteSpace(mode))
                {
                    mode = "Local";
                }

                _settingsService.SetDbProfile("Local", localHost, localPort, localDbName);
                _settingsService.SetDbProfile("Network", networkHost, networkPort, networkDbName);
                _settingsService.SetDbConnectionMode(mode);
                ConnectionStringProvider.ResetDatabaseProfileCache();

                Environment.SetEnvironmentVariable("SMS_DB_MODE", mode, EnvironmentVariableTarget.Process);

                ThemedMessageBox.ShowInfo(
                    this,
                    "Connection profiles saved.\nNew database connections now use the selected mode.",
                    "Settings");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.SaveConnectionSettings", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private static string NormalizeMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return "Local";
            }

            if (string.Equals(mode.Trim(), "network", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode.Trim(), "lan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode.Trim(), "ip", StringComparison.OrdinalIgnoreCase))
            {
                return "Network";
            }

            return "Local";
        }
    }
}
