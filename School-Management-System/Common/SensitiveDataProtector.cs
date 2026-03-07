using System;
using System.Security.Cryptography;
using System.Text;

namespace School_Management_System.Common
{
    public static class SensitiveDataProtector
    {
        private const string Prefix = "encv1:";
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SchoolManagementSystem::ProtectedSettingV1");

        public static bool IsProtected(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
        }

        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            if (IsProtected(plainText))
            {
                return plainText;
            }

            var clearBytes = Encoding.UTF8.GetBytes(plainText);
            var protectedBytes = ProtectedData.Protect(clearBytes, Entropy, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(protectedBytes);
        }

        public static string Unprotect(string protectedValue)
        {
            if (string.IsNullOrEmpty(protectedValue))
            {
                return string.Empty;
            }

            if (!IsProtected(protectedValue))
            {
                return protectedValue;
            }

            try
            {
                var payload = protectedValue.Substring(Prefix.Length);
                var protectedBytes = Convert.FromBase64String(payload);
                var clearBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(clearBytes);
            }
            catch
            {
                // Preserve backward compatibility for malformed/legacy values.
                return protectedValue;
            }
        }
    }
}
