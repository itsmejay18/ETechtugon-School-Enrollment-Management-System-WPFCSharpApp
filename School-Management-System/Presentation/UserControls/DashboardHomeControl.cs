using System;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class DashboardHomeControl : BaseUserControl
    {
        private readonly StudentService _studentService;
        private readonly FacultyService _facultyService;
        private readonly CourseService _courseService;
        private readonly SubjectService _subjectService;

        private Label _lblStudentsValue;
        private Label _lblFacultyValue;
        private Label _lblCoursesValue;
        private Label _lblSubjectsValue;

        public DashboardHomeControl(StudentService studentService, FacultyService facultyService, CourseService courseService, SubjectService subjectService)
        {
            _studentService = studentService;
            _facultyService = facultyService;
            _courseService = courseService;
            _subjectService = subjectService;
            InitializeComponent();
            LoadStats();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            var header = new Label
            {
                Text = "Overview",
                Dock = DockStyle.Top,
                Height = 36,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text
            };

            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 140,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            var studentCard = CreateStatCard("Students", "S", ThemeColors.Success, out _lblStudentsValue);
            cards.Controls.Add(studentCard);

            cards.Controls.Add(CreateStatCard("Faculty", "F", ThemeColors.Secondary, out _lblFacultyValue));
            cards.Controls.Add(CreateStatCard("Courses", "C", ThemeColors.Secondary, out _lblCoursesValue));
            cards.Controls.Add(CreateStatCard("Subjects", "U", ThemeColors.Secondary, out _lblSubjectsValue));

            Controls.Add(cards);
            Controls.Add(header);
        }

        private Control CreateStatCard(string title, string iconLetter, Color iconColor, out Label valueLabel)
        {
            var card = new Panel { Width = 260, Height = 110, BackColor = ThemeColors.CardBackground, Padding = new Padding(16), Margin = new Padding(0, 0, 12, 0) };
            ThemeManager.StyleCardPanel(card);

            var icon = new PictureBox
            {
                Size = new Size(46, 46),
                Location = new Point(16, 18),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconFactory.CreateCircleIcon(iconColor, iconLetter, 46)
            };

            var lblTitle = new Label
            {
                Text = title ?? string.Empty,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Location = new Point(72, 20),
                AutoSize = true
            };

            valueLabel = new Label
            {
                Text = "0",
                Font = ThemeFonts.Header,
                ForeColor = ThemeColors.Text,
                Location = new Point(72, 42),
                AutoSize = true
            };

            card.Controls.Add(icon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);
            return card;
        }

        private void LoadStats()
        {
            try
            {
                var count = _studentService == null ? 0 : _studentService.GetActiveCount();
                _lblStudentsValue.Text = count.ToString();
            }
            catch
            {
                _lblStudentsValue.Text = "-";
            }

            try
            {
                var count = _facultyService == null ? 0 : _facultyService.GetActiveCount();
                _lblFacultyValue.Text = count.ToString();
            }
            catch
            {
                _lblFacultyValue.Text = "-";
            }

            try
            {
                var count = _courseService == null ? 0 : _courseService.GetActiveCount();
                _lblCoursesValue.Text = count.ToString();
            }
            catch
            {
                _lblCoursesValue.Text = "-";
            }

            try
            {
                var count = _subjectService == null ? 0 : _subjectService.GetActiveCount();
                _lblSubjectsValue.Text = count.ToString();
            }
            catch
            {
                _lblSubjectsValue.Text = "-";
            }
        }
    }
}
