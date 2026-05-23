using System;
using System.Drawing;
using System.IO;

namespace School_Management_System.Common
{
    public static class ImageFileReader
    {
        public const int MaxImageBytes = 2 * 1024 * 1024;

        public static bool TryReadImageBytes(string filePath, out byte[] bytes, out string errorMessage)
        {
            bytes = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                errorMessage = "Select an existing image file.";
                return false;
            }

            var extension = Path.GetExtension(filePath);
            if (!IsAllowedImageExtension(extension))
            {
                errorMessage = "Only JPG, PNG, BMP, or GIF images are allowed.";
                return false;
            }

            FileInfo info;
            try
            {
                info = new FileInfo(filePath);
            }
            catch
            {
                errorMessage = "The selected image file could not be inspected.";
                return false;
            }

            if (info.Length <= 0)
            {
                errorMessage = "The selected image file is empty.";
                return false;
            }

            if (info.Length > MaxImageBytes)
            {
                errorMessage = "The selected image is too large. Use an image up to 2 MB.";
                return false;
            }

            try
            {
                using (var stream = File.OpenRead(filePath))
                using (var image = Image.FromStream(stream, false, true))
                {
                    if (image.Width <= 0 || image.Height <= 0)
                    {
                        errorMessage = "The selected file is not a valid image.";
                        return false;
                    }
                }

                bytes = File.ReadAllBytes(filePath);
                return true;
            }
            catch
            {
                errorMessage = "The selected file is not a valid image.";
                return false;
            }
        }

        private static bool IsAllowedImageExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return false;
            }

            extension = extension.Trim().ToLowerInvariant();
            return extension == ".jpg" ||
                   extension == ".jpeg" ||
                   extension == ".png" ||
                   extension == ".bmp" ||
                   extension == ".gif";
        }
    }
}
