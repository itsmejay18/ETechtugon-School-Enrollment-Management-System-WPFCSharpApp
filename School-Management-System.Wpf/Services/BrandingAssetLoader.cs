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
            var path = FindRasterBrandAssetPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = FindAppIconAssetPath();
            }

            return LoadImageSource(path);
        }

        public static ImageSource LoadAppIcon()
        {
            var path = FindAppIconAssetPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = FindRasterBrandAssetPath();
            }

            return LoadImageSource(path);
        }

        private static ImageSource LoadImageSource(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                if (string.Equals(Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var decoder = new IconBitmapDecoder(
                            stream,
                            BitmapCreateOptions.PreservePixelFormat,
                            BitmapCacheOption.OnLoad);

                        if (decoder.Frames.Count == 0)
                        {
                            return null;
                        }

                        var frame = decoder.Frames[0];
                        frame.Freeze();
                        return frame;
                    }
                }

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

        private static string FindRasterBrandAssetPath()
        {
            return FindExistingAssetPath(new[]
            {
                "brand-logo.png",
                "school-logo.png",
                "logo.png",
                "brand.png"
            });
        }

        private static string FindAppIconAssetPath()
        {
            return FindExistingAssetPath(new[]
            {
                "app-logo.ico",
                "app.ico",
                "logo.ico"
            });
        }

        private static string FindExistingAssetPath(string[] assetNames)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                return null;
            }

            var assetDirectories = new[]
            {
                Path.Combine(baseDir, "assets"),
                Path.Combine(baseDir, "..", "assets"),
                Path.Combine(baseDir, "..", "..", "assets"),
                Path.Combine(baseDir, "..", "..", "..", "assets"),
                Path.Combine(baseDir, "..", "..", "..", "..", "assets")
            };

            for (var i = 0; i < assetDirectories.Length; i++)
            {
                for (var j = 0; j < assetNames.Length; j++)
                {
                    try
                    {
                        var fullPath = Path.GetFullPath(Path.Combine(assetDirectories[i], assetNames[j]));
                        if (File.Exists(fullPath))
                        {
                            return fullPath;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
    }
}
