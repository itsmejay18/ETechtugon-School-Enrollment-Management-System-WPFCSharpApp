using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace School_Management_System.Presentation.Helpers
{
    public static class BrandAssets
    {
        private static readonly object SyncRoot = new object();
        private static Bitmap _logo;
        private static bool _loaded;
        private static Icon _appIcon;
        private static bool _appIconLoaded;

        public static string LoadedLogoPath { get; private set; }

        public static bool HasLogo
        {
            get { return GetLogo() != null; }
        }

        public static Image CreateLogoImage()
        {
            var logo = GetLogo();
            return logo == null ? null : new Bitmap(logo);
        }

        public static Icon CreateAppIcon()
        {
            var icon = GetAppIcon();
            return icon == null ? null : (Icon)icon.Clone();
        }

        private static Bitmap GetLogo()
        {
            lock (SyncRoot)
            {
                if (!_loaded)
                {
                    _logo = TryLoadLogo();
                    _loaded = true;
                }

                return _logo;
            }
        }

        private static Icon GetAppIcon()
        {
            lock (SyncRoot)
            {
                if (!_appIconLoaded)
                {
                    _appIcon = TryLoadAppIcon();
                    _appIconLoaded = true;
                }

                return _appIcon;
            }
        }

        private static Bitmap TryLoadLogo()
        {
            foreach (var path in EnumerateLogoCandidates())
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    var bytes = File.ReadAllBytes(path);
                    using (var stream = new MemoryStream(bytes))
                    using (var image = Image.FromStream(stream))
                    using (var raw = new Bitmap(image))
                    {
                        LoadedLogoPath = path;
                        return TrimTransparentPadding(raw);
                    }
                }
                catch
                {
                    // Skip invalid image files and continue scanning.
                }
            }

            return null;
        }

        private static Icon TryLoadAppIcon()
        {
            foreach (var iconPath in EnumerateIconCandidates())
            {
                if (!File.Exists(iconPath))
                {
                    continue;
                }

                try
                {
                    using (var raw = new Icon(iconPath))
                    {
                        return (Icon)raw.Clone();
                    }
                }
                catch
                {
                    // Ignore invalid icon files and continue scanning.
                }
            }

            var logo = GetLogo();
            var generatedIcon = TryCreateIconFromBitmap(logo, 64);
            if (generatedIcon != null)
            {
                return generatedIcon;
            }

            if (logo == null)
            {
                return null;
            }

            return TryCreateIconFromBitmap(logo, 64);
        }

        private static Icon TryCreateIconFromBitmap(Bitmap source, int size)
        {
            if (source == null)
            {
                return null;
            }

            var canvasSize = Math.Max(16, size);
            using (var canvas = new Bitmap(canvasSize, canvasSize))
            using (var g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                var ratio = Math.Min((double)canvasSize / source.Width, (double)canvasSize / source.Height);
                var drawWidth = Math.Max(1, (int)Math.Round(source.Width * ratio));
                var drawHeight = Math.Max(1, (int)Math.Round(source.Height * ratio));
                var x = (canvasSize - drawWidth) / 2;
                var y = (canvasSize - drawHeight) / 2;
                g.DrawImage(source, new Rectangle(x, y, drawWidth, drawHeight));

                var hIcon = canvas.GetHicon();
                try
                {
                    using (var raw = Icon.FromHandle(hIcon))
                    {
                        return (Icon)raw.Clone();
                    }
                }
                finally
                {
                    NativeMethods.DestroyIcon(hIcon);
                }
            }
        }

        private static Bitmap TrimTransparentPadding(Bitmap source)
        {
            if (source == null)
            {
                return null;
            }

            var minX = source.Width;
            var minY = source.Height;
            var maxX = -1;
            var maxY = -1;

            for (var y = 0; y < source.Height; y++)
            {
                for (var x = 0; x < source.Width; x++)
                {
                    var pixel = source.GetPixel(x, y);
                    if (pixel.A <= 8)
                    {
                        continue;
                    }

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return new Bitmap(source);
            }

            var width = maxX - minX + 1;
            var height = maxY - minY + 1;
            if (width >= source.Width && height >= source.Height)
            {
                return new Bitmap(source);
            }

            var trimmed = new Bitmap(width, height);
            using (var g = Graphics.FromImage(trimmed))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(
                    source,
                    new Rectangle(0, 0, width, height),
                    new Rectangle(minX, minY, width, height),
                    GraphicsUnit.Pixel);
            }

            return trimmed;
        }

        private static IEnumerable<string> EnumerateLogoCandidates()
        {
            var preferredNames = new[]
            {
                "clientlogo.png",
                "client-logo.png",
                "client_logo.png",
                "companylogo.png",
                "company-logo.png",
                "company_logo.png",
                "brand-logo.png",
                "school-logo.png",
                "logo.png",
                "brand.png"
            };

            foreach (var directory in EnumerateAssetDirectories().Distinct(StringComparer.OrdinalIgnoreCase))
            {
                foreach (var preferredName in preferredNames)
                {
                    yield return Path.Combine(directory, preferredName);
                }

                if (!Directory.Exists(directory))
                {
                    continue;
                }

                IEnumerable<string> discovered;
                try
                {
                    discovered = Directory.EnumerateFiles(directory)
                        .Where(IsSupportedImage)
                        .OrderByDescending(GetLogoScore)
                        .ThenByDescending(GetFileSizeOrZero);
                }
                catch
                {
                    continue;
                }

                foreach (var path in discovered)
                {
                    yield return path;
                }
            }
        }

        private static IEnumerable<string> EnumerateIconCandidates()
        {
            var iconNames = new[]
            {
                "app-logo.ico",
                "app.ico",
                "logo.ico"
            };

            foreach (var directory in EnumerateAssetDirectories().Distinct(StringComparer.OrdinalIgnoreCase))
            {
                foreach (var iconName in iconNames)
                {
                    yield return Path.Combine(directory, iconName);
                }
            }
        }

        private static IEnumerable<string> EnumerateAssetDirectories()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                yield break;
            }

            yield return Path.Combine(baseDir, "assets");
            yield return Path.GetFullPath(Path.Combine(baseDir, "..", "assets"));
            yield return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "assets"));
            yield return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "assets"));
            yield return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "assets"));
        }

        private static bool IsSupportedImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var ext = Path.GetExtension(path);
            return string.Equals(ext, ".png", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".jpeg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".bmp", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetLogoScore(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path) ?? string.Empty;
            var lower = name.ToLowerInvariant();
            var score = 0;

            if (string.Equals(lower, "brand-logo", StringComparison.Ordinal))
            {
                score += 200;
            }

            if (lower.Contains("logo"))
            {
                score += 80;
            }

            if (lower.Contains("school"))
            {
                score += 50;
            }

            if (lower.Contains("brand"))
            {
                score += 30;
            }

            if (lower.Contains("removebg"))
            {
                score += 20;
            }

            return score;
        }

        private static long GetFileSizeOrZero(string path)
        {
            try
            {
                return new FileInfo(path).Length;
            }
            catch
            {
                return 0L;
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DestroyIcon(IntPtr handle);
        }
    }
}
