using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.Common;
using School_Management_System.DataLayer;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;
using School_Management_System.Presentation.UserControls;

namespace School_Management_System.Presentation.Forms
{
    public sealed partial class DashboardForm : BaseForm
    {
        private readonly DatabaseHelper _db;

        private Panel _sidebar;
        private Panel _content;
        private Panel _header;
        private Panel _body;
        private Label _lblHeader;
        private Label _lblUser;

        private FlowLayoutPanel _nav;
        private readonly Dictionary<string, Button> _navButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, UserControl> _cache = new Dictionary<string, UserControl>(StringComparer.OrdinalIgnoreCase);

        private StudentService _studentService;
        private FacultyService _facultyService;
        private CourseService _courseService;
        private SubjectService _subjectService;
        private LookupService _lookupService;
        private CurriculumService _curriculumService;
        private EnrollmentService _enrollmentService;
        private UserManagementService _userManagementService;
        private DepartmentService _departmentService;
        private YearLevelService _yearLevelService;
        private SectionService _sectionService;
        private ClassScheduleService _classScheduleService;
        private SystemSettingService _systemSettingService;

        public DashboardForm()
        {
            _db = null;

            Text = AppConstants.AppTitle;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1024, 640);

            InitializeComponent();
            if (IsDesignerHost())
            {
                InitializeDesignerSurface();
                return;
            }

            InitializeRuntimeComponent();
        }

        public DashboardForm(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            Text = AppConstants.AppTitle;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1024, 640);

            InitializeComponent();
            if (IsDesignerHost())
            {
                InitializeDesignerSurface();
                return;
            }

