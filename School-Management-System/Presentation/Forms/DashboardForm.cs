using System;
using System.Collections.Generic;
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
using School_Management_System.Presentation.UserControls;

namespace School_Management_System.Presentation.Forms
{
    public sealed class DashboardForm : BaseForm
    {
        private readonly DatabaseHelper _db;

        private Panel _sidebar;
        private Panel _content;
        private Panel _header;
        private Label _lblHeader;
        private Label _lblUser;

        private FlowLayoutPanel _nav;

        private readonly Dictionary<string, UserControl> _cache = new Dictionary<string, UserControl>(StringComparer.OrdinalIgnoreCase);

        private StudentService _studentService;

        public DashboardForm(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            Text = AppConstants.AppTitle;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1024, 640);

            InitializeServices();
            InitializeComponent();
        }

        private void InitializeServices()
        {
            IStudentData studentData = new StudentData(_db);
            _studentService = new StudentService(studentData);
        }

        private void InitializeComponent()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = ThemeColors.SidebarBackground };
            _content = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            Controls.Add(_content);
            Controls.Add(_sidebar);

            var brand = new Panel { Dock = DockStyle.Top, Height = 74, Padding = new Padding(12, 12, 12, 12), BackColor = ThemeColors.SidebarBackground };
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

            var logoutPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12, 10, 12, 10), BackColor = ThemeColors.SidebarBackground };
            var btnLogout = new Button { Text = "  Logout", Dock = DockStyle.Fill };
            btnLogout.Image = IconFactory.CreateCircleIcon(ThemeColors.AccentDanger, "X", 24);
            ThemeManager.StyleSidebarButton(btnLogout);
            btnLogout.Click += (s, e) => Close();
            logoutPanel.Controls.Add(btnLogout);
            _sidebar.Controls.Add(logoutPanel);

            _nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 6, 0, 6),
                BackColor = ThemeColors.SidebarBackground
            };
            _sidebar.Controls.Add(_nav);

            AddNavButton("Dashboard", "D", ThemeColors.Secondary, () => LoadModule("dashboard", () => new DashboardHomeControl(_studentService)));
            AddNavButton("Students", "S", ThemeColors.Success, () => LoadModule("students", () => new StudentControl(_studentService)));

            // Placeholders for other required modules (wired in, implemented incrementally).
            AddNavButton("Faculty", "F", ThemeColors.Secondary, () => LoadModule("faculty", () => new PlaceholderControl("Faculty module is next.")));
            AddNavButton("Courses", "C", ThemeColors.Secondary, () => LoadModule("courses", () => new PlaceholderControl("Course module is next.")));
            AddNavButton("Subjects", "U", ThemeColors.Secondary, () => LoadModule("subjects", () => new PlaceholderControl("Subject module is next.")));
            AddNavButton("Users", "A", ThemeColors.Secondary, () => LoadModule("users", () => new PlaceholderControl("Users module is next.")));
            AddNavButton("Curriculum", "L", ThemeColors.Secondary, () => LoadModule("curriculum", () => new PlaceholderControl("Curriculum module is next (ListView + checkboxes).")));
            AddNavButton("Enrollment", "E", ThemeColors.Secondary, () => LoadModule("enrollment", () => new PlaceholderControl("Enrollment transaction is next (multi-step).")));

            _header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 14, 18, 14) };
            _lblHeader = new Label { Dock = DockStyle.Left, Width = 420, Font = ThemeFonts.SubHeader, ForeColor = ThemeColors.Text, Text = "Dashboard", TextAlign = ContentAlignment.MiddleLeft };
            _lblUser = new Label { Dock = DockStyle.Right, Width = 320, Font = ThemeFonts.Label, ForeColor = ThemeColors.MutedText, TextAlign = ContentAlignment.MiddleRight };
            _lblUser.Text = GetUserDisplay();
            _header.Controls.Add(_lblUser);
            _header.Controls.Add(_lblHeader);

            _content.Controls.Add(_header);

            var body = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(18) };
            _content.Controls.Add(body);

            // Default page.
            LoadInto(body, "dashboard", () => new DashboardHomeControl(_studentService));

            void LoadInto(Panel container, string key, Func<UserControl> factory)
            {
                container.Controls.Clear();
                var ctrl = GetOrCreate(key, factory);
                ctrl.Dock = DockStyle.Fill;
                container.Controls.Add(ctrl);
            }

            void LoadModule(string key, Func<UserControl> factory)
            {
                _lblHeader.Text = GetHeaderForKey(key);
                LoadInto(body, key, factory);
            }

            void AddNavButton(string text, string iconLetter, Color iconColor, Action onClick)
            {
                var btn = new Button { Text = "  " + text, Width = _sidebar.Width - 2, Height = 42 };
                btn.Image = IconFactory.CreateCircleIcon(iconColor, iconLetter, 24);
                ThemeManager.StyleSidebarButton(btn);
                btn.Click += (s, e) => onClick();
                _nav.Controls.Add(btn);
            }

            UserControl GetOrCreate(string key, Func<UserControl> factory)
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
        }

        private static string GetHeaderForKey(string key)
        {
            if (string.Equals(key, "dashboard", StringComparison.OrdinalIgnoreCase)) return "Dashboard";
            if (string.Equals(key, "students", StringComparison.OrdinalIgnoreCase)) return "Students";
            if (string.Equals(key, "faculty", StringComparison.OrdinalIgnoreCase)) return "Faculty";
            if (string.Equals(key, "courses", StringComparison.OrdinalIgnoreCase)) return "Courses";
            if (string.Equals(key, "subjects", StringComparison.OrdinalIgnoreCase)) return "Subjects";
            if (string.Equals(key, "users", StringComparison.OrdinalIgnoreCase)) return "Users";
            if (string.Equals(key, "curriculum", StringComparison.OrdinalIgnoreCase)) return "Curriculum";
            if (string.Equals(key, "enrollment", StringComparison.OrdinalIgnoreCase)) return "Enrollment";
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

