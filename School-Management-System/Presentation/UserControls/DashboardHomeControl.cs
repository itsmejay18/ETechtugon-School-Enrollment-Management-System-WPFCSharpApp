using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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

        private readonly PieEntry[] _pieEntries = new PieEntry[4];
        private readonly List<Control> _statCards = new List<Control>();

        private TableLayoutPanel _rootLayout;
        private TableLayoutPanel _cardsLayout;
        private TableLayoutPanel _analyticsLayout;
        private Control _lineChartHost;
        private Control _pieChartHost;
        private Panel _lineChartPanel;
        private Panel _pieChartPanel;
        private Label _lblHeroTotal;
        private Label _lblStudentsValue;
        private Label _lblFacultyValue;
        private Label _lblCoursesValue;
        private Label _lblSubjectsValue;

        private int _studentsCount;
        private int _facultyCount;
        private int _coursesCount;
        private int _subjectsCount;
        private int[] _trendValues = new int[0];
        private string[] _trendLabels = new string[0];

        public DashboardHomeControl() : this(null, null, null, null) { }

        public DashboardHomeControl(StudentService studentService, FacultyService facultyService, CourseService courseService, SubjectService subjectService)
        {
            _studentService = studentService;
            _facultyService = facultyService;
            _courseService = courseService;
            _subjectService = subjectService;

            BuildUi();
            LoadStats();
        }

        private void BuildUi()
        {
            BackColor = ThemeColors.Background;
            UiHelper.EnableDoubleBuffering(this);

            _rootLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = ThemeColors.Background, Padding = new Padding(0, 4, 0, 0) };
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 136));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 178));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _cardsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 0, 14) };
            _analyticsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };

            _statCards.Add(CreateStatCard("Students", "Active records", IconKind.Students, ThemeColors.ChartGreen, out _lblStudentsValue));
            _statCards.Add(CreateStatCard("Faculty", "Teaching staff", IconKind.Faculty, ThemeColors.ChartBlue, out _lblFacultyValue));
            _statCards.Add(CreateStatCard("Courses", "Available programs", IconKind.Courses, ThemeColors.ChartOrange, out _lblCoursesValue));
            _statCards.Add(CreateStatCard("Subjects", "Course catalog", IconKind.Subjects, ThemeColors.ChartRed, out _lblSubjectsValue));

            _rootLayout.Controls.Add(CreateHeroCard(), 0, 0);
            _rootLayout.Controls.Add(CreateSectionHeader(), 0, 1);
            _rootLayout.Controls.Add(_cardsLayout, 0, 2);
            _rootLayout.Controls.Add(CreateAnalyticsLayout(), 0, 3);

            Controls.Clear();
            Controls.Add(_rootLayout);

            Resize += (s, e) => ApplyResponsiveLayout();
            ApplyResponsiveLayout();
        }

        private Control CreateHeroCard()
        {
            var host = CreateHost(new Padding(0, 0, 0, 14));
            var card = CreateCard();
            var accent = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = ThemeColors.Secondary };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(24, 18, 24, 18) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Margin = new Padding(0) };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Dashboard overview", Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), ForeColor = ThemeColors.Text, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) }, 0, 0);
            left.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "A cleaner operational view for the current database with live counts and quick trends.", Font = new Font("Segoe UI", 10F), ForeColor = ThemeColors.MutedText, TextAlign = ContentAlignment.TopLeft, Margin = new Padding(0, 2, 0, 0) }, 0, 1);

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Margin = new Padding(0) };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var chips = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 4), Margin = new Padding(0) };
            chips.Controls.Add(CreateChip(DateTime.Now.ToString("MMM dd, yyyy"), ThemeColors.Secondary, ThemeColors.Text, DockStyle.None, 0, 28));
            chips.Controls.Add(CreateChip("Live overview", ThemeColors.ChartGreen, ThemeColors.Text, DockStyle.None, 0, 28));
            _lblHeroTotal = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), ForeColor = ThemeColors.Text, TextAlign = ContentAlignment.MiddleRight, Text = "0 tracked records", Margin = new Padding(0) };
            right.Controls.Add(chips, 0, 0);
            right.Controls.Add(_lblHeroTotal, 0, 1);

            layout.Controls.Add(left, 0, 0);
            layout.Controls.Add(right, 1, 0);
            card.Controls.Add(layout);
            card.Controls.Add(accent);
            host.Controls.Add(card);
            return host;
        }

        private Control CreateSectionHeader()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(2, 0, 2, 0) };
            panel.Controls.Add(new Label { Dock = DockStyle.Left, Width = 240, Text = "Overview analytics", Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold), ForeColor = ThemeColors.Text, TextAlign = ContentAlignment.MiddleLeft });
            return panel;
        }

        private Control CreateStatCard(string title, string subtitle, IconKind iconKind, Color accent, out Label valueLabel)
        {
            var host = CreateHost(new Padding(0, 0, 14, 0));
            var card = CreateCard();
            var strip = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = accent };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(18, 16, 18, 16) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var icon = new PictureBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), SizeMode = PictureBoxSizeMode.CenterImage, Image = IconFactory.CreateCircleIcon(accent, iconKind, 54) };
            var text = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent, Margin = new Padding(0) };
            text.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            text.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            text.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            text.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            text.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            text.Controls.Add(new Label { Dock = DockStyle.Fill, Text = title, Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold), ForeColor = ThemeColors.MutedText, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) }, 0, 0);
            valueLabel = new Label { Dock = DockStyle.Fill, Text = "0", Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold), ForeColor = ThemeColors.Text, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
            text.Controls.Add(valueLabel, 0, 1);
            text.Controls.Add(new Label { Dock = DockStyle.Fill, Text = subtitle, Font = new Font("Segoe UI", 9.5F), ForeColor = ThemeColors.MutedText, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) }, 0, 2);
            var chipHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0), Margin = new Padding(0) };
            chipHost.Controls.Add(CreateChip("Current", Blend(accent, 0.88f), accent, DockStyle.Left, 84, 26));
            text.Controls.Add(chipHost, 0, 3);

            layout.Controls.Add(icon, 0, 0);
            layout.Controls.Add(text, 1, 0);
            card.Controls.Add(layout);
            card.Controls.Add(strip);
            host.Controls.Add(card);
            return host;
        }

        private Control CreateAnalyticsLayout()
        {
            var lineCard = CreateCard();
            lineCard.Controls.Add(_lineChartPanel = CreateChartSurface());
            lineCard.Controls.Add(CreateChartHeader("Enrollment trend", "Last 6 months projection", IconKind.Trend, ThemeColors.ChartBlue, "Projection"));
            _lineChartPanel.Paint += DrawLineChart;

            var pieCard = CreateCard();
            pieCard.Controls.Add(_pieChartPanel = CreateChartSurface());
            pieCard.Controls.Add(CreateChartHeader("Distribution", "Current active records", IconKind.Pie, ThemeColors.Secondary, "Live"));
            _pieChartPanel.Paint += DrawPieChart;

            _lineChartHost = CreateHost(new Padding(0, 0, 14, 0), lineCard);
            _pieChartHost = CreateHost(new Padding(0), pieCard);
            _analyticsLayout.Controls.Add(_lineChartHost, 0, 0);
            _analyticsLayout.Controls.Add(_pieChartHost, 1, 0);
            return _analyticsLayout;
        }

        private Panel CreateChartHeader(string title, string subtitle, IconKind iconKind, Color accent, string badge)
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = ThemeColors.CardBackground, Padding = new Padding(18, 14, 18, 10) };
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Blend(ThemeColors.Border, 0.2f) });

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent, Margin = new Padding(0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var icon = new PictureBox { Dock = DockStyle.Fill, Image = IconFactory.CreateGlyphIcon(iconKind, 20, accent), SizeMode = PictureBoxSizeMode.CenterImage, Margin = new Padding(0) };
            var text = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8, 0, 0, 0), BackColor = Color.Transparent, Margin = new Padding(0) };
            text.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            text.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            text.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            text.Controls.Add(new Label { Dock = DockStyle.Fill, Text = title, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = ThemeColors.Text, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) }, 0, 0);
            text.Controls.Add(new Label { Dock = DockStyle.Fill, Text = subtitle, Font = new Font("Segoe UI", 9.3F), ForeColor = ThemeColors.MutedText, TextAlign = ContentAlignment.TopLeft, Margin = new Padding(0) }, 0, 1);
            var badgeHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0) };
            badgeHost.Controls.Add(CreateChip(badge, Blend(accent, 0.9f), accent, DockStyle.Right, 94, 30));

            layout.Controls.Add(icon, 0, 0);
            layout.Controls.Add(text, 1, 0);
            layout.Controls.Add(badgeHost, 2, 0);
            header.Controls.Add(layout);
            return header;
        }

        private Panel CreateChartSurface()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(16, 8, 16, 16) };
            UiHelper.EnableDoubleBuffering(panel);
            return panel;
        }

        private void ApplyResponsiveLayout()
        {
            var width = Math.Max(ClientSize.Width, Width);
            var columns = width >= 1500 ? 4 : width >= 980 ? 2 : 1;
            var stackCharts = width < 1360;

            _cardsLayout.SuspendLayout();
            _cardsLayout.Controls.Clear();
            _cardsLayout.ColumnStyles.Clear();
            _cardsLayout.RowStyles.Clear();
            _cardsLayout.ColumnCount = columns;
            for (var i = 0; i < columns; i++) _cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            var rows = (int)Math.Ceiling(_statCards.Count / (double)columns);
            _cardsLayout.RowCount = rows;
            for (var i = 0; i < rows; i++) _cardsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            for (var i = 0; i < _statCards.Count; i++) _cardsLayout.Controls.Add(_statCards[i], i % columns, i / columns);
            _rootLayout.RowStyles[2].Height = rows * 178;
            _cardsLayout.ResumeLayout();

            _analyticsLayout.SuspendLayout();
            _analyticsLayout.Controls.Clear();
            _analyticsLayout.ColumnStyles.Clear();
            _analyticsLayout.RowStyles.Clear();
            if (stackCharts)
            {
                _analyticsLayout.ColumnCount = 1;
                _analyticsLayout.RowCount = 2;
                _analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                _analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
                _analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            }
            else
            {
                _analyticsLayout.ColumnCount = 2;
                _analyticsLayout.RowCount = 1;
                _analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63));
                _analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
                _analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            }
            if (_lineChartHost != null)
            {
                _analyticsLayout.Controls.Add(_lineChartHost, 0, 0);
            }

            if (_pieChartHost != null)
            {
                _analyticsLayout.Controls.Add(_pieChartHost, stackCharts ? 0 : 1, stackCharts ? 1 : 0);
            }
            _analyticsLayout.ResumeLayout();
        }

        private void LoadStats()
        {
            _studentsCount = LoadCount(_lblStudentsValue, () => _studentService == null ? 0 : _studentService.GetActiveCount());
            _facultyCount = LoadCount(_lblFacultyValue, () => _facultyService == null ? 0 : _facultyService.GetActiveCount());
            _coursesCount = LoadCount(_lblCoursesValue, () => _courseService == null ? 0 : _courseService.GetActiveCount());
            _subjectsCount = LoadCount(_lblSubjectsValue, () => _subjectService == null ? 0 : _subjectService.GetActiveCount());
            _lblHeroTotal.Text = (_studentsCount + _facultyCount + _coursesCount + _subjectsCount).ToString("N0") + " tracked records";

            var baseValue = Math.Max(12, (_studentsCount * 2) + _facultyCount + _coursesCount + (_subjectsCount / 2));
            var step = Math.Max(2, (int)Math.Ceiling(baseValue * 0.09));
            _trendLabels = Enumerable.Range(0, 6).Select(i => DateTime.Today.AddMonths(i - 5).ToString("MMM")).ToArray();
            _trendValues = Enumerable.Range(0, 6).Select(i =>
            {
                var trend = baseValue + ((i - 2) * step) + (i % 2 == 0 ? step / 2 : -(step / 3));
                return i >= 4 ? trend + step : trend;
            }).Select(v => Math.Max(0, v)).ToArray();

            _pieEntries[0] = new PieEntry("Students", _studentsCount, ThemeColors.ChartGreen);
            _pieEntries[1] = new PieEntry("Faculty", _facultyCount, ThemeColors.ChartBlue);
            _pieEntries[2] = new PieEntry("Courses", _coursesCount, ThemeColors.ChartOrange);
            _pieEntries[3] = new PieEntry("Subjects", _subjectsCount, ThemeColors.ChartRed);

            _lineChartPanel.Invalidate();
            _pieChartPanel.Invalidate();
        }

        private static int LoadCount(Label label, Func<int> getCount)
        {
            try
            {
                var count = Math.Max(0, getCount == null ? 0 : getCount());
                label.Text = count.ToString("N0");
                return count;
            }
            catch
            {
                label.Text = "-";
                return 0;
            }
        }

        private void DrawLineChart(object sender, PaintEventArgs e)
        {
            if (_trendValues.Length < 2) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(ThemeColors.CardBackground);

            var area = new Rectangle(56, 18, _lineChartPanel.ClientSize.Width - 82, _lineChartPanel.ClientSize.Height - 76);
            if (area.Width < 100 || area.Height < 100) return;

            using (var brush = new SolidBrush(ThemeColors.Surface))
            using (var pen = new Pen(Blend(ThemeColors.Border, 0.1f)))
            {
                e.Graphics.FillRectangle(brush, area);
                e.Graphics.DrawRectangle(pen, area);
            }

            var max = Math.Max(1, _trendValues.Max());
            using (var grid = new Pen(Blend(ThemeColors.Border, 0.5f)))
            using (var axis = new Pen(Blend(ThemeColors.Border, 0.15f)))
            using (var text = new SolidBrush(ThemeColors.MutedText))
            using (var line = new Pen(ThemeColors.ChartBlue, 3.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var fill = new SolidBrush(Color.FromArgb(52, ThemeColors.ChartBlue)))
            using (var point = new SolidBrush(ThemeColors.ChartBlue))
            using (var ring = new Pen(Color.White, 2f))
            {
                for (var i = 0; i <= 4; i++)
                {
                    var y = area.Bottom - ((float)i / 4f * area.Height);
                    e.Graphics.DrawLine(i == 0 ? axis : grid, area.Left, y, area.Right, y);
                    var value = (max * i / 4).ToString("N0");
                    var size = e.Graphics.MeasureString(value, ThemeFonts.Label);
                    e.Graphics.DrawString(value, ThemeFonts.Label, text, area.Left - size.Width - 10, y - size.Height / 2);
                }

                var points = Enumerable.Range(0, _trendValues.Length)
                    .Select(i => new PointF(area.Left + ((float)i / (_trendValues.Length - 1) * area.Width), area.Bottom - ((_trendValues[i] / (float)max) * area.Height)))
                    .ToArray();
                var fillPoints = new List<PointF> { new PointF(points[0].X, area.Bottom) };
                fillPoints.AddRange(points);
                fillPoints.Add(new PointF(points[points.Length - 1].X, area.Bottom));
                e.Graphics.FillPolygon(fill, fillPoints.ToArray());
                e.Graphics.DrawCurve(line, points, 0.35f);

                for (var i = 0; i < points.Length; i++)
                {
                    e.Graphics.FillEllipse(point, points[i].X - 5f, points[i].Y - 5f, 10f, 10f);
                    e.Graphics.DrawEllipse(ring, points[i].X - 5f, points[i].Y - 5f, 10f, 10f);
                    var size = e.Graphics.MeasureString(_trendLabels[i], ThemeFonts.Label);
                    e.Graphics.DrawString(_trendLabels[i], ThemeFonts.Label, text, points[i].X - size.Width / 2, area.Bottom + 10);
                }
            }
        }

        private void DrawPieChart(object sender, PaintEventArgs e)
        {
            var total = _pieEntries.Sum(x => x.Value);
            if (total <= 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(ThemeColors.CardBackground);

            var size = Math.Min(182, Math.Min(_pieChartPanel.ClientSize.Width - 170, _pieChartPanel.ClientSize.Height - 44));
            if (size < 120) size = 120;
            var donut = new Rectangle(16, 24, size, size);
            var angle = -90f;

            foreach (var entry in _pieEntries.Where(x => x.Value > 0))
            {
                var sweep = (float)entry.Value / total * 360f;
                using (var brush = new SolidBrush(entry.Color)) e.Graphics.FillPie(brush, donut, angle, sweep);
                angle += sweep;
            }

            var inner = Rectangle.Inflate(donut, -(int)(donut.Width * 0.28f), -(int)(donut.Height * 0.28f));
            using (var brush = new SolidBrush(ThemeColors.CardBackground)) e.Graphics.FillEllipse(brush, inner);
            using (var titleBrush = new SolidBrush(ThemeColors.Text))
            using (var mutedBrush = new SolidBrush(ThemeColors.MutedText))
            {
                var totalFont = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
                var totalText = total.ToString("N0");
                var totalSize = e.Graphics.MeasureString(totalText, totalFont);
                e.Graphics.DrawString(totalText, totalFont, titleBrush, inner.X + (inner.Width - totalSize.Width) / 2f, inner.Y + 18);
                var labelSize = e.Graphics.MeasureString("records", ThemeFonts.Label);
                e.Graphics.DrawString("records", ThemeFonts.Label, mutedBrush, inner.X + (inner.Width - labelSize.Width) / 2f, inner.Y + 44);
            }

            var y = donut.Top + 4;
            using (var labelBrush = new SolidBrush(ThemeColors.Text))
            using (var mutedBrush = new SolidBrush(ThemeColors.MutedText))
            {
                foreach (var entry in _pieEntries.Where(x => x.Value > 0))
                {
                    using (var dot = new SolidBrush(entry.Color))
                    {
                        e.Graphics.FillEllipse(dot, donut.Right + 20, y + 7, 10, 10);
                    }

                    e.Graphics.DrawString(entry.Label, new Font("Segoe UI Semibold", 9.8F, FontStyle.Bold), labelBrush, donut.Right + 38, y);
                    e.Graphics.DrawString(entry.Value.ToString("N0") + "  |  " + string.Format("{0:P0}", (double)entry.Value / total), ThemeFonts.Label, mutedBrush, donut.Right + 38, y + 18);
                    y += 42;
                }
            }
        }

        private Panel CreateHost(Padding padding)
        {
            return CreateHost(padding, null);
        }

        private Panel CreateHost(Padding padding, Control child)
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = padding };
            if (child != null) host.Controls.Add(child);
            return host;
        }

        private Panel CreateCard()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(0) };
            ThemeManager.StyleCardPanel(panel);
            return panel;
        }

        private Panel CreateChip(string text, Color background)
        {
            return CreateChip(text, background, ThemeColors.Text, DockStyle.None, 0, 28);
        }

        private Panel CreateChip(string text, Color background, Color foreground, DockStyle dock, int width, int height)
        {
            var panel = new Panel
            {
                BackColor = background,
                Margin = dock == DockStyle.None ? new Padding(8, 0, 0, 0) : Padding.Empty,
                Padding = new Padding(10, 4, 10, 4),
                Dock = dock,
                Height = Math.Max(24, height)
            };
            if (width > 0) panel.Width = width;
            UiHelper.ApplyRoundedCorners(panel, 12);
            panel.Controls.Add(new Label { Dock = DockStyle.Fill, Text = text, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), ForeColor = foreground, TextAlign = ContentAlignment.MiddleCenter });
            return panel;
        }

        private static Color Blend(Color color, double amount)
        {
            amount = Math.Max(0d, Math.Min(1d, amount));
            return Color.FromArgb(255,
                (int)Math.Round((255 - color.R) * amount + color.R),
                (int)Math.Round((255 - color.G) * amount + color.G),
                (int)Math.Round((255 - color.B) * amount + color.B));
        }

        private struct PieEntry
        {
            public readonly string Label;
            public readonly int Value;
            public readonly Color Color;

            public PieEntry(string label, int value, Color color)
            {
                Label = label;
                Value = value;
                Color = color;
            }
        }
    }
}
