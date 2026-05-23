using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Session;
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
        private TextBox _txtTuitionPerUnit;
        private TextBox _txtMiscellaneousFee;
        private TextBox _txtRegistrationFee;
        private TextBox _txtLaboratoryFee;
        private Button _btnSaveBillingSettings;

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

        private TextBox _txtOnlineHost;
        private TextBox _txtOnlinePort;
        private TextBox _txtOnlineDbName;
        private TextBox _txtOnlineUsername;
        private TextBox _txtOnlinePassword;
        private ComboBox _cmbOnlineSslMode;
        private TextBox _txtOnlineSslCaPath;
        private Button _btnBrowseOnlineSslCaPath;

        private Button _btnSaveConnection;
        private Button _btnTestConnection;
        private Button _btnApplyRuntime;
        private Label _lblConnectionStatus;

        private ComboBox _cmbBackupType;
        private TextBox _txtBackupDirectory;
        private Button _btnBrowseBackupDirectory;
        private Button _btnOpenBackupDirectory;
        private Button _btnSaveBackupSettings;
        private Button _btnCreateBackup;
        private Button _btnRestoreBackup;
        private Button _btnCancelBackupOperation;
        private Label _lblBackupStatus;
        private CancellationTokenSource _backupOperationCts;

        private DateTimePicker _dtLogFrom;
        private DateTimePicker _dtLogTo;
        private TextBox _txtLogUser;
        private ComboBox _cmbLogAction;
        private NumericUpDown _numLogLimit;
        private Button _btnRefreshLogs;
        private Button _btnExportLogs;
        private DataGridView _gridUserLogs;
        private IList<School_Management_System.Models.ActivityLog> _currentLogs = new List<School_Management_System.Models.ActivityLog>();

        private TabControl _tabs;
        private readonly Dictionary<TabPage, Func<UserControl>> _moduleFactories = new Dictionary<TabPage, Func<UserControl>>();
        private readonly HashSet<TabPage> _loadedModuleTabs = new HashSet<TabPage>();
        private readonly ActivityLogService _activityLogService;
        private readonly DatabaseBackupService _databaseBackupService;

        public SettingsControl()
            : this(null, null, null, null, null, null, null, null, null, null, null, false)
        {
        }

        public SettingsControl(SystemSettingService settingsService, LookupService lookupService)
            : this(settingsService, lookupService, null, null, null, null, null, null, null, null, null, false)
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
            ActivityLogService activityLogService,
            DatabaseBackupService databaseBackupService,
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
            _activityLogService = activityLogService;
            _databaseBackupService = databaseBackupService;
            _isAdmin = isAdmin;

            InitializeComponent();
            LoadLookups();
            LoadCurrentTerm();
            LoadBillingSettings();
            if (_isAdmin)
            {
                LoadConnectionSettings();
            }
            LoadBackupSettings();
            LoadUserLogs();
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

            host.Controls.Add(BuildTuitionGroup());
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

            if (!_isAdmin)
            {
                host.Controls.Add(BuildDatabaseAdminOnlyNotice());
                page.Controls.Add(host);
                return page;
            }

            host.Controls.Add(BuildLogGroup());
            host.Controls.Add(BuildBackupGroup());
            host.Controls.Add(BuildConnectionGroup());
            page.Controls.Add(host);
            return page;
        }

        private Control BuildDatabaseAdminOnlyNotice()
        {
            var gb = new GroupBox
            {
                Text = "Database Access",
                Dock = DockStyle.Top,
                Height = 150,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(gb);

            var message = new Label
            {
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Database connection profiles, backup tools, and audit logs are available to administrators only. This keeps server settings out of the day-to-day enrollment workflow."
            };

            gb.Controls.Add(message);
            return gb;
        }

        private GroupBox BuildTermGroup()
        {
            var gb = new GroupBox
            {
                Text = "Global Term Settings",
                Dock = DockStyle.Top,
                Height = 206,
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            _cmbAcademicYear = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbAcademicYear);

            _cmbSemester = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbSemester);

            _btnSaveTerm = new Button { Text = "Save Active Term", Width = 170, Height = 34, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
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

        private GroupBox BuildTuitionGroup()
        {
            var gb = new GroupBox
            {
                Text = "Enrollment Billing Defaults",
                Dock = DockStyle.Top,
                Height = 318,
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
                RowCount = 6,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _txtTuitionPerUnit = MakeTextBox();
            _txtMiscellaneousFee = MakeTextBox();
            _txtRegistrationFee = MakeTextBox();
            _txtLaboratoryFee = MakeTextBox();

            _btnSaveBillingSettings = new Button
            {
                Text = "Save Billing Defaults",
                Width = 176,
                Height = 34,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 0)
            };
            ThemeManager.StyleButtonPrimary(_btnSaveBillingSettings);
            _btnSaveBillingSettings.Click += (s, e) => SaveBillingSettings();

            var note = new Label
            {
                Text = "These values are used by Enrollment assessment and COR printing. Leave laboratory fee at 0 if not used.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = false
            };

            layout.Controls.Add(MakeLabel("Tuition Per Unit"), 0, 0);
            layout.Controls.Add(_txtTuitionPerUnit, 1, 0);
            layout.Controls.Add(MakeLabel("Miscellaneous Fee"), 0, 1);
            layout.Controls.Add(_txtMiscellaneousFee, 1, 1);
            layout.Controls.Add(MakeLabel("Registration Fee"), 0, 2);
            layout.Controls.Add(_txtRegistrationFee, 1, 2);
            layout.Controls.Add(MakeLabel("Laboratory Fee"), 0, 3);
            layout.Controls.Add(_txtLaboratoryFee, 1, 3);
            layout.Controls.Add(note, 0, 4);
            layout.SetColumnSpan(note, 2);
            layout.Controls.Add(_btnSaveBillingSettings, 1, 5);

            gb.Controls.Add(layout);
            return gb;
        }

        private GroupBox BuildConnectionGroup()
        {
            var gb = new GroupBox
            {
                Text = "Database Connection Profiles",
                Dock = DockStyle.Top,
                Height = 434,
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
                RowCount = 11,
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

            _txtOnlineHost = MakeTextBox();
            _txtOnlinePort = MakeTextBox();
            _txtOnlineDbName = MakeTextBox();
            _txtOnlineUsername = MakeTextBox();
            _txtOnlinePassword = MakeTextBox(true);

            AddProfileRow(layout, 1, "Local", _txtLocalHost, _txtLocalPort, _txtLocalDbName, _txtLocalUsername, _txtLocalPassword);
            AddProfileRow(layout, 2, "Wired", _txtWiredHost, _txtWiredPort, _txtWiredDbName, _txtWiredUsername, _txtWiredPassword);
            AddProfileRow(layout, 3, "Wireless", _txtWirelessHost, _txtWirelessPort, _txtWirelessDbName, _txtWirelessUsername, _txtWirelessPassword);
            AddProfileRow(layout, 4, "Online", _txtOnlineHost, _txtOnlinePort, _txtOnlineDbName, _txtOnlineUsername, _txtOnlinePassword);

            _cmbConnectionMode = new ComboBox { Dock = DockStyle.Left, Width = 180, Font = ThemeFonts.Input, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeManager.StyleComboBox(_cmbConnectionMode);
            _cmbConnectionMode.Items.Add("Local");
            _cmbConnectionMode.Items.Add("Wired");
            _cmbConnectionMode.Items.Add("Wireless");
            _cmbConnectionMode.Items.Add("Online");

            layout.Controls.Add(MakeLabel("Active Mode"), 0, 5);
            layout.Controls.Add(_cmbConnectionMode, 1, 5);
            layout.SetColumnSpan(_cmbConnectionMode, 2);

            _cmbOnlineSslMode = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeManager.StyleComboBox(_cmbOnlineSslMode);
            _cmbOnlineSslMode.Items.Add("Required");
            _cmbOnlineSslMode.Items.Add("VerifyCA");
            _cmbOnlineSslMode.Items.Add("VerifyFull");
            _cmbOnlineSslMode.Items.Add("Preferred");
            _cmbOnlineSslMode.SelectedItem = "Required";

            _txtOnlineSslCaPath = MakeTextBox();

            _btnBrowseOnlineSslCaPath = new Button { Text = "Browse CA", Width = 96, Height = 30, Margin = new Padding(0) };
            ThemeManager.StyleButtonNeutral(_btnBrowseOnlineSslCaPath);
            _btnBrowseOnlineSslCaPath.Click += (s, e) => ChooseOnlineSslCaPath();

            layout.Controls.Add(MakeLabel("Online TLS"), 0, 6);
            layout.Controls.Add(_cmbOnlineSslMode, 1, 6);
            layout.Controls.Add(_txtOnlineSslCaPath, 3, 6);
            layout.SetColumnSpan(_txtOnlineSslCaPath, 2);
            layout.Controls.Add(_btnBrowseOnlineSslCaPath, 5, 6);

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

            layout.Controls.Add(buttonHost, 1, 7);
            layout.SetColumnSpan(buttonHost, 5);

            _lblConnectionStatus = new Label
            {
                Text = "Status: Waiting for test.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(_lblConnectionStatus, 0, 8);
            layout.SetColumnSpan(_lblConnectionStatus, 6);

            var note = new Label
            {
                Text = "Tip: use Local on same PC, Wired/Wireless for LAN/Wi-Fi, and Online for Hostinger internet DB. CLI: --db-mode=Local|Wired|Wireless|Online.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 9);
            layout.SetColumnSpan(note, 6);

            gb.Controls.Add(layout);
            return gb;
        }

        private GroupBox BuildBackupGroup()
        {
            var gb = new GroupBox
            {
                Text = "Backup and Restore",
                Dock = DockStyle.Top,
                Height = 230,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 6,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _cmbBackupType = new ComboBox { Dock = DockStyle.Left, Width = 190, Font = ThemeFonts.Input, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeManager.StyleComboBox(_cmbBackupType);
            _cmbBackupType.Items.Add("Full");
            _cmbBackupType.Items.Add("Incremental");
            _cmbBackupType.Items.Add("Differential");
            _cmbBackupType.SelectedIndex = 0;

            _txtBackupDirectory = MakeTextBox();

            _btnBrowseBackupDirectory = new Button { Text = "Browse", Width = 96, Height = 30 };
            ThemeManager.StyleButtonNeutral(_btnBrowseBackupDirectory);
            _btnBrowseBackupDirectory.Click += (s, e) => ChooseBackupDirectory();

            _btnOpenBackupDirectory = new Button { Text = "Open", Width = 96, Height = 30 };
            ThemeManager.StyleButtonNeutral(_btnOpenBackupDirectory);
            _btnOpenBackupDirectory.Click += (s, e) => OpenBackupDirectory();

            _btnSaveBackupSettings = new Button { Text = "Save Backup Settings", Width = 178, Height = 32, Margin = new Padding(0, 0, 10, 0) };
            ThemeManager.StyleButtonPrimary(_btnSaveBackupSettings);
            _btnSaveBackupSettings.Click += (s, e) => SaveBackupSettings();

            _btnCreateBackup = new Button { Text = "Create Backup", Width = 148, Height = 32, Margin = new Padding(0, 0, 10, 0) };
            ThemeManager.StyleButtonPrimary(_btnCreateBackup);
            _btnCreateBackup.Click += async (s, e) => await CreateBackupNowAsync();

            _btnRestoreBackup = new Button { Text = "Restore Backup", Width = 148, Height = 32, Margin = new Padding(0) };
            ThemeManager.StyleButtonDanger(_btnRestoreBackup);
            _btnRestoreBackup.Click += async (s, e) => await RestoreBackupFromFileAsync();

            _btnCancelBackupOperation = new Button { Text = "Cancel", Width = 108, Height = 32, Margin = new Padding(10, 0, 0, 0), Enabled = false };
            ThemeManager.StyleButtonNeutral(_btnCancelBackupOperation);
            _btnCancelBackupOperation.Click += (s, e) => CancelActiveBackupOperation();

            var actionHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };
            actionHost.Controls.Add(_btnSaveBackupSettings);
            actionHost.Controls.Add(_btnCreateBackup);
            actionHost.Controls.Add(_btnRestoreBackup);
            actionHost.Controls.Add(_btnCancelBackupOperation);

            _lblBackupStatus = new Label
            {
                Text = "Status: Backup system ready.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var note = new Label
            {
                Text = "Incremental uses the latest backup as baseline. Differential uses the latest full backup baseline.",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(MakeLabel("Backup Type"), 0, 0);
            layout.Controls.Add(_cmbBackupType, 1, 0);
            layout.SetColumnSpan(_cmbBackupType, 2);

            layout.Controls.Add(MakeLabel("Backup Folder"), 0, 1);
            layout.Controls.Add(_txtBackupDirectory, 1, 1);
            layout.SetColumnSpan(_txtBackupDirectory, 2);
            layout.Controls.Add(_btnBrowseBackupDirectory, 3, 1);
            layout.Controls.Add(_btnOpenBackupDirectory, 4, 1);

            layout.Controls.Add(actionHost, 1, 2);
            layout.SetColumnSpan(actionHost, 4);

            layout.Controls.Add(_lblBackupStatus, 0, 3);
            layout.SetColumnSpan(_lblBackupStatus, 5);

            layout.Controls.Add(note, 0, 4);
            layout.SetColumnSpan(note, 5);

            gb.Controls.Add(layout);
            return gb;
        }

        private GroupBox BuildLogGroup()
        {
            var gb = new GroupBox
            {
                Text = "User Logs",
                Dock = DockStyle.Top,
                Height = 420,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(gb);

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.CardBackground
            };

            var filters = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 74,
                ColumnCount = 10,
                RowCount = 2,
                Padding = new Padding(8, 6, 8, 6)
            };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
            filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            _dtLogFrom = new DateTimePicker { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Format = DateTimePickerFormat.Short };
            ThemeManager.StyleDatePicker(_dtLogFrom);
            _dtLogFrom.Value = DateTime.Today.AddDays(-30);

            _dtLogTo = new DateTimePicker { Dock = DockStyle.Fill, Font = ThemeFonts.Input, Format = DateTimePickerFormat.Short };
            ThemeManager.StyleDatePicker(_dtLogTo);
            _dtLogTo.Value = DateTime.Today;

            _cmbLogAction = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeManager.StyleComboBox(_cmbLogAction);
            _cmbLogAction.Items.Add("All");
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.LoginSuccess);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.LoginFailed);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.Logout);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.BackupFull);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.BackupIncremental);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.BackupDifferential);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.RestoreDatabase);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.BackupError);
            _cmbLogAction.Items.Add(AppConstants.ActivityActions.RestoreError);
            _cmbLogAction.SelectedIndex = 0;

            _txtLogUser = MakeTextBox();

            _numLogLimit = new NumericUpDown
            {
                Dock = DockStyle.Left,
                Width = 70,
                Minimum = 10,
                Maximum = 5000,
                Value = 300
            };
            ThemeManager.StyleInput(_numLogLimit);

            _btnRefreshLogs = new Button { Text = "Refresh", Width = 86, Height = 30 };
            ThemeManager.StyleButtonPrimary(_btnRefreshLogs);
            _btnRefreshLogs.Click += (s, e) => LoadUserLogs();

            _btnExportLogs = new Button { Text = "Export", Width = 86, Height = 30 };
            ThemeManager.StyleButtonNeutral(_btnExportLogs);
            _btnExportLogs.Click += (s, e) => ExportLogsToCsv();

            filters.Controls.Add(MakeLabel("From"), 0, 0);
            filters.Controls.Add(_dtLogFrom, 1, 0);
            filters.Controls.Add(MakeLabel("To"), 2, 0);
            filters.Controls.Add(_dtLogTo, 3, 0);
            filters.Controls.Add(MakeLabel("Action"), 4, 0);
            filters.Controls.Add(_cmbLogAction, 5, 0);
            filters.Controls.Add(MakeLabel("Max"), 6, 0);
            filters.Controls.Add(_numLogLimit, 7, 0);
            filters.Controls.Add(_btnRefreshLogs, 8, 0);
            filters.Controls.Add(_btnExportLogs, 9, 0);

            filters.Controls.Add(MakeLabel("User"), 0, 1);
            filters.Controls.Add(_txtLogUser, 1, 1);
            filters.SetColumnSpan(_txtLogUser, 7);

            _gridUserLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false
            };
            ThemeManager.StyleDataGrid(_gridUserLogs);

            _gridUserLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedAt",
                HeaderText = "Timestamp",
                DataPropertyName = "CreatedAt",
                FillWeight = 22
            });
            _gridUserLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "User",
                HeaderText = "User",
                DataPropertyName = "UserDisplay",
                FillWeight = 18
            });
            _gridUserLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Action",
                HeaderText = "Action",
                DataPropertyName = "Action",
                FillWeight = 14
            });
            _gridUserLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Entity",
                HeaderText = "Entity",
                DataPropertyName = "Entity",
                FillWeight = 12
            });
            _gridUserLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MachineName",
                HeaderText = "Machine",
                DataPropertyName = "MachineName",
                FillWeight = 12
            });
            _gridUserLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Details",
                HeaderText = "Details",
                DataPropertyName = "Details",
                FillWeight = 32
            });

            host.Controls.Add(_gridUserLogs);
            host.Controls.Add(filters);
            gb.Controls.Add(host);
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

        private void LoadBillingSettings()
        {
            if (_settingsService == null || _txtTuitionPerUnit == null)
            {
                return;
            }

            try
            {
                _txtTuitionPerUnit.Text = _settingsService.GetDecimal(AppConstants.SettingKeys.TuitionPerUnit, 650m).ToString("0.00");
                _txtMiscellaneousFee.Text = _settingsService.GetDecimal(AppConstants.SettingKeys.MiscellaneousFee, 1850m).ToString("0.00");
                _txtRegistrationFee.Text = _settingsService.GetDecimal(AppConstants.SettingKeys.RegistrationFee, 350m).ToString("0.00");
                _txtLaboratoryFee.Text = _settingsService.GetDecimal(AppConstants.SettingKeys.LaboratoryFee, 0m).ToString("0.00");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadBillingSettings", ex);
            }
        }

        private void SaveBillingSettings()
        {
            if (_settingsService == null)
            {
                return;
            }

            decimal tuitionPerUnit;
            decimal miscFee;
            decimal registrationFee;
            decimal laboratoryFee;

            if (!decimal.TryParse(_txtTuitionPerUnit.Text, out tuitionPerUnit) || tuitionPerUnit < 0m)
            {
                ThemedMessageBox.ShowError(this, "Enter a valid non-negative Tuition Per Unit.", "Billing");
                return;
            }

            if (!decimal.TryParse(_txtMiscellaneousFee.Text, out miscFee) || miscFee < 0m)
            {
                ThemedMessageBox.ShowError(this, "Enter a valid non-negative Miscellaneous Fee.", "Billing");
                return;
            }

            if (!decimal.TryParse(_txtRegistrationFee.Text, out registrationFee) || registrationFee < 0m)
            {
                ThemedMessageBox.ShowError(this, "Enter a valid non-negative Registration Fee.", "Billing");
                return;
            }

            if (!decimal.TryParse(_txtLaboratoryFee.Text, out laboratoryFee) || laboratoryFee < 0m)
            {
                ThemedMessageBox.ShowError(this, "Enter a valid non-negative Laboratory Fee.", "Billing");
                return;
            }

            try
            {
                _settingsService.Set(AppConstants.SettingKeys.TuitionPerUnit, tuitionPerUnit.ToString("0.00", CultureInfo.InvariantCulture));
                _settingsService.Set(AppConstants.SettingKeys.MiscellaneousFee, miscFee.ToString("0.00", CultureInfo.InvariantCulture));
                _settingsService.Set(AppConstants.SettingKeys.RegistrationFee, registrationFee.ToString("0.00", CultureInfo.InvariantCulture));
                _settingsService.Set(AppConstants.SettingKeys.LaboratoryFee, laboratoryFee.ToString("0.00", CultureInfo.InvariantCulture));
                ThemedMessageBox.ShowInfo(this, "Billing defaults updated successfully.", "Billing");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.SaveBillingSettings", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void LoadConnectionSettings()
        {
            if (!_isAdmin || _cmbConnectionMode == null)
            {
                return;
            }

            var fallbackHost = RuntimeConfiguration.ReadAppSetting("DbHost") ?? "localhost";
            var fallbackPort = RuntimeConfiguration.ReadAppSetting("DbPort") ?? "3306";
            var fallbackDbName = RuntimeConfiguration.ReadAppSetting("DbName") ?? "schoolmanagementsystem";
            var fallbackUser = RuntimeConfiguration.ReadAppSetting("DbUser") ?? string.Empty;
            var fallbackPassword = RuntimeConfiguration.ReadAppSetting("DbPassword") ?? string.Empty;
            var fallbackMode = RuntimeConfiguration.ReadDbMode("Local") ?? "Local";
            var fallbackOnlineHost = ReadAppSettingOrFallback("DbHostOnline", fallbackHost);
            var fallbackOnlinePort = ReadAppSettingOrFallback("DbPortOnline", fallbackPort);
            var fallbackOnlineDbName = ReadAppSettingOrFallback("DbNameOnline", fallbackDbName);
            var fallbackOnlineUser = ReadAppSettingOrFallback("DbUserOnline", fallbackUser);
            var fallbackOnlinePassword = ReadAppSettingOrFallback("DbPasswordOnline", fallbackPassword);
            var fallbackOnlineSslMode = RuntimeConfiguration.ReadAppSetting("DbSslModeOnline")
                                        ?? RuntimeConfiguration.ReadAppSetting("DbSslMode")
                                        ?? "Required";
            var fallbackOnlineSslCaPath = RuntimeConfiguration.ReadAppSetting("DbSslCaPathOnline")
                                          ?? RuntimeConfiguration.ReadAppSetting("DbSslCaPath")
                                          ?? string.Empty;

            try
            {
                if (_settingsService != null)
                {
                    var local = _settingsService.GetDbProfile("Local");
                    var wired = _settingsService.GetDbProfile("Wired");
                    var wireless = _settingsService.GetDbProfile("Wireless");
                    var online = _settingsService.GetDbProfile("Online");
                    var mode = _settingsService.GetDbConnectionMode();
                    var onlineSslMode = _settingsService.Get(AppConstants.SettingKeys.DbSslModeOnline);
                    var onlineSslCaPath = _settingsService.Get(AppConstants.SettingKeys.DbSslCaPathOnline);

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

                    SetProfileInputs(
                        "Online",
                        string.IsNullOrWhiteSpace(online.Host) ? fallbackOnlineHost : online.Host,
                        string.IsNullOrWhiteSpace(online.Port) ? fallbackOnlinePort : online.Port,
                        string.IsNullOrWhiteSpace(online.Database) ? fallbackOnlineDbName : online.Database,
                        string.IsNullOrWhiteSpace(online.Username) ? fallbackOnlineUser : online.Username,
                        string.IsNullOrWhiteSpace(online.Password) ? fallbackOnlinePassword : online.Password);

                    var selectedMode = NormalizeMode(string.IsNullOrWhiteSpace(mode) ? fallbackMode : mode);
                    _cmbConnectionMode.SelectedItem = selectedMode;
                    if (_cmbConnectionMode.SelectedIndex < 0)
                    {
                        _cmbConnectionMode.SelectedItem = "Local";
                    }

                    SetOnlineSecurityInputs(
                        string.IsNullOrWhiteSpace(onlineSslMode) ? fallbackOnlineSslMode : onlineSslMode,
                        string.IsNullOrWhiteSpace(onlineSslCaPath) ? fallbackOnlineSslCaPath : onlineSslCaPath);

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
            SetProfileInputs("Online", fallbackOnlineHost, fallbackOnlinePort, fallbackOnlineDbName, fallbackOnlineUser, fallbackOnlinePassword);
            SetOnlineSecurityInputs(fallbackOnlineSslMode, fallbackOnlineSslCaPath);

            _cmbConnectionMode.SelectedItem = NormalizeMode(fallbackMode);
            if (_cmbConnectionMode.SelectedIndex < 0)
            {
                _cmbConnectionMode.SelectedItem = "Local";
            }
        }

        private static string ReadAppSettingOrFallback(string key, string fallbackValue)
        {
            var value = RuntimeConfiguration.ReadAppSetting(key);
            return string.IsNullOrWhiteSpace(value) ? fallbackValue : value;
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
            if (!_isAdmin)
            {
                ThemedMessageBox.ShowError(this, "Only administrators can edit database profiles.", "Database");
                return;
            }

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
                ValidateProfileInput("Online");

                SaveProfile("Local");
                SaveProfile("Wired");
                SaveProfile("Wireless");
                SaveProfile("Online");

                var mode = NormalizeMode(_cmbConnectionMode.SelectedItem == null ? null : _cmbConnectionMode.SelectedItem.ToString());
                _settingsService.SetDbConnectionMode(mode);
                _settingsService.Set(AppConstants.SettingKeys.DbSslModeOnline, GetOnlineSslModeInput());
                _settingsService.Set(AppConstants.SettingKeys.DbSslCaPathOnline, GetOnlineSslCaPathInput());
                ConnectionModeHelper.ApplyRuntimeMode(mode);

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
            if (!_isAdmin)
            {
                ThemedMessageBox.ShowError(this, "Only administrators can test database profiles.", "Database");
                return;
            }

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

                if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyOnlineSslInputs(builder);
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
            if (!_isAdmin)
            {
                ThemedMessageBox.ShowError(this, "Only administrators can apply database profiles.", "Database");
                return;
            }

            var mode = NormalizeMode(_cmbConnectionMode.SelectedItem == null ? null : _cmbConnectionMode.SelectedItem.ToString());
            ConnectionModeHelper.ApplyRuntimeMode(mode);
            _lblConnectionStatus.Text = "Status: Runtime mode applied (" + mode + ").";
            _lblConnectionStatus.ForeColor = ThemeColors.Secondary;
            ThemedMessageBox.ShowInfo(this, "Runtime DB mode applied: " + mode + ".", "Database");
        }

        private void LoadBackupSettings()
        {
            var fallbackDirectory = _databaseBackupService == null
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SchoolManagementSystem", "Backups")
                : _databaseBackupService.GetDefaultBackupDirectory();

            if (_txtBackupDirectory != null)
            {
                _txtBackupDirectory.Text = fallbackDirectory;
            }

            if (_cmbBackupType != null && _cmbBackupType.SelectedIndex < 0)
            {
                _cmbBackupType.SelectedItem = "Full";
            }

            if (_settingsService == null)
            {
                return;
            }

            try
            {
                var directory = _settingsService.Get(AppConstants.SettingKeys.BackupDirectory);
                var backupType = _settingsService.Get(AppConstants.SettingKeys.BackupPreferredType);

                if (_txtBackupDirectory != null && !string.IsNullOrWhiteSpace(directory))
                {
                    _txtBackupDirectory.Text = directory.Trim();
                }

                if (_cmbBackupType != null)
                {
                    var normalizedType = NormalizeBackupType(backupType);
                    _cmbBackupType.SelectedItem = normalizedType;
                    if (_cmbBackupType.SelectedIndex < 0)
                    {
                        _cmbBackupType.SelectedItem = "Full";
                    }
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadBackupSettings", ex);
            }
        }

        private void SaveBackupSettings(bool showConfirmation = true)
        {
            var directory = (_txtBackupDirectory == null ? string.Empty : _txtBackupDirectory.Text) ?? string.Empty;
            var selectedType = NormalizeBackupType(_cmbBackupType == null || _cmbBackupType.SelectedItem == null ? null : _cmbBackupType.SelectedItem.ToString());

            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = _databaseBackupService == null
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SchoolManagementSystem", "Backups")
                    : _databaseBackupService.GetDefaultBackupDirectory();
            }

            directory = directory.Trim();
            Directory.CreateDirectory(directory);

            if (_txtBackupDirectory != null)
            {
                _txtBackupDirectory.Text = directory;
            }

            if (_settingsService != null)
            {
                _settingsService.Set(AppConstants.SettingKeys.BackupDirectory, directory);
                _settingsService.Set(AppConstants.SettingKeys.BackupPreferredType, selectedType);
            }

            _lblBackupStatus.Text = "Status: Backup settings saved.";
            _lblBackupStatus.ForeColor = ThemeColors.Success;
            if (showConfirmation)
            {
                ThemedMessageBox.ShowInfo(this, "Backup settings saved.", "Backup");
            }
        }

        private void ChooseBackupDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose backup output folder";
                dialog.SelectedPath = _txtBackupDirectory == null ? string.Empty : (_txtBackupDirectory.Text ?? string.Empty).Trim();

                if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    _txtBackupDirectory.Text = dialog.SelectedPath.Trim();
                }
            }
        }

        private void OpenBackupDirectory()
        {
            try
            {
                var path = _txtBackupDirectory == null ? string.Empty : (_txtBackupDirectory.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = _databaseBackupService == null
                        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SchoolManagementSystem", "Backups")
                        : _databaseBackupService.GetDefaultBackupDirectory();
                }

                Directory.CreateDirectory(path);
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.OpenBackupDirectory", ex);
                ThemedMessageBox.ShowError(this, "Unable to open backup directory.", "Backup");
            }
        }

        private async Task CreateBackupNowAsync()
        {
            if (_databaseBackupService == null)
            {
                ThemedMessageBox.ShowError(this, "Backup service is unavailable.", "Backup");
                return;
            }

            if (_backupOperationCts != null)
            {
                ThemedMessageBox.ShowInfo(this, "A backup/restore operation is already running.", "Backup");
                return;
            }

            var cts = new CancellationTokenSource();
            _backupOperationCts = cts;

            try
            {
                SaveBackupSettings(false);

                var selectedType = NormalizeBackupType(_cmbBackupType == null || _cmbBackupType.SelectedItem == null ? null : _cmbBackupType.SelectedItem.ToString());
                var outputDirectory = _txtBackupDirectory == null ? null : _txtBackupDirectory.Text;
                var currentUser = UserSession.CurrentUser;

                UseWaitCursor = true;
                SetBackupOperationUiState(true);
                _lblBackupStatus.Text = "Status: Creating " + selectedType + " backup...";
                _lblBackupStatus.ForeColor = ThemeColors.Secondary;

                var result = await Task.Run(
                    () => _databaseBackupService.CreateBackup(
                        selectedType,
                        outputDirectory,
                        currentUser == null ? (int?)null : currentUser.UserId,
                        currentUser == null ? null : currentUser.Username,
                        cts.Token),
                    cts.Token);

                _lblBackupStatus.Text = "Status: " + result.EffectiveType + " backup saved (" + result.RowCount + " rows changed).";
                _lblBackupStatus.ForeColor = ThemeColors.Success;
                ThemedMessageBox.ShowInfo(
                    this,
                    "Backup complete.\nType: " + result.EffectiveType + "\nFile: " + result.OutputFilePath,
                    "Backup");

                LoadUserLogs();
            }
            catch (OperationCanceledException)
            {
                _lblBackupStatus.Text = "Status: Backup cancelled.";
                _lblBackupStatus.ForeColor = ThemeColors.Secondary;
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.CreateBackupNowAsync", ex);
                _lblBackupStatus.Text = "Status: Backup failed.";
                _lblBackupStatus.ForeColor = ThemeColors.AccentDanger;
                ThemedMessageBox.ShowError(this, "Backup failed.\n" + BuildConnectionHint(ex.Message), "Backup");
            }
            finally
            {
                if (_backupOperationCts == cts)
                {
                    _backupOperationCts = null;
                }

                cts.Dispose();
                UseWaitCursor = false;
                SetBackupOperationUiState(false);
            }
        }

        private async Task RestoreBackupFromFileAsync()
        {
            if (_databaseBackupService == null)
            {
                ThemedMessageBox.ShowError(this, "Backup service is unavailable.", "Restore");
                return;
            }

            if (_backupOperationCts != null)
            {
                ThemedMessageBox.ShowInfo(this, "A backup/restore operation is already running.", "Restore");
                return;
            }

            var initialDirectory = _txtBackupDirectory == null ? string.Empty : (_txtBackupDirectory.Text ?? string.Empty).Trim();
            if (!Directory.Exists(initialDirectory))
            {
                initialDirectory = _databaseBackupService.GetDefaultBackupDirectory();
            }

            string selectedBackupPath = null;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "School backup (*.smsbak)|*.smsbak|All files (*.*)|*.*";
                dialog.Title = "Select backup file to restore";
                dialog.InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : string.Empty;

                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    return;
                }

                selectedBackupPath = dialog.FileName.Trim();
            }

            var confirm = ThemedMessageBox.ShowConfirm(
                this,
                "Restore will overwrite current database data.\nA safety Full backup will be created first.\nContinue restore?",
                "Restore Backup");
            if (confirm != DialogResult.OK)
            {
                return;
            }

            var cts = new CancellationTokenSource();
            _backupOperationCts = cts;

            try
            {
                SaveBackupSettings(false);

                var backupDirectory = _txtBackupDirectory == null ? string.Empty : (_txtBackupDirectory.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(backupDirectory))
                {
                    backupDirectory = _databaseBackupService.GetDefaultBackupDirectory();
                }

                var currentUser = UserSession.CurrentUser;
                UseWaitCursor = true;
                SetBackupOperationUiState(true);

                _lblBackupStatus.Text = "Status: Creating safety full backup before restore...";
                _lblBackupStatus.ForeColor = ThemeColors.Secondary;

                var safetyBackup = await Task.Run(
                    () => _databaseBackupService.CreateBackup(
                        "Full",
                        backupDirectory,
                        currentUser == null ? (int?)null : currentUser.UserId,
                        currentUser == null ? null : currentUser.Username,
                        cts.Token),
                    cts.Token);

                _lblBackupStatus.Text = "Status: Safety backup created. Restoring selected backup...";
                _lblBackupStatus.ForeColor = ThemeColors.Secondary;

                var result = await Task.Run(
                    () => _databaseBackupService.RestoreBackup(
                        selectedBackupPath,
                        currentUser == null ? (int?)null : currentUser.UserId,
                        currentUser == null ? null : currentUser.Username,
                        cts.Token),
                    cts.Token);

                ConnectionStringProvider.ResetDatabaseProfileCache();
                _lblBackupStatus.Text = "Status: Restore complete (" + result.RestoredRows + " rows).";
                _lblBackupStatus.ForeColor = ThemeColors.Success;

                LoadConnectionSettings();
                LoadUserLogs();

                ThemedMessageBox.ShowInfo(
                    this,
                    "Restore complete.\nSafety backup: " + safetyBackup.OutputFilePath + "\nApplied packages: " + result.AppliedBackupPackages + "\nRows restored: " + result.RestoredRows + ".",
                    "Restore Backup");
            }
            catch (OperationCanceledException)
            {
                _lblBackupStatus.Text = "Status: Restore cancelled.";
                _lblBackupStatus.ForeColor = ThemeColors.Secondary;
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.RestoreBackupFromFileAsync", ex);
                _lblBackupStatus.Text = "Status: Restore failed.";
                _lblBackupStatus.ForeColor = ThemeColors.AccentDanger;
                ThemedMessageBox.ShowError(this, "Restore failed.\n" + BuildConnectionHint(ex.Message), "Restore");
            }
            finally
            {
                if (_backupOperationCts == cts)
                {
                    _backupOperationCts = null;
                }

                cts.Dispose();
                UseWaitCursor = false;
                SetBackupOperationUiState(false);
            }
        }

        private void CancelActiveBackupOperation()
        {
            var cts = _backupOperationCts;
            if (cts == null || cts.IsCancellationRequested)
            {
                return;
            }

            cts.Cancel();
            _lblBackupStatus.Text = "Status: Cancellation requested. Waiting for current step to finish...";
            _lblBackupStatus.ForeColor = ThemeColors.Secondary;
        }

        private void SetBackupOperationUiState(bool isRunning)
        {
            if (_btnSaveBackupSettings != null) _btnSaveBackupSettings.Enabled = !isRunning;
            if (_btnCreateBackup != null) _btnCreateBackup.Enabled = !isRunning;
            if (_btnRestoreBackup != null) _btnRestoreBackup.Enabled = !isRunning;
            if (_btnBrowseBackupDirectory != null) _btnBrowseBackupDirectory.Enabled = !isRunning;
            if (_btnOpenBackupDirectory != null) _btnOpenBackupDirectory.Enabled = !isRunning;
            if (_cmbBackupType != null) _cmbBackupType.Enabled = !isRunning;
            if (_txtBackupDirectory != null) _txtBackupDirectory.ReadOnly = isRunning;
            if (_btnCancelBackupOperation != null) _btnCancelBackupOperation.Enabled = isRunning;
        }

        private void LoadUserLogs()
        {
            if (_gridUserLogs == null)
            {
                return;
            }

            if (_activityLogService == null)
            {
                _currentLogs = new List<School_Management_System.Models.ActivityLog>();
                _gridUserLogs.DataSource = new List<UserLogViewRow>();
                return;
            }

            try
            {
                var fromDate = _dtLogFrom == null ? DateTime.Today.AddDays(-30) : _dtLogFrom.Value.Date;
                var toDateExclusive = (_dtLogTo == null ? DateTime.Today : _dtLogTo.Value.Date).AddDays(1);
                var action = _cmbLogAction == null || _cmbLogAction.SelectedItem == null ? null : _cmbLogAction.SelectedItem.ToString();
                if (string.Equals(action, "All", StringComparison.OrdinalIgnoreCase))
                {
                    action = null;
                }

                var userLike = _txtLogUser == null ? null : _txtLogUser.Text;
                var maxRows = _numLogLimit == null ? 300 : Decimal.ToInt32(_numLogLimit.Value);

                _currentLogs = _activityLogService.Search(fromDate, toDateExclusive, userLike, action, maxRows);

                var rows = _currentLogs
                    .Select(log => new UserLogViewRow
                    {
                        CreatedAt = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        UserDisplay = BuildUserDisplay(log),
                        Action = log.Action,
                        Entity = string.IsNullOrWhiteSpace(log.Entity) ? "-" : log.Entity,
                        MachineName = string.IsNullOrWhiteSpace(log.MachineName) ? "-" : log.MachineName,
                        Details = string.IsNullOrWhiteSpace(log.Details) ? string.Empty : log.Details
                    })
                    .ToList();

                _gridUserLogs.DataSource = rows;
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadUserLogs", ex);
                ThemedMessageBox.ShowError(this, "Unable to load user logs.", "User Logs");
            }
        }

        private void ExportLogsToCsv()
        {
            try
            {
                if (_currentLogs == null || _currentLogs.Count == 0)
                {
                    ThemedMessageBox.ShowInfo(this, "No logs available to export.", "User Logs");
                    return;
                }

                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    dialog.Title = "Export user logs";
                    dialog.FileName = "user-logs-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".csv";

                    if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
                    {
                        return;
                    }

                    using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                    {
                        writer.WriteLine("Timestamp,Username,DisplayName,Action,Entity,EntityId,MachineName,Details");
                        foreach (var log in _currentLogs)
                        {
                            var line = string.Join(",",
                                EscapeCsvValue(log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                                EscapeCsvValue(log.Username),
                                EscapeCsvValue(log.DisplayName),
                                EscapeCsvValue(log.Action),
                                EscapeCsvValue(log.Entity),
                                EscapeCsvValue(log.EntityId.HasValue ? log.EntityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                                EscapeCsvValue(log.MachineName),
                                EscapeCsvValue(log.Details));
                            writer.WriteLine(line);
                        }
                    }

                    ThemedMessageBox.ShowInfo(this, "Logs exported successfully.", "User Logs");
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.ExportLogsToCsv", ex);
                ThemedMessageBox.ShowError(this, "Unable to export logs.", "User Logs");
            }
        }

        private static string BuildUserDisplay(School_Management_System.Models.ActivityLog log)
        {
            if (log == null)
            {
                return "-";
            }

            if (!string.IsNullOrWhiteSpace(log.DisplayName) && !string.IsNullOrWhiteSpace(log.Username))
            {
                return log.DisplayName.Trim() + " (" + log.Username.Trim() + ")";
            }

            if (!string.IsNullOrWhiteSpace(log.DisplayName))
            {
                return log.DisplayName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(log.Username))
            {
                return log.Username.Trim();
            }

            return log.UserId.HasValue ? "User #" + log.UserId.Value.ToString(CultureInfo.InvariantCulture) : "-";
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var safe = value.Replace("\"", "\"\"");
            if (safe.Contains(",") || safe.Contains("\"") || safe.Contains("\r") || safe.Contains("\n"))
            {
                return "\"" + safe + "\"";
            }

            return safe;
        }

        private static string NormalizeBackupType(string backupType)
        {
            if (string.IsNullOrWhiteSpace(backupType))
            {
                return "Full";
            }

            var value = backupType.Trim();
            if (string.Equals(value, "incremental", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "inc", StringComparison.OrdinalIgnoreCase))
            {
                return "Incremental";
            }

            if (string.Equals(value, "differential", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "diff", StringComparison.OrdinalIgnoreCase))
            {
                return "Differential";
            }

            return "Full";
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

        private void SetOnlineSecurityInputs(string sslMode, string sslCaPath)
        {
            if (_cmbOnlineSslMode != null)
            {
                var normalizedMode = NormalizeOnlineSslMode(sslMode);
                _cmbOnlineSslMode.SelectedItem = normalizedMode;
                if (_cmbOnlineSslMode.SelectedIndex < 0)
                {
                    _cmbOnlineSslMode.SelectedItem = "Required";
                }
            }

            if (_txtOnlineSslCaPath != null)
            {
                _txtOnlineSslCaPath.Text = sslCaPath ?? string.Empty;
            }
        }

        private string GetOnlineSslModeInput()
        {
            var mode = _cmbOnlineSslMode == null || _cmbOnlineSslMode.SelectedItem == null
                ? "Required"
                : _cmbOnlineSslMode.SelectedItem.ToString();

            return NormalizeOnlineSslMode(mode);
        }

        private string GetOnlineSslCaPathInput()
        {
            return _txtOnlineSslCaPath == null ? string.Empty : (_txtOnlineSslCaPath.Text ?? string.Empty).Trim();
        }

        private void ApplyOnlineSslInputs(MySqlConnectionStringBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            var sslMode = GetOnlineSslModeInput();
            MySqlSslMode parsedMode;
            if (!Enum.TryParse(sslMode, true, out parsedMode))
            {
                parsedMode = MySqlSslMode.Required;
            }

            if (string.Equals(parsedMode.ToString(), "None", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parsedMode.ToString(), "Disabled", StringComparison.OrdinalIgnoreCase))
            {
                parsedMode = MySqlSslMode.Required;
            }

            builder.SslMode = parsedMode;

            var caPath = GetOnlineSslCaPathInput();
            if (!string.IsNullOrWhiteSpace(caPath))
            {
                builder.SslCa = caPath;
            }
        }

        private void ChooseOnlineSslCaPath()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select SSL CA certificate file";
                dialog.Filter = "Certificate files (*.pem;*.crt;*.cer)|*.pem;*.crt;*.cer|All files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                dialog.FileName = GetOnlineSslCaPathInput();

                if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    _txtOnlineSslCaPath.Text = dialog.FileName.Trim();
                }
            }
        }

        private static string NormalizeOnlineSslMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return "Required";
            }

            var value = mode.Trim();
            if (string.Equals(value, "verifyca", StringComparison.OrdinalIgnoreCase))
            {
                return "VerifyCA";
            }

            if (string.Equals(value, "verifyfull", StringComparison.OrdinalIgnoreCase))
            {
                return "VerifyFull";
            }

            if (string.Equals(value, "preferred", StringComparison.OrdinalIgnoreCase))
            {
                return "Preferred";
            }

            return "Required";
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
                case "Online":
                    _txtOnlineHost.Text = host ?? string.Empty;
                    _txtOnlinePort.Text = port ?? string.Empty;
                    _txtOnlineDbName.Text = db ?? string.Empty;
                    _txtOnlineUsername.Text = user ?? string.Empty;
                    _txtOnlinePassword.Text = password ?? string.Empty;
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
                case "Online":
                    host = (_txtOnlineHost.Text ?? string.Empty).Trim();
                    port = (_txtOnlinePort.Text ?? string.Empty).Trim();
                    db = (_txtOnlineDbName.Text ?? string.Empty).Trim();
                    user = (_txtOnlineUsername.Text ?? string.Empty).Trim();
                    password = _txtOnlinePassword.Text ?? string.Empty;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backupOperationCts != null)
                {
                    try
                    {
                        _backupOperationCts.Cancel();
                    }
                    catch
                    {
                    }

                    _backupOperationCts.Dispose();
                    _backupOperationCts = null;
                }
            }

            base.Dispose(disposing);
        }

        private sealed class UserLogViewRow
        {
            public string CreatedAt { get; set; }
            public string UserDisplay { get; set; }
            public string Action { get; set; }
            public string Entity { get; set; }
            public string MachineName { get; set; }
            public string Details { get; set; }
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
                return "Unknown error.";
            }

            var lower = error.ToLowerInvariant();
            if (lower.Contains("access denied"))
            {
                return "Access denied. Verify username/password and remote host grants. For Hostinger, ensure remote MySQL access is enabled for your client IP.";
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


