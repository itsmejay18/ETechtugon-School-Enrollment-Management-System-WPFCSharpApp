using System;
using System.IO;
using System.Text;
using School_Management_System.Common;

namespace School_Management_System.Wpf.Services
{
    public static class PhotoStorageHelper
    {
        private static string GetStorageRoot()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localAppData))
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }

            return Path.Combine(localAppData, "SchoolManagementSystem");
        }

        public static byte[] ReadPhotoBytes(string filePath)
        {
            byte[] bytes;
            string errorMessage;
            return TryReadPhotoBytes(filePath, out bytes, out errorMessage) ? bytes : null;
        }

        public static bool TryReadPhotoBytes(string filePath, out byte[] bytes, out string errorMessage)
        {
            return ImageFileReader.TryReadImageBytes(filePath, out bytes, out errorMessage);
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