            InitializeServices();
            InitializeRuntimeComponent();
        }

        private void InitializeServices()
        {
            IStudentData studentData = new StudentData(_db);
            _studentService = new StudentService(studentData);

            IFacultyData facultyData = new FacultyData(_db);
            _facultyService = new FacultyService(facultyData);

            IDepartmentData departmentData = new DepartmentData(_db);
            _departmentService = new DepartmentService(departmentData);

            ICourseData courseData = new CourseData(_db);
            _courseService = new CourseService(courseData);

            ISubjectData subjectData = new SubjectData(_db);
            _subjectService = new SubjectService(subjectData);

            ILookupData lookupData = new LookupData(_db);
            _lookupService = new LookupService(lookupData);

            IYearLevelData yearLevelData = new YearLevelData(_db);
            _yearLevelService = new YearLevelService(yearLevelData);

            ICurriculumData curriculumData = new CurriculumData(_db);
            _curriculumService = new CurriculumService(curriculumData);

            ISectionData sectionData = new SectionData(_db);
            _sectionService = new SectionService(sectionData);

            IClassScheduleData classScheduleData = new ClassScheduleData(_db);
            _classScheduleService = new ClassScheduleService(classScheduleData);

            IEnrollmentData enrollmentData = new EnrollmentData(_db);
            _enrollmentService = new EnrollmentService(enrollmentData);

            IUserManagementData userMgmtData = new UserManagementData(_db);
            _userManagementService = new UserManagementService(userMgmtData);

            ISystemSettingData systemSettingData = new SystemSettingData(_db);
            _systemSettingService = new SystemSettingService(systemSettingData);
        }

        private void InitializeRuntimeComponent()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 252, BackColor = ThemeColors.SidebarBackground };
            _content = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            Controls.Add(_content);
            Controls.Add(_sidebar);

            var brand = new Panel { Dock = DockStyle.Top, Height = 84, Padding = new Padding(12, 12, 12, 12), BackColor = ThemeColors.SidebarBackground };
            var brandIcon = new PictureBox
            {
                Size = new Size(42, 42),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconFactory.CreateCircleIcon(ThemeColors.SidebarIcon, IconKind.School, 42),
                Location = new Point(12, 15)
            };
            var brandText = new Label
            {
                Text = "School\nManagement",
                ForeColor = Color.White,
                Font = ThemeFonts.Sidebar,
                AutoSize = false,
                Location = new Point(62, 16),
                Size = new Size(172, 40)
            };
            brand.Controls.Add(brandIcon);
            brand.Controls.Add(brandText);

            var brandDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeColors.SidebarDivider };

            var navHeader = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(14, 8, 12, 4), BackColor = ThemeColors.SidebarBackground };
            var navHeaderText = new Label
            {
                Dock = DockStyle.Fill,
                Text = "NAVIGATION",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.SidebarSectionText,
                TextAlign = ContentAlignment.MiddleLeft
            };
            navHeader.Controls.Add(navHeaderText);

            var logoutPanel = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(10, 12, 10, 12), BackColor = ThemeColors.SidebarBackground };
            var logoutDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeColors.SidebarDivider };
            var btnLogout = new Button { Text = "  Logout", Dock = DockStyle.Fill };
            btnLogout.Image = IconFactory.CreateCircleIcon(ThemeColors.SidebarIconDanger, IconKind.Logout, 22);
            ThemeManager.StyleSidebarButton(btnLogout);
            btnLogout.ForeColor = Color.White;
            btnLogout.Click += (s, e) => Close();
            logoutPanel.Controls.Add(btnLogout);
            logoutPanel.Controls.Add(logoutDivider);

            _nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 6, 10, 10),
                BackColor = ThemeColors.SidebarBackground
            };
            _nav.SizeChanged += (s, e) => UpdateNavButtonWidths();

            // Add fill first, then bottom/top panels so docking reserves space correctly.
            _sidebar.Controls.Add(_nav);
            _sidebar.Controls.Add(navHeader);
            _sidebar.Controls.Add(logoutPanel);
            _sidebar.Controls.Add(brandDivider);
            _sidebar.Controls.Add(brand);

            _header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 14, 18, 14) };
            _lblHeader = new Label { Dock = DockStyle.Left, Width = 420, Font = ThemeFonts.SubHeader, ForeColor = ThemeColors.Text, Text = "Dashboard", TextAlign = ContentAlignment.MiddleLeft };
            _lblUser = new Label { Dock = DockStyle.Right, Width = 320, Font = ThemeFonts.Label, ForeColor = ThemeColors.MutedText, TextAlign = ContentAlignment.MiddleRight };
            _lblUser.Text = GetUserDisplay();
            _header.Controls.Add(_lblUser);
            _header.Controls.Add(_lblHeader);

            _body = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(18) };
            _content.Controls.Add(_body);
            _content.Controls.Add(_header);

            var role = (UserSession.CurrentUser != null ? (UserSession.CurrentUser.Role ?? string.Empty) : string.Empty).Trim();
            var isAdmin = string.Equals(role, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase);

            AddNavButton("dashboard", "Dashboard", IconKind.Dashboard, ThemeColors.SidebarIcon, () => LoadModule("dashboard", () => new DashboardHomeControl(_studentService, _facultyService, _courseService, _subjectService)));
            AddNavButton("students", "Students", IconKind.Students, ThemeColors.SidebarIcon, () => LoadModule("students", () => new StudentControl(_studentService, _systemSettingService)));

            AddNavButton("faculty", "Faculty", IconKind.Faculty, ThemeColors.SidebarIcon, () => LoadModule("faculty", () => new FacultyControl(_facultyService)));
            AddNavButton("departments", "Departments", IconKind.School, ThemeColors.SidebarIcon, () => LoadModule("departments", () => new DepartmentControl(_departmentService)));
            AddNavButton("courses", "Courses", IconKind.Courses, ThemeColors.SidebarIcon, () => LoadModule("courses", () => new CourseControl(_courseService, _departmentService)));
            AddNavButton("yearlevels", "Year Levels", IconKind.Curriculum, ThemeColors.SidebarIcon, () => LoadModule("yearlevels", () => new YearLevelControl(_yearLevelService)));
            AddNavButton("sections", "Sections", IconKind.Subjects, ThemeColors.SidebarIcon, () => LoadModule("sections", () => new SectionControl(_sectionService, _courseService, _lookupService, _systemSettingService)));
            AddNavButton("subjects", "Subjects", IconKind.Subjects, ThemeColors.SidebarIcon, () => LoadModule("subjects", () => new SubjectControl(_subjectService, _courseService)));
            AddNavButton("curriculum", "Curriculum", IconKind.Curriculum, ThemeColors.SidebarIcon, () => LoadModule("curriculum", () => new CurriculumControl(_curriculumService, _courseService, _lookupService)));
            AddNavButton("schedule", "Class Schedule", IconKind.Enrollment, ThemeColors.SidebarIcon, () => LoadModule("schedule", () => new ClassScheduleControl(_classScheduleService, _sectionService, _curriculumService, _systemSettingService)));
            AddNavButton("enrollment", "Enrollment", IconKind.Enrollment, ThemeColors.SidebarIcon, () => LoadModule("enrollment", () => new EnrollmentControl(_enrollmentService, _studentService, _curriculumService, _courseService, _lookupService, _sectionService, _systemSettingService, _classScheduleService)));
            AddNavButton("settings", "Settings", IconKind.Trend, ThemeColors.SidebarIcon, () => LoadModule("settings", () => new SettingsControl(_systemSettingService, _lookupService)));

            if (isAdmin)
            {
                AddNavButton("users", "User Roles", IconKind.Users, ThemeColors.SidebarIcon, () => LoadModule("users", () => new UsersControl(_userManagementService)));
            }

            // Default page.
            LoadModule("dashboard", () => new DashboardHomeControl(_studentService, _facultyService, _courseService, _subjectService));
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

        private void LoadModule(string key, Func<UserControl> factory)
        {
            _lblHeader.Text = GetHeaderForKey(key);
            SetActiveNavButton(key);

            _body.Controls.Clear();
            var ctrl = GetOrCreate(key, factory);
            ctrl.Dock = DockStyle.Fill;
            _body.Controls.Add(ctrl);
        }

        private void AddNavButton(string key, string text, IconKind iconKind, Color iconColor, Action onClick)
        {
            var btn = new Button
            {
                Text = "  " + text,
                Height = 46,
                Margin = new Padding(0, 2, 0, 2)
            };
            btn.Image = IconFactory.CreateCircleIcon(iconColor, iconKind, 24);
            ThemeManager.StyleSidebarButton(btn);
            btn.Click += (s, e) => onClick();
            _nav.Controls.Add(btn);
            _navButtons[key] = btn;
            UpdateNavButtonWidths();
        }

        private void UpdateNavButtonWidths()
        {
            if (_nav == null) return;

            var width = _nav.ClientSize.Width
                        - _nav.Padding.Left
                        - _nav.Padding.Right
                        - (_nav.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
            if (width < 120) width = 120;

            foreach (var button in _nav.Controls.OfType<Button>())
            {
                button.Width = width;
            }
        }

        private void SetActiveNavButton(string key)
        {
            foreach (var pair in _navButtons)
            {
                ThemeManager.SetSidebarButtonState(pair.Value, string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase));
            }
        }

        private UserControl GetOrCreate(string key, Func<UserControl> factory)
        {
            UserControl existing;
            if (_cache.TryGetValue(key, out existing))
            {
                return existing;
            }

            var created = factory();
            _cache[key] = created;
            return created;
        }

        private static string GetHeaderForKey(string key)
        {
            if (string.Equals(key, "dashboard", StringComparison.OrdinalIgnoreCase)) return "Dashboard";
            if (string.Equals(key, "students", StringComparison.OrdinalIgnoreCase)) return "Students";
            if (string.Equals(key, "faculty", StringComparison.OrdinalIgnoreCase)) return "Faculty";
            if (string.Equals(key, "courses", StringComparison.OrdinalIgnoreCase)) return "Courses";
            if (string.Equals(key, "subjects", StringComparison.OrdinalIgnoreCase)) return "Subjects";
            if (string.Equals(key, "users", StringComparison.OrdinalIgnoreCase)) return "User Roles";
            if (string.Equals(key, "curriculum", StringComparison.OrdinalIgnoreCase)) return "Curriculum";
            if (string.Equals(key, "enrollment", StringComparison.OrdinalIgnoreCase)) return "Enrollment";
            if (string.Equals(key, "departments", StringComparison.OrdinalIgnoreCase)) return "Departments";
            if (string.Equals(key, "yearlevels", StringComparison.OrdinalIgnoreCase)) return "Year Levels";
            if (string.Equals(key, "sections", StringComparison.OrdinalIgnoreCase)) return "Sections";
            if (string.Equals(key, "schedule", StringComparison.OrdinalIgnoreCase)) return "Class Schedule";
            if (string.Equals(key, "settings", StringComparison.OrdinalIgnoreCase)) return "Settings";
            return "Module";
        }

        private static string GetUserDisplay()
        {
            var u = UserSession.CurrentUser;
            if (u == null) return string.Empty;

            var name = string.IsNullOrWhiteSpace(u.DisplayName) ? u.Username : u.DisplayName;
            return name + " (" + (u.Role ?? "User") + ")";
        }
    }
}
