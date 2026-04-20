using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using School_Management_System.Models;

namespace School_Management_System.Wpf.Services
{
    public static class WpfUiDataHelper
    {
        public static ImageSource LoadImage(byte[] bytes)
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
                    image.Freeze();
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }

        public static ImageSource LoadImage(byte[] bytes, string path)
        {
            var image = LoadImage(bytes);
            return image ?? LoadImageFromPath(path);
        }

        public static ImageSource LoadImageFromPath(string path)
        {
            var fullPath = ResolvePhotoPath(path);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                return null;
            }

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(fullPath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
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
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrWhiteSpace(localAppData))
            {
                var appDataPath = Path.Combine(localAppData, "SchoolManagementSystem", relativePath);
                if (File.Exists(appDataPath))
                {
                    return appDataPath;
                }
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, relativePath);
        }

        public static DataTable CreateSettingsTable(IDictionary<string, string> settings, string searchText, Func<KeyValuePair<string, string>, bool> extraFilter = null)
        {
            var table = new DataTable();
            table.Columns.Add("SettingKey", typeof(string));
            table.Columns.Add("SettingValue", typeof(string));

            var search = (searchText ?? string.Empty).Trim();
            var matchesSearch = string.IsNullOrWhiteSpace(search);

            foreach (var pair in settings ?? new Dictionary<string, string>())
            {
                if (extraFilter != null && !extraFilter(pair))
                {
                    continue;
                }

                var safeKey = pair.Key ?? string.Empty;
                var safeValue = pair.Value ?? string.Empty;
                if (!matchesSearch &&
                    safeKey.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0 &&
                    safeValue.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                table.Rows.Add(safeKey, MaskSensitiveValue(safeKey, safeValue));
            }

            return table;
        }

        public static DataTable CreateActivityLogTable(IEnumerable<ActivityLog> activityLogs)
        {
            var table = new DataTable();
            table.Columns.Add("CreatedAt", typeof(string));
            table.Columns.Add("Username", typeof(string));
            table.Columns.Add("DisplayName", typeof(string));
            table.Columns.Add("Action", typeof(string));
            table.Columns.Add("Entity", typeof(string));
            table.Columns.Add("EntityId", typeof(string));
            table.Columns.Add("MachineName", typeof(string));
            table.Columns.Add("Details", typeof(string));

            foreach (var log in activityLogs ?? Enumerable.Empty<ActivityLog>())
            {
                table.Rows.Add(
                    log.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture),
                    log.Username ?? string.Empty,
                    log.DisplayName ?? string.Empty,
                    log.Action ?? string.Empty,
                    log.Entity ?? string.Empty,
                    log.EntityId.HasValue ? log.EntityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    log.MachineName ?? string.Empty,
                    log.Details ?? string.Empty);
            }

            return table;
        }

        public static string FormatValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (value is DateTime)
            {
                return ((DateTime)value).ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
            }

            if (value is TimeSpan)
            {
                return ((TimeSpan)value).ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            }

            if (value is bool)
            {
                return (bool)value ? "Yes" : "No";
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public static string ToFriendlyLabel(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var withSpaces = Regex.Replace(raw, "([a-z0-9])([A-Z])", "$1 $2");
            withSpaces = Regex.Replace(withSpaces, "([A-Za-z])([0-9])", "$1 $2");
            return withSpaces.Replace("Id", "ID").Trim();
        }

        private static string MaskSensitiveValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return value ?? string.Empty;
            }

            return key.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0
                ? "********"
                : (value ?? string.Empty);
        }
    }
}
