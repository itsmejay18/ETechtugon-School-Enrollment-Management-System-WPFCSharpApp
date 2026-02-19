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
            // Create main layout structure: Sidebar (left) + Content (fill)
            InitializeMainLayout();
            
            // Initialize sidebar components: brand + navigation + logout
            InitializeSidebar();
            
            // Initialize content area: header + body
            InitializeContentArea();
            
            // Add navigation buttons
            InitializeNavigation();

            // Default page.
            LoadModule("dashboard", () => new DashboardHomeControl(_studentService, _facultyService, _courseService, _subjectService));
        }

        /// <summary>
        /// Initialize the main layout structure with sidebar and content panels.
        /// </summary>
        private void InitializeMainLayout()
        {
            _sidebar = new Panel 
            { 
                Dock = DockStyle.Left, 
                Width = 252, 
                BackColor = ThemeColors.SidebarBackground,
                Name = "SidebarPanel"
            };
            
            _content = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = ThemeColors.Background,
                Name = "ContentPanel"
            };
            
            // Add in order: sidebar first (left), then content (fill)
            Controls.Add(_sidebar);
            Controls.Add(_content);
        }

        /// <summary>
        /// Initialize sidebar with brand, navigation, and logout sections.
        /// </summary>
        private void InitializeSidebar()
        {
            // Brand section (top)
            var brand = BuildBrandPanel();
            var brandDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeColors.SidebarDivider };

            // Navigation header
            var navHeader = BuildNavigationHeader();

            // Logout section (bottom)
            var logoutPanel = BuildLogoutPanel();

            // Navigation items (fill)
            _nav = BuildNavigationPanel();

            // Add to sidebar in correct order: top to bottom
            _sidebar.Controls.Add(brand);
            _sidebar.Controls.Add(brandDivider);
            _sidebar.Controls.Add(navHeader);
            _sidebar.Controls.Add(logoutPanel);
            _sidebar.Controls.Add(_nav);
        }

        /// <summary>
        /// Build the brand/logo section of the sidebar.
        /// </summary>
        private Panel BuildBrandPanel()
        {
            var brand = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 84, 
                BackColor = ThemeColors.SidebarBackground,
                Name = "BrandPanel"
            };

            var brandLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 12, 12, 12),
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = false
            };
            brandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            brandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            brandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var brandIcon = new PictureBox
            {
                Size = new Size(42, 42),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconFactory.CreateCircleIcon(ThemeColors.SidebarIcon, IconKind.School, 42),
                Dock = DockStyle.Fill
            };

            var brandText = new Label
            {
                Text = "School\nManagement",
                ForeColor = Color.White,
                Font = ThemeFonts.Sidebar,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            brandLayout.Controls.Add(brandIcon, 0, 0);
            brandLayout.Controls.Add(brandText, 1, 0);
            brand.Controls.Add(brandLayout);

            return brand;
        }

        /// <summary>
        /// Build the navigation section header.
        /// </summary>
        private Panel BuildNavigationHeader()
        {
            var navHeader = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 34, 
                Padding = new Padding(14, 8, 12, 4), 
                BackColor = ThemeColors.SidebarBackground,
                Name = "NavHeaderPanel"
            };

            var navHeaderText = new Label
            {
                Dock = DockStyle.Fill,
                Text = "NAVIGATION",
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.SidebarSectionText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            navHeader.Controls.Add(navHeaderText);
            return navHeader;
        }

        /// <summary>
        /// Build the logout panel at the bottom of the sidebar.
        /// </summary>
        private Panel BuildLogoutPanel()
        {
            var logoutPanel = new Panel 
            { 
                Dock = DockStyle.Bottom, 
                Height = 72, 
                Padding = new Padding(10, 12, 10, 12), 
                BackColor = ThemeColors.SidebarBackground,
                Name = "LogoutPanel"
            };

            var logoutDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeColors.SidebarDivider };
            var btnLogout = new Button { Text = "  Logout", Dock = DockStyle.Fill };
            btnLogout.Image = IconFactory.CreateCircleIcon(ThemeColors.SidebarIconDanger, IconKind.Logout, 22);
            ThemeManager.StyleSidebarButton(btnLogout);
            btnLogout.ForeColor = Color.White;
            btnLogout.Click += (s, e) => Close();

            logoutPanel.Controls.Add(btnLogout);
            logoutPanel.Controls.Add(logoutDivider);

            return logoutPanel;
        }

        /// <summary>
        /// Build the navigation panel that scrolls through menu items.
        /// </summary>
        private FlowLayoutPanel BuildNavigationPanel()
        {
            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 6, 10, 10),
                BackColor = ThemeColors.SidebarBackground,
                Name = "NavigationPanel"
            };
            
            nav.SizeChanged += (s, e) => UpdateNavButtonWidths();
            return nav;
        }

        /// <summary>
        /// Initialize the header and body of the content area.
        /// </summary>
        private void InitializeContentArea()
        {
            // Header panel
            _header = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 60, 
                BackColor = ThemeColors.CardBackground, 
                Padding = new Padding(18, 14, 18, 14),
                Name = "HeaderPanel"
            };

            // Header labels using table layout for better spacing
            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = false
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lblHeader = new Label 
            { 
                Dock = DockStyle.Fill, 
                Font = ThemeFonts.SubHeader, 
                ForeColor = ThemeColors.Text, 
                Text = "Dashboard", 
                TextAlign = ContentAlignment.MiddleLeft 
            };

            _lblUser = new Label 
            { 
                Dock = DockStyle.Fill, 
                Font = ThemeFonts.Label, 
                ForeColor = ThemeColors.MutedText, 
                TextAlign = ContentAlignment.MiddleRight 
            };
            _lblUser.Text = GetUserDisplay();

            headerLayout.Controls.Add(_lblHeader, 0, 0);
            headerLayout.Controls.Add(_lblUser, 2, 0);
            _header.Controls.Add(headerLayout);

            // Body panel for content
            _body = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = ThemeColors.Background, 
                Padding = new Padding(18),
                Name = "BodyPanel"
            };

            // Add to content in order: header first, body fills remaining space
            _content.Controls.Add(_header);
            _content.Controls.Add(_body);
        }

        /// <summary>
        /// Initialize all navigation buttons and modules.
        /// </summary>
        private void InitializeNavigation()
        {
            var role = (UserSession.CurrentUser != null ? (UserSession.CurrentUser.Role ?? string.Empty) : string.Empty).Trim();
            var isAdmin = string.Equals(role, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase);

            // Add navigation buttons
            AddNavButton("dashboard", "Dashboard", IconKind.Dashboard, ThemeColors.SidebarIcon, 
                () => LoadModule("dashboard", () => new DashboardHomeControl(_studentService, _facultyService, _courseService, _subjectService)));
            
            AddNavButton("students", "Students", IconKind.Students, ThemeColors.SidebarIcon, 
                () => LoadModule("students", () => new StudentControl(_studentService, _systemSettingService)));

            AddNavButton("faculty", "Faculty", IconKind.Faculty, ThemeColors.SidebarIcon, 
                () => LoadModule("faculty", () => new FacultyControl(_facultyService)));
            
            AddNavButton("departments", "Departments", IconKind.School, ThemeColors.SidebarIcon, 
                () => LoadModule("departments", () => new DepartmentControl(_departmentService)));
            
            AddNavButton("courses", "Courses", IconKind.Courses, ThemeColors.SidebarIcon, 
                () => LoadModule("courses", () => new CourseControl(_courseService, _departmentService)));
            
            AddNavButton("yearlevels", "Year Levels", IconKind.Curriculum, ThemeColors.SidebarIcon, 
                () => LoadModule("yearlevels", () => new YearLevelControl(_yearLevelService)));
            
            AddNavButton("sections", "Sections", IconKind.Subjects, ThemeColors.SidebarIcon, 
                () => LoadModule("sections", () => new SectionControl(_sectionService, _courseService, _lookupService, _systemSettingService)));
            
            AddNavButton("subjects", "Subjects", IconKind.Subjects, ThemeColors.SidebarIcon, 
                () => LoadModule("subjects", () => new SubjectControl(_subjectService, _courseService)));
            
            AddNavButton("curriculum", "Curriculum", IconKind.Curriculum, ThemeColors.SidebarIcon, 
                () => LoadModule("curriculum", () => new CurriculumControl(_curriculumService, _courseService, _lookupService)));
            
            AddNavButton("schedule", "Class Schedule", IconKind.Enrollment, ThemeColors.SidebarIcon, 
                () => LoadModule("schedule", () => new ClassScheduleControl(_classScheduleService, _sectionService, _curriculumService, _systemSettingService)));
            
            AddNavButton("enrollment", "Enrollment", IconKind.Enrollment, ThemeColors.SidebarIcon, 
                () => LoadModule("enrollment", () => new EnrollmentControl(_enrollmentService, _studentService, _curriculumService, _courseService, _lookupService, _sectionService, _systemSettingService, _classScheduleService)));
            
            AddNavButton("settings", "Settings", IconKind.Trend, ThemeColors.SidebarIcon, 
                () => LoadModule("settings", () => new SettingsControl(_systemSettingService, _lookupService)));

            if (isAdmin)
            {
                AddNavButton("users", "User Roles", IconKind.Users, ThemeColors.SidebarIcon, 
                    () => LoadModule("users", () => new UsersControl(_userManagementService)));
            }

            // Load default page
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

        /// <summary>
        /// Load a module (UserControl) into the main content area and update the header.
        /// </summary>
        private void LoadModule(string key, Func<UserControl> factory)
        {
            if (string.IsNullOrWhiteSpace(key) || factory == null)
            {
                return;
            }

            _lblHeader.Text = GetHeaderForKey(key);
            SetActiveNavButton(key);

            _body.Controls.Clear();
            var ctrl = GetOrCreate(key, factory);
            if (ctrl != null)
            {
                ctrl.Dock = DockStyle.Fill;
                _body.Controls.Add(ctrl);
            }
        }

        /// <summary>
        /// Add a navigation button to the sidebar.
        /// </summary>
        private void AddNavButton(string key, string text, IconKind iconKind, Color iconColor, Action onClick)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var btn = new Button
            {
                Text = "  " + text,
                Height = 46,
                Margin = new Padding(0, 2, 0, 2),
                Name = "NavBtn_" + key
            };
            btn.Image = IconFactory.CreateCircleIcon(iconColor, iconKind, 24);
            ThemeManager.StyleSidebarButton(btn);
            btn.Click += (s, e) => onClick?.Invoke();
            
            _nav.Controls.Add(btn);
            _navButtons[key] = btn;
            UpdateNavButtonWidths();
        }

        /// <summary>
        /// Update the width of all navigation buttons to fill the available sidebar width.
        /// </summary>
        private void UpdateNavButtonWidths()
        {
            if (_nav == null || _nav.Controls.Count == 0)
            {
                return;
            }

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

        /// <summary>
        /// Set the active/inactive state of navigation buttons.
        /// </summary>
        private void SetActiveNavButton(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            foreach (var pair in _navButtons)
            {
                var isActive = string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase);
                ThemeManager.SetSidebarButtonState(pair.Value, isActive);
            }
        }

        /// <summary>
        /// Get or create a cached UserControl for a module key.
        /// </summary>
        private UserControl GetOrCreate(string key, Func<UserControl> factory)
        {
            if (string.IsNullOrWhiteSpace(key) || factory == null)
            {
                return null;
            }

            UserControl existing;
            if (_cache.TryGetValue(key, out existing))
            {
                return existing;
            }

            var created = factory();
            if (created != null)
            {
                _cache[key] = created;
            }
            return created;
        }

        /// <summary>
        /// Get the header text for a given module key.
        /// </summary>
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

        /// <summary>
        /// Get the current user display string with name and role.
        /// </summary>
        private static string GetUserDisplay()
        {
            var u = UserSession.CurrentUser;
            if (u == null) return string.Empty;

            var name = string.IsNullOrWhiteSpace(u.DisplayName) ? u.Username : u.DisplayName;
            return name + " (" + (u.Role ?? "User") + ")";
        }
    }
}
