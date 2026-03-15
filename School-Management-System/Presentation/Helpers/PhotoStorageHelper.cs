using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace School_Management_System.Presentation.Helpers
{
    public static class PhotoStorageHelper
    {
        private static string GetStorageRoot()
        {
            // Use a per-user writable folder so installed builds can persist photos.
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localAppData))
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }

            return Path.Combine(localAppData, "SchoolManagementSystem");
        }

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

            var relativePath = path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var appDataPath = Path.Combine(GetStorageRoot(), relativePath);
            if (File.Exists(appDataPath))
            {
                return appDataPath;
            }

            // Backward compatibility for records created before LocalAppData storage.
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
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

        public static void ShowPhotoFromBytes(PictureBox pictureBox, byte[] data)
        {
            if (pictureBox == null) return;

            var oldImage = pictureBox.Image;
            pictureBox.Image = null;
            oldImage?.Dispose();

            if (data == null || data.Length == 0) return;

            try
            {
                using (var ms = new MemoryStream(data))
                {
                    pictureBox.Image = new Bitmap(Image.FromStream(ms));
                }
            }
            catch
            {
                pictureBox.Image = null;
            }
        }

        public static byte[] ReadPhotoBytes(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return null;
            try { return File.ReadAllBytes(filePath); }
            catch { return null; }
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
                var photosDir = Path.Combine(GetStorageRoot(), "Photos", safeFolder);
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
