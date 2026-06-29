using System;
using System.Collections.Generic;
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
            return LoadClientLogo();
        }

        public static ImageSource LoadClientLogo()
        {
            return LoadImageSource(SchoolBranding.BrandLogoData, true)
                   ?? LoadImageSource(FindClientLogoAssetPath(), true)
                   ?? LoadImageSource(FindRasterBrandAssetPath(), true)
                   ?? LoadImageSource(FindAppIconAssetPath());
        }

        public static ImageSource LoadClientLogoMark()
        {
            return LoadClientLogo();
        }

        public static ImageSource LoadCompanyLogo()
        {
            return LoadHeaderLogo();
        }

        public static ImageSource LoadHeaderLogo()
        {
            return LoadImageSource(SchoolBranding.CompactLogoData, true)
                   ?? LoadImageSource(SchoolBranding.BrandLogoData, true)
                   ?? LoadImageSource(FindCompanyLogoAssetPath(), true)
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

        public static ImageSource LoadLogoPreview(byte[] bytes)
        {
            return LoadImageSource(bytes, true);
        }

        private static ImageSource LoadImageSource(byte[] bytes, bool stripSolidWhiteBackground = false)
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
                    image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    image.StreamSource = stream;
                    image.EndInit();
                    return PrepareImageSource(image, stripSolidWhiteBackground);
                }
            }
            catch
            {
                return null;
            }
        }

        private static ImageSource LoadImageSource(string path, bool stripSolidWhiteBackground = false)
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

                        return PrepareImageSource(decoder.Frames[0], stripSolidWhiteBackground);
                    }
                }

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                return PrepareImageSource(image, stripSolidWhiteBackground);
            }
            catch
            {
                return null;
            }
        }

        private static ImageSource PrepareImageSource(BitmapSource source, bool stripSolidWhiteBackground = false)
        {
            if (source == null)
            {
                return null;
            }

            try
            {
                var processed = stripSolidWhiteBackground
                    ? StripSolidWhiteBackground(source)
                    : source;

                var trimmed = TrimTransparentMargins(processed);
                if (trimmed != null && trimmed.CanFreeze && !trimmed.IsFrozen)
                {
                    trimmed.Freeze();
                }

                return trimmed ?? processed ?? source;
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

        private static BitmapSource StripSolidWhiteBackground(BitmapSource source)
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

            var visited = new bool[width * height];
            var queue = new Queue<int>();

            for (var x = 0; x < width; x++)
            {
                TryEnqueueBackgroundPixel(x, 0, width, height, stride, pixels, visited, queue);
                TryEnqueueBackgroundPixel(x, height - 1, width, height, stride, pixels, visited, queue);
            }

            for (var y = 0; y < height; y++)
            {
                TryEnqueueBackgroundPixel(0, y, width, height, stride, pixels, visited, queue);
                TryEnqueueBackgroundPixel(width - 1, y, width, height, stride, pixels, visited, queue);
            }

            if (queue.Count == 0)
            {
                return source;
            }

            while (queue.Count > 0)
            {
                var index = queue.Dequeue();
                var x = index % width;
                var y = index / width;
                var pixelOffset = (y * stride) + (x * 4);

                pixels[pixelOffset] = 0;
                pixels[pixelOffset + 1] = 0;
                pixels[pixelOffset + 2] = 0;
                pixels[pixelOffset + 3] = 0;

                TryEnqueueBackgroundPixel(x - 1, y, width, height, stride, pixels, visited, queue);
                TryEnqueueBackgroundPixel(x + 1, y, width, height, stride, pixels, visited, queue);
                TryEnqueueBackgroundPixel(x, y - 1, width, height, stride, pixels, visited, queue);
                TryEnqueueBackgroundPixel(x, y + 1, width, height, stride, pixels, visited, queue);
            }

            var cleaned = BitmapSource.Create(
                width,
                height,
                working.DpiX,
                working.DpiY,
                PixelFormats.Bgra32,
                null,
                pixels,
                stride);

            if (cleaned.CanFreeze)
            {
                cleaned.Freeze();
            }

            return cleaned;
        }

        private static void TryEnqueueBackgroundPixel(
            int x,
            int y,
            int width,
            int height,
            int stride,
            byte[] pixels,
            bool[] visited,
            Queue<int> queue)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return;
            }

            var index = (y * width) + x;
            if (visited[index])
            {
                return;
            }

            var pixelOffset = (y * stride) + (x * 4);
            if (!IsNearWhiteBackgroundPixel(
                    pixels[pixelOffset + 2],
                    pixels[pixelOffset + 1],
                    pixels[pixelOffset],
                    pixels[pixelOffset + 3]))
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        private static bool IsNearWhiteBackgroundPixel(byte r, byte g, byte b, byte a)
        {
            if (a <= 10)
            {
                return false;
            }

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var spread = max - min;

            if (r >= 220 && g >= 220 && b >= 220 && spread <= 25)
            {
                return true;
            }

            return max >= 48 && spread <= 24;
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

        private static ImageSource CropLogoTagline(ImageSource source)
        {
            var bitmap = source as BitmapSource;
            if (bitmap == null || bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0)
            {
                return source;
            }

            var aspectRatio = (double)bitmap.PixelWidth / bitmap.PixelHeight;
            if (aspectRatio < 2.2)
            {
                return source;
            }

            var cropHeight = (int)Math.Round(bitmap.PixelHeight * 0.84);
            if (cropHeight <= 0 || cropHeight >= bitmap.PixelHeight)
            {
                return source;
            }

            var cropped = new CroppedBitmap(
                bitmap,
                new Int32Rect(0, 0, bitmap.PixelWidth, cropHeight));

            return PrepareImageSource(cropped);
        }

        private static string FindRasterBrandAssetPath()
        {
            return FindExistingAssetPath(new[]
            {
                "finallogo.png",
                "newbranding +.png",
                "newbranding.png",
                "brand-logo.png",
                "school-logo.png",
                "logo.png",
                "brand.png"
            });
        }

        private static string FindClientLogoAssetPath()
        {
            return FindExistingAssetPath(new[]
            {
                "finallogo.png",
                "clientlogo.png",
                "client-logo.png",
                "client_logo.png"
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
