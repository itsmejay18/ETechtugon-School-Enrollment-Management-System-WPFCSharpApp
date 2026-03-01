using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace School_Management_System.Presentation.Helpers
{
    public static class PhotoStorageHelper
    {
        public static string ResolvePhotoPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        public static void ShowPhoto(PictureBox pictureBox, string path)
        {
            if (pictureBox == null) return;

            var oldImage = pictureBox.Image;
            pictureBox.Image = null;
            if (oldImage != null)
            {
                oldImage.Dispose();
            }

            var fullPath = ResolvePhotoPath(path);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                return;
            }

            try
            {
                using (var img = Image.FromFile(fullPath))
                {
                    pictureBox.Image = new Bitmap(img);
                }
            }
            catch
            {
                pictureBox.Image = null;
            }
        }

        public static string PersistPhoto(string sourcePath, string folderName, string entityKey, string fallbackPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                return fallbackPath;
            }

            try
            {
                var safeFolder = string.IsNullOrWhiteSpace(folderName) ? "General" : SanitizePathToken(folderName);
                var photosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos", safeFolder);
                if (!Directory.Exists(photosDir))
                {
                    Directory.CreateDirectory(photosDir);
                }

                var ext = Path.GetExtension(sourcePath);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    ext = ".jpg";
                }

                var safeName = SanitizePathToken(string.IsNullOrWhiteSpace(entityKey) ? Guid.NewGuid().ToString("N") : entityKey);
                var fileName = safeName + ext.ToLowerInvariant();
                var destFile = Path.Combine(photosDir, fileName);
                File.Copy(sourcePath, destFile, true);

                return Path.Combine("Photos", safeFolder, fileName);
            }
            catch
            {
                return fallbackPath;
            }
        }

        private static string SanitizePathToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "photo";
            }

            var builder = new StringBuilder(value.Length);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var ch in value.Trim())
            {
                var isInvalid = false;
                for (var i = 0; i < invalid.Length; i++)
                {
                    if (ch == invalid[i])
                    {
                        isInvalid = true;
                        break;
                    }
                }

                builder.Append(isInvalid ? '_' : ch);
            }

            var safe = builder.ToString().Replace(" ", "_");
            return string.IsNullOrWhiteSpace(safe) ? "photo" : safe;
        }
    }
}
