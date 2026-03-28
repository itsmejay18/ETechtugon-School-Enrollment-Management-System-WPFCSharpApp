using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace School_Management_System.Wpf.Services
{
    public static class BrandingAssetLoader
    {
        public static ImageSource LoadBrandLogo()
        {
            var path = FindBrandLogoPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        private static string FindBrandLogoPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                return null;
            }

            var candidates = new[]
            {
                Path.Combine(baseDir, "assets", "brand-logo.png"),
                Path.Combine(baseDir, "..", "assets", "brand-logo.png"),
                Path.Combine(baseDir, "..", "..", "assets", "brand-logo.png"),
                Path.Combine(baseDir, "..", "..", "..", "assets", "brand-logo.png"),
                Path.Combine(baseDir, "..", "..", "..", "..", "assets", "brand-logo.png")
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                try
                {
                    var fullPath = Path.GetFullPath(candidates[i]);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
