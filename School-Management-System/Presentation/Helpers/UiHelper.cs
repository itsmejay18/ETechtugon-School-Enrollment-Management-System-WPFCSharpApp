using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace School_Management_System.Presentation.Helpers
{
    public static class UiHelper
    {
        public static void ApplyRoundedCorners(Control control, int radius)
        {
            if (control == null) return;
            if (radius <= 0) return;

            void apply()
            {
                var rect = new Rectangle(0, 0, control.Width, control.Height);
                if (rect.Width <= 0 || rect.Height <= 0) return;

                using (var path = GetRoundedRectPath(rect, radius))
                {
                    control.Region = new Region(path);
                }
            }

            control.SizeChanged += (s, e) => apply();
            apply();
        }

        public static GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(d, d));

            // top left
            path.AddArc(arc, 180, 90);

            // top right
            arc.X = bounds.Right - d;
            path.AddArc(arc, 270, 90);

            // bottom right
            arc.Y = bounds.Bottom - d;
            path.AddArc(arc, 0, 90);

            // bottom left
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;

            var pi = control.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (pi != null)
            {
                pi.SetValue(control, true, null);
            }
        }
    }
}

