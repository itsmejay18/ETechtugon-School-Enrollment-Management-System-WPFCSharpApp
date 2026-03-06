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
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;
using School_Management_System.Presentation.UserControls;

namespace School_Management_System.Presentation.Forms
{
    public sealed partial class DashboardForm : BaseForm
    {
        private const int SidebarHeaderHeight = 94;

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
        private ActivityLogService _activityLogService;
        private DatabaseBackupService _databaseBackupService;

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

            IActivityLogData activityLogData = new ActivityLogData(_db);
            _activityLogService = new ActivityLogService(activityLogData);
            _databaseBackupService = new DatabaseBackupService(_db, _activityLogService);
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
                Width = 272, 
                BackColor = ThemeColors.SidebarBackground,
                Name = "SidebarPanel"
            };
            
            _content = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = ThemeColors.Background,
                Name = "ContentPanel"
            };
            
            // For Dock layout, add Fill first then Left to avoid overlap.
            Controls.Add(_content);
            Controls.Add(_sidebar);
        }

        /// <summary>
        /// Initialize sidebar with brand, navigation, and logout sections.
        /// </summary>
        private void InitializeSidebar()
        {
            // Brand section (top)
            var brand = BuildBrandPanel();
            var brandDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeColors.SidebarDivider };
            var profile = BuildProfilePanel();
            var profileDivider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ThemeColors.SidebarDivider };

            // Navigation header
            var navHeader = BuildNavigationHeader();

            // Logout section (bottom)
            var logoutPanel = BuildLogoutPanel();

            // Navigation items (fill)
            _nav = BuildNavigationPanel();

            // For Dock layout, add Fill first then Bottom/Top panels.
            _sidebar.Controls.Add(_nav);
            _sidebar.Controls.Add(profile);
            _sidebar.Controls.Add(profileDivider);
            _sidebar.Controls.Add(logoutPanel);
            _sidebar.Controls.Add(navHeader);
            _sidebar.Controls.Add(brandDivider);
            _sidebar.Controls.Add(brand);
        }

        /// <summary>
        /// Build the brand/logo section of the sidebar.
        /// </summary>
        private Panel BuildBrandPanel()
        {
            var brandLogo = BrandAssets.CreateLogoImage();
            var brand = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = SidebarHeaderHeight, 
                BackColor = ThemeColors.SidebarBackground,
                Name = "BrandPanel"
            };

            var brandLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 8),
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = false
            };
            brandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            brandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            brandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var brandIcon = new PictureBox
            {
                Size = new Size(50, 50),
                SizeMode = brandLogo != null ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage,
                Image = brandLogo ?? IconFactory.CreateCircleIcon(ThemeColors.SidebarIcon, IconKind.School, 44),
                Dock = DockStyle.Fill
            };

            var textHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 6, 0, 0), BackColor = Color.Transparent };
            var brandTitle = new Label
            {
                Text = "School Management",
                ForeColor = ThemeColors.Text,
                Font = ThemeFonts.Sidebar,
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0)
            };
            var brandSubtitle = new Label
            {
                Text = "SYSTEM",
                ForeColor = ThemeColors.SidebarSectionText,
                Font = ThemeFonts.Label,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textHost.Controls.Add(brandSubtitle);
            textHost.Controls.Add(brandTitle);

            brandLayout.Controls.Add(brandIcon, 0, 0);
            brandLayout.Controls.Add(textHost, 1, 0);
            var accentLine = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 2,
                BackColor = Color.FromArgb(120, ThemeColors.Secondary)
            };
            brand.Controls.Add(accentLine);
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

        private Panel BuildProfilePanel()
        {
            var user = UserSession.CurrentUser;
            var displayName = user == null
                ? "Guest User"
                : (string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName);
            var role = user == null || string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role.Trim();
            var roleColor = GetRoleAccentColor(role);
            var hasPhoto = false;
            var avatarImage = BuildProfileAvatarImage(user, roleColor, displayName, out hasPhoto);

            var profilePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 88,
                BackColor = ThemeColors.SidebarBackground,
                Padding = new Padding(12, 10, 12, 10),
                Name = "SidebarProfilePanel"
            };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(10, 8, 10, 8)
            };
            UiHelper.ApplyRoundedCorners(card, 6);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            var avatar = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = hasPhoto ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage,
                Image = avatarImage,
                BackColor = hasPhoto ? Color.White : Color.Transparent,
                BorderStyle = hasPhoto ? BorderStyle.FixedSingle : BorderStyle.None,
                Margin = new Padding(0, 0, 8, 0)
            };

            var nameLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = displayName,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = ThemeColors.Text,
                Font = ThemeFonts.Sidebar
            };

            var roleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = role,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = roleColor,
                Font = ThemeFonts.CaptionStrong
            };

            layout.Controls.Add(avatar, 0, 0);
            layout.SetRowSpan(avatar, 2);
            layout.Controls.Add(nameLabel, 1, 0);
            layout.Controls.Add(roleLabel, 1, 1);

            card.Controls.Add(layout);
            profilePanel.Controls.Add(card);
            return profilePanel;
        }

        private static Image BuildProfileAvatarImage(User user, Color roleColor, string displayName, out bool hasPhoto)
        {
            hasPhoto = false;
            var photoPath = user == null ? null : user.PhotoPath;
            var fullPath = PhotoStorageHelper.ResolvePhotoPath(photoPath);
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                try
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        using (var image = Image.FromFile(fullPath))
                        {
                            hasPhoto = true;
                            return new Bitmap(image);
                        }
                    }
                }
                catch
                {
                    // Fallback to generated avatar.
                }
            }

            return IconFactory.CreateCircleIcon(roleColor, displayName, 36);
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
            btnLogout.ForeColor = ThemeColors.Text;
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
                AutoScroll = false,
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
                Height = SidebarHeaderHeight, 
                BackColor = ThemeColors.CardBackground, 
                Padding = new Padding(18, 0, 18, 0),
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
            _header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ThemeColors.SidebarDivider });

            // Body panel for content
            _body = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = ThemeColors.Background, 
                Padding = new Padding(18),
                Name = "BodyPanel",
                AutoScroll = true
            };

            // For Dock layout, add Fill first then Top.
            _content.Controls.Add(_body);
            _content.Controls.Add(_header);
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

            AddNavButton("enrollment", "Enrollment", IconKind.Enrollment, ThemeColors.SidebarIcon,
                () => LoadModule("enrollment", () => new EnrollmentControl(_enrollmentService, _studentService, _curriculumService, _courseService, _lookupService, _sectionService, _systemSettingService, _classScheduleService)));

            AddNavButton("schedule", "Schedule", IconKind.Enrollment, ThemeColors.SidebarIcon,
                () => LoadModule("schedule", () => new ClassScheduleControl(_classScheduleService, _sectionService, _curriculumService, _systemSettingService)));

            AddNavButton("calendar", "Calendar", IconKind.Pie, ThemeColors.SidebarIcon,
                () => LoadModule("calendar", () => new ClassScheduleControl(_classScheduleService, _sectionService, _curriculumService, _systemSettingService)));

            AddNavButton("settings", "Settings", IconKind.Trend, ThemeColors.SidebarIcon,
                () => LoadModule(
                    "settings",
                    () => new SettingsControl(
                        _systemSettingService,
                        _lookupService,
                        _departmentService,
                        _courseService,
                        _yearLevelService,
                        _sectionService,
                        _subjectService,
                        _curriculumService,
                        _userManagementService,
                        _activityLogService,
                        _databaseBackupService,
                        isAdmin)));

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

            try
            {
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
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("DashboardForm.LoadModule." + key, ex);
                var message = "Unable to open " + GetHeaderForKey(key) + ". Check database setup and try again.";
                var detail = BuildModuleErrorHint(ex);
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    message += "\n" + detail;
                }

                ThemedMessageBox.ShowError(this, message, "Module Error");
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

            var buttons = _nav.Controls.OfType<Button>().ToList();
            var availableHeight = _nav.ClientSize.Height - _nav.Padding.Top - _nav.Padding.Bottom;
            var totalMarginHeight = buttons.Count * 4; // top+bottom margin (2 + 2)
            var computedHeight = buttons.Count == 0 ? 46 : (availableHeight - totalMarginHeight) / buttons.Count;
            if (computedHeight > 46) computedHeight = 46;
            if (computedHeight < 28) computedHeight = 28;

            foreach (var button in buttons)
            {
                button.Width = width;
                button.Height = computedHeight;
                button.Margin = new Padding(0, 2, 0, 2);
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
            if (string.Equals(key, "schedule", StringComparison.OrdinalIgnoreCase)) return "Schedule";
            if (string.Equals(key, "calendar", StringComparison.OrdinalIgnoreCase)) return "Calendar";
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

        private static Color GetRoleAccentColor(string role)
        {
            if (string.Equals(role, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return ThemeColors.AccentDanger;
            }

            if (string.Equals(role, AppConstants.Roles.Registrar, StringComparison.OrdinalIgnoreCase))
            {
                return ThemeColors.Secondary;
            }

            if (string.Equals(role, AppConstants.Roles.Faculty, StringComparison.OrdinalIgnoreCase))
            {
                return ThemeColors.Success;
            }

            return ThemeColors.MutedText;
        }

        private static string BuildModuleErrorHint(Exception ex)
        {
            if (ex == null || string.IsNullOrWhiteSpace(ex.Message))
            {
                return string.Empty;
            }

            var lower = ex.Message.ToLowerInvariant();
            if (lower.Contains("doesn't exist") || lower.Contains("unknown column"))
            {
                return "Database schema appears outdated. Run DatabaseScripts\\Patch-RemoteSchema.sql (or Patch-RemoteMySQL.ps1) then reopen the app.";
            }

            if (lower.Contains("access denied") || (lower.Contains("host") && lower.Contains("not allowed")))
            {
                return "Database credentials/host is blocked. Verify App.config or Settings > Database > Database Connection Profiles.";
            }

            if (ex.Message.Length > 180)
            {
                return ex.Message.Substring(0, 180) + "...";
            }

            return ex.Message;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var user = UserSession.CurrentUser;
            if (user != null && _activityLogService != null)
            {
                _activityLogService.LogLogout(user);
            }

            base.OnFormClosing(e);
        }
    }
}
