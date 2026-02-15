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

        private Label _lblStudentsValue;
        private Label _lblFacultyValue;
        private Label _lblCoursesValue;
        private Label _lblSubjectsValue;

        private Panel _lineChartPanel;
        private Panel _pieChartPanel;

        private int _studentsCount;
        private int _facultyCount;
        private int _coursesCount;
        private int _subjectsCount;

        private int[] _trendValues = new int[0];
        private string[] _trendLabels = new string[0];

        private readonly PieEntry[] _pieEntries = new PieEntry[4];

        public DashboardHomeControl()
            : this(null, null, null, null)
        {
        }

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

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = ThemeColors.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 148));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new Label
            {
                Text = "Overview Analytics",
                Dock = DockStyle.Fill,
                Height = 34,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = ThemeColors.Background,
                Padding = new Padding(0, 0, 0, 10)
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            cards.Controls.Add(CreateStatCard("Students", "Active records", IconKind.Students, ThemeColors.Success, out _lblStudentsValue), 0, 0);
            cards.Controls.Add(CreateStatCard("Faculty", "Teaching staff", IconKind.Faculty, ThemeColors.Secondary, out _lblFacultyValue), 1, 0);
            cards.Controls.Add(CreateStatCard("Courses", "Available programs", IconKind.Courses, ThemeColors.ChartOrange, out _lblCoursesValue), 2, 0);
            cards.Controls.Add(CreateStatCard("Subjects", "Course subjects", IconKind.Subjects, ThemeColors.ChartRed, out _lblSubjectsValue), 3, 0);

            var analytics = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeColors.Background,
                Padding = new Padding(0, 4, 0, 0)
            };
            analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            analytics.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            analytics.Controls.Add(CreateLineChartCard(), 0, 0);
            analytics.Controls.Add(CreatePieChartCard(), 1, 0);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(cards, 0, 1);
            root.Controls.Add(analytics, 0, 2);

            Controls.Clear();
            Controls.Add(root);
        }

        private Control CreateStatCard(string title, string subtitle, IconKind iconKind, Color iconColor, out Label valueLabel)
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 12, 0) };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(16, 14, 16, 14) };
            ThemeManager.StyleCardPanel(card);

            var icon = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(14, 18),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconFactory.CreateCircleIcon(iconColor, iconKind, 50)
            };

            var lblTitle = new Label
            {
                Text = title ?? string.Empty,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Location = new Point(76, 18),
                AutoSize = true
            };

            valueLabel = new Label
            {
                Text = "0",
                Font = ThemeFonts.Header,
                ForeColor = ThemeColors.Text,
                Location = new Point(76, 38),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = subtitle ?? string.Empty,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                Location = new Point(76, 72),
                AutoSize = true
            };

            card.Controls.Add(icon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);
            card.Controls.Add(lblSubtitle);
            host.Controls.Add(card);
            return host;
        }

        private Control CreateLineChartCard()
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(0, 0, 12, 0) };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(0) };
            ThemeManager.StyleCardPanel(card);

            var header = BuildChartHeader("Enrollment Trend", "Last 6 months projection", IconKind.Trend);
            _lineChartPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10, 0, 10, 10) };
            UiHelper.EnableDoubleBuffering(_lineChartPanel);
            _lineChartPanel.Paint += DrawLineChart;

            card.Controls.Add(_lineChartPanel);
            card.Controls.Add(header);
            host.Controls.Add(card);
            return host;
        }

        private Control CreatePieChartCard()
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(0) };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(0) };
            ThemeManager.StyleCardPanel(card);

            var header = BuildChartHeader("Distribution", "Current active records", IconKind.Pie);
            _pieChartPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(10, 0, 10, 10) };
            UiHelper.EnableDoubleBuffering(_pieChartPanel);
            _pieChartPanel.Paint += DrawPieChart;

            card.Controls.Add(_pieChartPanel);
            card.Controls.Add(header);
            host.Controls.Add(card);
            return host;
        }

        private static Panel BuildChartHeader(string title, string subtitle, IconKind iconKind)
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColors.CardBackground, Padding = new Padding(14, 10, 14, 8) };
            var icon = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 26,
                Image = IconFactory.CreateGlyphIcon(iconKind, 20, ThemeColors.Secondary),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            var textHost = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(6, 0, 0, 0) };
            var lblTitle = new Label
            {
                Text = title ?? string.Empty,
                Dock = DockStyle.Top,
                Height = 24,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblSubtitle = new Label
            {
                Text = subtitle ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.TopLeft
            };

            textHost.Controls.Add(lblSubtitle);
            textHost.Controls.Add(lblTitle);
            header.Controls.Add(textHost);
            header.Controls.Add(icon);
            return header;
        }

        private void LoadStats()
        {
            _studentsCount = LoadCount(_lblStudentsValue, () => _studentService == null ? 0 : _studentService.GetActiveCount());
            _facultyCount = LoadCount(_lblFacultyValue, () => _facultyService == null ? 0 : _facultyService.GetActiveCount());
            _coursesCount = LoadCount(_lblCoursesValue, () => _courseService == null ? 0 : _courseService.GetActiveCount());
            _subjectsCount = LoadCount(_lblSubjectsValue, () => _subjectService == null ? 0 : _subjectService.GetActiveCount());

            BuildChartData();
            InvalidateCharts();
        }

        private static int LoadCount(Label label, Func<int> countProvider)
        {
            if (label == null) return 0;

            try
            {
                var count = Math.Max(0, countProvider == null ? 0 : countProvider());
                label.Text = count.ToString();
                return count;
            }
            catch
            {
                label.Text = "-";
                return 0;
            }
        }

        private void BuildChartData()
        {
            var baseValue = Math.Max(3, (_studentsCount * 2) + _facultyCount + _coursesCount + (_subjectsCount / 2));
            var step = Math.Max(1, (int)Math.Ceiling(baseValue * 0.09));

            _trendValues = new int[6];
            _trendLabels = new string[6];

            for (var i = 0; i < 6; i++)
            {
                _trendLabels[i] = DateTime.Today.AddMonths(i - 5).ToString("MMM");

                var trend = baseValue + ((i - 2) * step);
                if (i % 2 == 0)
                {
                    trend += step / 2;
                }
                else
                {
                    trend -= step / 3;
                }

                if (trend < 0) trend = 0;
                _trendValues[i] = trend;
            }

            _pieEntries[0] = new PieEntry("Students", _studentsCount, ThemeColors.ChartGreen);
            _pieEntries[1] = new PieEntry("Faculty", _facultyCount, ThemeColors.ChartBlue);
            _pieEntries[2] = new PieEntry("Courses", _coursesCount, ThemeColors.ChartOrange);
            _pieEntries[3] = new PieEntry("Subjects", _subjectsCount, ThemeColors.ChartRed);
        }

        private void InvalidateCharts()
        {
            if (_lineChartPanel != null) _lineChartPanel.Invalidate();
            if (_pieChartPanel != null) _pieChartPanel.Invalidate();
        }

        private void DrawLineChart(object sender, PaintEventArgs e)
        {
            if (_lineChartPanel == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(ThemeColors.CardBackground);

            var area = new Rectangle(44, 18, _lineChartPanel.ClientSize.Width - 64, _lineChartPanel.ClientSize.Height - 52);
            if (area.Width < 80 || area.Height < 80) return;

            using (var areaBrush = new SolidBrush(ThemeColors.Surface))
            using (var areaPen = new Pen(ThemeColors.Border))
            {
                e.Graphics.FillRectangle(areaBrush, area);
                e.Graphics.DrawRectangle(areaPen, area);
            }

            var maxValue = _trendValues.Length == 0 ? 1 : Math.Max(1, _trendValues.Max());

            using (var gridPen = new Pen(ThemeColors.SurfaceAlt))
            using (var axisPen = new Pen(ThemeColors.Border, 1.2f))
            using (var textBrush = new SolidBrush(ThemeColors.MutedText))
            using (var linePen = new Pen(ThemeColors.ChartBlue, 3f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var dotBrush = new SolidBrush(ThemeColors.Secondary))
            {
                for (var i = 0; i <= 4; i++)
                {
                    var y = area.Bottom - ((float)i / 4f * area.Height);
                    e.Graphics.DrawLine(i == 0 ? axisPen : gridPen, area.Left, y, area.Right, y);

                    var label = ((int)Math.Round(maxValue * (i / 4.0))).ToString();
                    e.Graphics.DrawString(label, ThemeFonts.Label, textBrush, 8, y - 8);
                }

                if (_trendValues.Length >= 2)
                {
                    var points = new PointF[_trendValues.Length];
                    for (var i = 0; i < _trendValues.Length; i++)
                    {
                        var x = area.Left + ((float)i / (_trendValues.Length - 1) * area.Width);
                        var y = area.Bottom - ((_trendValues[i] / (float)maxValue) * area.Height);
                        points[i] = new PointF(x, y);
                    }

                    e.Graphics.DrawCurve(linePen, points);
                    foreach (var p in points)
                    {
                        e.Graphics.FillEllipse(dotBrush, p.X - 4f, p.Y - 4f, 8f, 8f);
                    }

                    for (var i = 0; i < _trendLabels.Length; i++)
                    {
                        var x = area.Left + ((float)i / (_trendLabels.Length - 1) * area.Width);
                        var labelSize = e.Graphics.MeasureString(_trendLabels[i], ThemeFonts.Label);
                        e.Graphics.DrawString(_trendLabels[i], ThemeFonts.Label, textBrush, x - (labelSize.Width / 2f), area.Bottom + 8);
                    }
                }
            }
        }

        private void DrawPieChart(object sender, PaintEventArgs e)
        {
            if (_pieChartPanel == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(ThemeColors.CardBackground);

            var total = _pieEntries.Sum(x => x.Value);
            if (total <= 0)
            {
                using (var brush = new SolidBrush(ThemeColors.MutedText))
                {
                    e.Graphics.DrawString("No data available", ThemeFonts.Label, brush, 16, 24);
                }
                return;
            }

            var pieBounds = new Rectangle(10, 16, Math.Min(170, _pieChartPanel.ClientSize.Width - 140), Math.Min(170, _pieChartPanel.ClientSize.Height - 36));
            if (pieBounds.Width < 90 || pieBounds.Height < 90)
            {
                pieBounds = new Rectangle(10, 16, 120, 120);
            }

            var startAngle = -90f;
            foreach (var entry in _pieEntries)
            {
                if (entry.Value <= 0) continue;
                var sweep = (float)entry.Value / total * 360f;

                using (var brush = new SolidBrush(entry.Color))
                {
                    e.Graphics.FillPie(brush, pieBounds, startAngle, sweep);
                }
                startAngle += sweep;
            }

            using (var pen = new Pen(Color.White, 2f))
            {
                e.Graphics.DrawEllipse(pen, pieBounds);
            }

            DrawLegend(e.Graphics, total, pieBounds.Right + 14, 20);
        }

        private void DrawLegend(Graphics g, int total, int startX, int startY)
        {
            using (var textBrush = new SolidBrush(ThemeColors.Text))
            using (var mutedBrush = new SolidBrush(ThemeColors.MutedText))
            {
                var y = startY;
                foreach (var entry in _pieEntries)
                {
                    if (entry.Value <= 0) continue;

                    using (var colorBrush = new SolidBrush(entry.Color))
                    {
                        g.FillRectangle(colorBrush, startX, y + 5, 12, 12);
                    }

                    g.DrawString(entry.Label, ThemeFonts.Label, textBrush, startX + 18, y);
                    var pct = string.Format("{0:P0}", (double)entry.Value / Math.Max(1, total));
                    g.DrawString(pct, ThemeFonts.Label, mutedBrush, startX + 18, y + 16);
                    y += 38;
                }
            }
        }

        private struct PieEntry
        {
            public readonly string Label;
            public readonly int Value;
            public readonly Color Color;

            public PieEntry(string label, int value, Color color)
            {
                Label = label ?? string.Empty;
                Value = value;
                Color = color;
            }
        }
    }
}
