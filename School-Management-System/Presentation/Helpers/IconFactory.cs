using System.Drawing;
using System.Drawing.Drawing2D;

namespace School_Management_System.Presentation.Helpers
{
    public enum IconKind
    {
        Dashboard,
        Students,
        Faculty,
        Courses,
        Subjects,
        Curriculum,
        Enrollment,
        Users,
        Logout,
        Login,
        Register,
        Trend,
        Pie,
        School
    }

    public static class IconFactory
    {
        public static Bitmap CreateCircleIcon(Color background, string text, int size)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(background))
                {
                    g.FillEllipse(brush, 0, 0, size - 1, size - 1);
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    using (var font = new Font("Segoe UI Semibold", size * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var textBrush = new SolidBrush(Color.White))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString(text.Trim().Substring(0, 1).ToUpperInvariant(), font, textBrush, new RectangleF(0, 0, size, size), sf);
                    }
                }
            }

            return bmp;
        }

        public static Bitmap CreateCircleIcon(Color background, IconKind kind, int size)
        {
            var safeSize = size < 16 ? 16 : size;
            var bmp = new Bitmap(safeSize, safeSize);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(background))
                {
                    g.FillEllipse(brush, 0, 0, safeSize - 1, safeSize - 1);
                }

                var iconInset = safeSize * 0.2f;
                DrawIcon(g, kind, new RectangleF(iconInset, iconInset, safeSize - (iconInset * 2f), safeSize - (iconInset * 2f)), Color.White);
            }

            return bmp;
        }

        public static Bitmap CreateGlyphIcon(IconKind kind, int size, Color color)
        {
            var safeSize = size < 14 ? 14 : size;
            var bmp = new Bitmap(safeSize, safeSize);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                DrawIcon(g, kind, new RectangleF(1f, 1f, safeSize - 2f, safeSize - 2f), color);
            }

            return bmp;
        }

        private static void DrawIcon(Graphics g, IconKind kind, RectangleF bounds, Color color)
        {
            var stroke = bounds.Width * 0.1f;
            if (stroke < 1.6f) stroke = 1.6f;

            using (var pen = new Pen(color, stroke) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (kind)
                {
                    case IconKind.Dashboard:
                        DrawDashboard(g, pen, brush, bounds);
                        break;
                    case IconKind.Students:
                        DrawPeople(g, pen, brush, bounds, false);
                        break;
                    case IconKind.Faculty:
                        DrawPeople(g, pen, brush, bounds, true);
                        break;
                    case IconKind.Courses:
                        DrawCourses(g, pen, brush, bounds);
                        break;
                    case IconKind.Subjects:
                        DrawSubjects(g, pen, brush, bounds);
                        break;
                    case IconKind.Curriculum:
                        DrawCurriculum(g, pen, brush, bounds);
                        break;
                    case IconKind.Enrollment:
                        DrawEnrollment(g, pen, brush, bounds);
                        break;
                    case IconKind.Users:
                        DrawUsers(g, pen, brush, bounds);
                        break;
                    case IconKind.Logout:
                        DrawLogArrow(g, pen, brush, bounds, true);
                        break;
                    case IconKind.Login:
                        DrawLogArrow(g, pen, brush, bounds, false);
                        break;
                    case IconKind.Register:
                        DrawRegister(g, pen, brush, bounds);
                        break;
                    case IconKind.Trend:
                        DrawTrend(g, pen, brush, bounds);
                        break;
                    case IconKind.Pie:
                        DrawPie(g, pen, brush, bounds);
                        break;
                    case IconKind.School:
                        DrawSchool(g, pen, brush, bounds);
                        break;
                    default:
                        DrawDashboard(g, pen, brush, bounds);
                        break;
                }
            }
        }

        private static void DrawDashboard(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            var gap = b.Width * 0.08f;
            var cell = (b.Width - gap) / 2f;

            g.FillRectangle(brush, b.Left, b.Top, cell, cell);
            g.FillRectangle(brush, b.Left + cell + gap, b.Top, cell, cell);
            g.FillRectangle(brush, b.Left, b.Top + cell + gap, cell, cell);
            g.FillRectangle(brush, b.Left + cell + gap, b.Top + cell + gap, cell, cell);
        }

        private static void DrawPeople(Graphics g, Pen pen, Brush brush, RectangleF b, bool withTie)
        {
            var head = b.Width * 0.22f;
            g.FillEllipse(brush, b.Left + b.Width * 0.39f, b.Top + b.Height * 0.05f, head, head);
            g.DrawArc(pen, b.Left + b.Width * 0.22f, b.Top + b.Height * 0.30f, b.Width * 0.56f, b.Height * 0.50f, 205, 130);

            g.FillEllipse(brush, b.Left + b.Width * 0.08f, b.Top + b.Height * 0.15f, b.Width * 0.17f, b.Width * 0.17f);
            g.FillEllipse(brush, b.Left + b.Width * 0.75f, b.Top + b.Height * 0.15f, b.Width * 0.17f, b.Width * 0.17f);

            if (withTie)
            {
                var tie = new PointF[]
                {
                    new PointF(b.Left + b.Width * 0.50f, b.Top + b.Height * 0.42f),
                    new PointF(b.Left + b.Width * 0.57f, b.Top + b.Height * 0.58f),
                    new PointF(b.Left + b.Width * 0.50f, b.Top + b.Height * 0.78f),
                    new PointF(b.Left + b.Width * 0.43f, b.Top + b.Height * 0.58f)
                };
                g.FillPolygon(brush, tie);
            }
        }

        private static void DrawCourses(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            var h = b.Height * 0.2f;
            g.DrawRectangle(pen, b.Left + b.Width * 0.14f, b.Top + b.Height * 0.16f, b.Width * 0.62f, h);
            g.DrawRectangle(pen, b.Left + b.Width * 0.20f, b.Top + b.Height * 0.40f, b.Width * 0.62f, h);
            g.DrawRectangle(pen, b.Left + b.Width * 0.26f, b.Top + b.Height * 0.64f, b.Width * 0.62f, h);
            g.FillRectangle(brush, b.Left + b.Width * 0.74f, b.Top + b.Height * 0.16f, b.Width * 0.02f, h);
            g.FillRectangle(brush, b.Left + b.Width * 0.80f, b.Top + b.Height * 0.40f, b.Width * 0.02f, h);
            g.FillRectangle(brush, b.Left + b.Width * 0.86f, b.Top + b.Height * 0.64f, b.Width * 0.02f, h);
        }

        private static void DrawSubjects(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            var left = new PointF[]
            {
                new PointF(b.Left + b.Width * 0.14f, b.Top + b.Height * 0.18f),
                new PointF(b.Left + b.Width * 0.48f, b.Top + b.Height * 0.26f),
                new PointF(b.Left + b.Width * 0.48f, b.Top + b.Height * 0.88f),
                new PointF(b.Left + b.Width * 0.14f, b.Top + b.Height * 0.80f)
            };

            var right = new PointF[]
            {
                new PointF(b.Left + b.Width * 0.86f, b.Top + b.Height * 0.18f),
                new PointF(b.Left + b.Width * 0.52f, b.Top + b.Height * 0.26f),
                new PointF(b.Left + b.Width * 0.52f, b.Top + b.Height * 0.88f),
                new PointF(b.Left + b.Width * 0.86f, b.Top + b.Height * 0.80f)
            };

            g.DrawPolygon(pen, left);
            g.DrawPolygon(pen, right);
            g.DrawLine(pen, b.Left + b.Width * 0.5f, b.Top + b.Height * 0.24f, b.Left + b.Width * 0.5f, b.Top + b.Height * 0.88f);
        }

        private static void DrawCurriculum(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            g.DrawRectangle(pen, b.Left + b.Width * 0.2f, b.Top + b.Height * 0.12f, b.Width * 0.6f, b.Height * 0.78f);
            g.DrawRectangle(pen, b.Left + b.Width * 0.38f, b.Top + b.Height * 0.03f, b.Width * 0.24f, b.Height * 0.15f);

            g.DrawLine(pen, b.Left + b.Width * 0.3f, b.Top + b.Height * 0.38f, b.Left + b.Width * 0.7f, b.Top + b.Height * 0.38f);
            g.DrawLine(pen, b.Left + b.Width * 0.3f, b.Top + b.Height * 0.56f, b.Left + b.Width * 0.7f, b.Top + b.Height * 0.56f);
            g.DrawLine(pen, b.Left + b.Width * 0.3f, b.Top + b.Height * 0.74f, b.Left + b.Width * 0.7f, b.Top + b.Height * 0.74f);
            g.FillEllipse(brush, b.Left + b.Width * 0.24f, b.Top + b.Height * 0.35f, b.Width * 0.05f, b.Width * 0.05f);
            g.FillEllipse(brush, b.Left + b.Width * 0.24f, b.Top + b.Height * 0.53f, b.Width * 0.05f, b.Width * 0.05f);
            g.FillEllipse(brush, b.Left + b.Width * 0.24f, b.Top + b.Height * 0.71f, b.Width * 0.05f, b.Width * 0.05f);
        }

        private static void DrawEnrollment(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            g.DrawRectangle(pen, b.Left + b.Width * 0.16f, b.Top + b.Height * 0.12f, b.Width * 0.68f, b.Height * 0.78f);
            g.DrawLine(pen, b.Left + b.Width * 0.30f, b.Top + b.Height * 0.45f, b.Left + b.Width * 0.44f, b.Top + b.Height * 0.58f);
            g.DrawLine(pen, b.Left + b.Width * 0.44f, b.Top + b.Height * 0.58f, b.Left + b.Width * 0.70f, b.Top + b.Height * 0.33f);
            g.DrawLine(pen, b.Left + b.Width * 0.30f, b.Top + b.Height * 0.72f, b.Left + b.Width * 0.70f, b.Top + b.Height * 0.72f);
        }

        private static void DrawUsers(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            g.FillEllipse(brush, b.Left + b.Width * 0.39f, b.Top + b.Height * 0.10f, b.Width * 0.22f, b.Width * 0.22f);
            g.FillEllipse(brush, b.Left + b.Width * 0.18f, b.Top + b.Height * 0.20f, b.Width * 0.18f, b.Width * 0.18f);
            g.FillEllipse(brush, b.Left + b.Width * 0.64f, b.Top + b.Height * 0.20f, b.Width * 0.18f, b.Width * 0.18f);

            g.DrawArc(pen, b.Left + b.Width * 0.27f, b.Top + b.Height * 0.33f, b.Width * 0.46f, b.Height * 0.56f, 205, 130);
            g.DrawArc(pen, b.Left + b.Width * 0.05f, b.Top + b.Height * 0.46f, b.Width * 0.34f, b.Height * 0.40f, 215, 120);
            g.DrawArc(pen, b.Left + b.Width * 0.61f, b.Top + b.Height * 0.46f, b.Width * 0.34f, b.Height * 0.40f, 205, 120);
        }

        private static void DrawLogArrow(Graphics g, Pen pen, Brush brush, RectangleF b, bool isLogout)
        {
            g.DrawRectangle(pen, b.Left + b.Width * 0.16f, b.Top + b.Height * 0.14f, b.Width * 0.42f, b.Height * 0.72f);

            if (isLogout)
            {
                g.DrawLine(pen, b.Left + b.Width * 0.45f, b.Top + b.Height * 0.50f, b.Left + b.Width * 0.86f, b.Top + b.Height * 0.50f);
                g.DrawLine(pen, b.Left + b.Width * 0.72f, b.Top + b.Height * 0.34f, b.Left + b.Width * 0.86f, b.Top + b.Height * 0.50f);
                g.DrawLine(pen, b.Left + b.Width * 0.72f, b.Top + b.Height * 0.66f, b.Left + b.Width * 0.86f, b.Top + b.Height * 0.50f);
            }
            else
            {
                g.DrawLine(pen, b.Left + b.Width * 0.14f, b.Top + b.Height * 0.50f, b.Left + b.Width * 0.56f, b.Top + b.Height * 0.50f);
                g.DrawLine(pen, b.Left + b.Width * 0.30f, b.Top + b.Height * 0.34f, b.Left + b.Width * 0.14f, b.Top + b.Height * 0.50f);
                g.DrawLine(pen, b.Left + b.Width * 0.30f, b.Top + b.Height * 0.66f, b.Left + b.Width * 0.14f, b.Top + b.Height * 0.50f);
            }
        }

        private static void DrawRegister(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            g.FillEllipse(brush, b.Left + b.Width * 0.22f, b.Top + b.Height * 0.14f, b.Width * 0.28f, b.Width * 0.28f);
            g.DrawArc(pen, b.Left + b.Width * 0.08f, b.Top + b.Height * 0.34f, b.Width * 0.56f, b.Height * 0.50f, 205, 130);

            g.DrawLine(pen, b.Left + b.Width * 0.70f, b.Top + b.Height * 0.46f, b.Left + b.Width * 0.92f, b.Top + b.Height * 0.46f);
            g.DrawLine(pen, b.Left + b.Width * 0.81f, b.Top + b.Height * 0.35f, b.Left + b.Width * 0.81f, b.Top + b.Height * 0.57f);
        }

        private static void DrawTrend(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            g.DrawLine(pen, b.Left + b.Width * 0.10f, b.Top + b.Height * 0.85f, b.Left + b.Width * 0.90f, b.Top + b.Height * 0.85f);
            g.DrawLine(pen, b.Left + b.Width * 0.10f, b.Top + b.Height * 0.85f, b.Left + b.Width * 0.10f, b.Top + b.Height * 0.20f);

            var p1 = new PointF(b.Left + b.Width * 0.18f, b.Top + b.Height * 0.72f);
            var p2 = new PointF(b.Left + b.Width * 0.38f, b.Top + b.Height * 0.62f);
            var p3 = new PointF(b.Left + b.Width * 0.57f, b.Top + b.Height * 0.46f);
            var p4 = new PointF(b.Left + b.Width * 0.80f, b.Top + b.Height * 0.30f);
            g.DrawLines(pen, new[] { p1, p2, p3, p4 });
            g.FillEllipse(brush, p4.X - (pen.Width * 0.8f), p4.Y - (pen.Width * 0.8f), pen.Width * 1.6f, pen.Width * 1.6f);
        }

        private static void DrawPie(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            g.DrawEllipse(pen, b.Left + b.Width * 0.12f, b.Top + b.Height * 0.12f, b.Width * 0.76f, b.Width * 0.76f);
            g.FillPie(brush, b.Left + b.Width * 0.12f, b.Top + b.Height * 0.12f, b.Width * 0.76f, b.Width * 0.76f, 300, 80);
            g.DrawLine(pen, b.Left + b.Width * 0.50f, b.Top + b.Height * 0.50f, b.Left + b.Width * 0.82f, b.Top + b.Height * 0.36f);
        }

        private static void DrawSchool(Graphics g, Pen pen, Brush brush, RectangleF b)
        {
            var roof = new PointF[]
            {
                new PointF(b.Left + b.Width * 0.10f, b.Top + b.Height * 0.40f),
                new PointF(b.Left + b.Width * 0.50f, b.Top + b.Height * 0.10f),
                new PointF(b.Left + b.Width * 0.90f, b.Top + b.Height * 0.40f)
            };

            g.DrawPolygon(pen, roof);
            g.DrawRectangle(pen, b.Left + b.Width * 0.18f, b.Top + b.Height * 0.40f, b.Width * 0.64f, b.Height * 0.48f);
            g.DrawRectangle(pen, b.Left + b.Width * 0.44f, b.Top + b.Height * 0.60f, b.Width * 0.12f, b.Height * 0.28f);
            g.FillRectangle(brush, b.Left + b.Width * 0.27f, b.Top + b.Height * 0.52f, b.Width * 0.10f, b.Height * 0.10f);
            g.FillRectangle(brush, b.Left + b.Width * 0.63f, b.Top + b.Height * 0.52f, b.Width * 0.10f, b.Height * 0.10f);
        }
    }
}
