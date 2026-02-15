using System.Drawing;
using System.Drawing.Drawing2D;

namespace School_Management_System.Presentation.Helpers
{
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
    }
}

