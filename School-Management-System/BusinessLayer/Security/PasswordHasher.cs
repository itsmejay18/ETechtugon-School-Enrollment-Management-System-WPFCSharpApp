using System;
using System.Security.Cryptography;

namespace School_Management_System.BusinessLayer.Security
{
    public sealed class PasswordHashResult
    {
        private byte[] _hash;
        private byte[] _salt;

        public byte[] Hash { get { return _hash; } set { _hash = value; } }
        public byte[] Salt { get { return _salt; } set { _salt = value; } }
    }

    public static class PasswordHasher
    {
        // Reasonable defaults for WinForms + SQL storage; can be tuned later.
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Iterations = 100000;

        /// <summary>
        /// Creates a PBKDF2 hash (Rfc2898DeriveBytes) for the supplied password.
        /// </summary>
        public static PasswordHashResult HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSizeBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = pbkdf2.GetBytes(HashSizeBytes);
            }

            return new PasswordHashResult { Hash = hash, Salt = salt };
        }

        /// <summary>
        /// Verifies the supplied password against the stored salt + hash.
        /// </summary>
        public static bool Verify(string password, byte[] salt, byte[] expectedHash)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (salt == null) throw new ArgumentNullException(nameof(salt));
            if (expectedHash == null) throw new ArgumentNullException(nameof(expectedHash));

            byte[] actualHash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                actualHash = pbkdf2.GetBytes(expectedHash.Length);
            }

            return FixedTimeEquals(actualHash, expectedHash);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}

