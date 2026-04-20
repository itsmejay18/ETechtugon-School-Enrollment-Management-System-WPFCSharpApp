using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace School_Management_System.Wpf.Services
{
    public static class BrandingAssetLoader
    {
        public static ImageSource LoadBrandLogo()
        {
            return LoadImageSource(SchoolBranding.BrandLogoData)
                   ?? LoadImageSource(FindRasterBrandAssetPath())
                   ?? LoadImageSource(FindAppIconAssetPath());
        }

        public static ImageSource LoadCompanyLogo()
        {
            return LoadImageSource(SchoolBranding.CompactLogoData)
                   ?? LoadImageSource(SchoolBranding.BrandLogoData)
                   ?? LoadImageSource(FindCompanyLogoAssetPath())
                   ?? LoadBrandLogo();
        }

        public static ImageSource LoadAppIcon()
        {
            return LoadImageSource(SchoolBranding.BrandLogoData)
                   ?? LoadImageSource(FindRasterBrandAssetPath())
                   ?? LoadImageSource(SchoolBranding.CompactLogoData)
                   ?? LoadImageSource(FindAppIconAssetPath())
                   ?? LoadBrandLogo();
        }

        private static ImageSource LoadImageSource(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    return PrepareImageSource(image);
                }
            }
            catch
            {
                return null;
            }
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

                        return PrepareImageSource(decoder.Frames[0]);
                    }
                }

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                return PrepareImageSource(image);
            }
            catch
            {
                return null;
            }
        }

        private static ImageSource PrepareImageSource(BitmapSource source)
        {
            if (source == null)
            {
                return null;
            }

            try
            {
                var trimmed = TrimTransparentMargins(source);
                if (trimmed != null && trimmed.CanFreeze && !trimmed.IsFrozen)
                {
                    trimmed.Freeze();
                }

                return trimmed ?? source;
            }
            catch
            {
                if (source.CanFreeze && !source.IsFrozen)
                {
                    source.Freeze();
                }

                return source;
            }
        }

        private static BitmapSource TrimTransparentMargins(BitmapSource source)
        {
            if (source == null || source.PixelWidth <= 0 || source.PixelHeight <= 0)
            {
                return source;
            }

            BitmapSource working = source;
            if (working.Format != PixelFormats.Bgra32)
            {
                var converted = new FormatConvertedBitmap();
                converted.BeginInit();
                converted.Source = working;
                converted.DestinationFormat = PixelFormats.Bgra32;
                converted.EndInit();
                if (converted.CanFreeze)
                {
                    converted.Freeze();
                }

                working = converted;
            }

            var width = working.PixelWidth;
            var height = working.PixelHeight;
            var stride = width * 4;
            var pixels = new byte[stride * height];
            working.CopyPixels(pixels, stride, 0);

            var minX = width;
            var minY = height;
            var maxX = -1;
            var maxY = -1;

            for (var y = 0; y < height; y++)
            {
                var rowOffset = y * stride;
                for (var x = 0; x < width; x++)
                {
                    var alpha = pixels[rowOffset + (x * 4) + 3];
                    if (alpha <= 10)
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
                return source;
            }

            if (minX == 0 && minY == 0 && maxX == width - 1 && maxY == height - 1)
            {
                return source;
            }

            var cropped = new CroppedBitmap(
                working,
                new Int32Rect(
                    minX,
                    minY,
                    (maxX - minX) + 1,
                    (maxY - minY) + 1));

            if (cropped.CanFreeze)
            {
                cropped.Freeze();
            }

            return cropped;
        }

        private static string FindRasterBrandAssetPath()
        {
            return FindExistingAssetPath(new[]
            {
                "newbranding +.png",
                "newbranding.png",
                "brand-logo.png",
                "school-logo.png",
                "logo.png",
                "brand.png"
            });
        }

        private static string FindCompanyLogoAssetPath()
        {
            return FindExistingAssetPath(new[]
            {
                "companylogo.png",
                "company-logo.png",
                "company_logo.png"
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
