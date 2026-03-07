using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed partial class DatabaseBackupService
    {
        private const string BackupEnvelopeFormatVersion = "SMSBAK2";
        private const string BackupCipherAlgorithm = "AES-256-CBC";
        private const string BackupSignatureAlgorithm = "HMAC-SHA256";
        private static readonly byte[] BackupSecretEntropy = Encoding.UTF8.GetBytes("SchoolManagementSystem::BackupSecret::v1");

        private static void SaveBackupPackage(string path, DatabaseBackupPackage package)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Backup file path is required.", nameof(path));
            }

            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var payloadXml = SerializeXmlToString(package);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadXml);
            var envelope = BuildEncryptedEnvelope(payloadBytes);
            SaveXml(path, envelope);
        }

        private static DatabaseBackupPackage ReadBackupPackage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            DatabaseBackupEnvelope envelope;
            if (TryLoadXml(path, out envelope) && envelope != null)
            {
                if (!string.Equals(envelope.FormatVersion, BackupEnvelopeFormatVersion, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Unsupported backup file format.");
                }

                var payloadBytes = DecryptEnvelopePayload(envelope);
                var payloadXml = Encoding.UTF8.GetString(payloadBytes);
                return DeserializeXmlFromString<DatabaseBackupPackage>(payloadXml);
            }

            DatabaseBackupPackage legacy;
            if (TryLoadXml(path, out legacy))
            {
                return legacy;
            }

            throw new InvalidOperationException("Invalid or unreadable backup file.");
        }

        private static bool TryLoadXml<T>(string path, out T payload) where T : class
        {
            payload = null;
            try
            {
                payload = LoadXml<T>(path);
                return payload != null;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        private static string SerializeXmlToString<T>(T payload)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                serializer.Serialize(sw, payload);
                return sw.ToString();
            }
        }

        private static T DeserializeXmlFromString<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(T));
            using (var sr = new StringReader(xml))
            {
                return serializer.Deserialize(sr) as T;
            }
        }

        private static DatabaseBackupEnvelope BuildEncryptedEnvelope(byte[] payloadBytes)
        {
            if (payloadBytes == null)
            {
                throw new ArgumentNullException(nameof(payloadBytes));
            }

            var secret = GetBackupSecretMaterial();
            var encryptionKey = DeriveKey(secret, "encrypt");
            var signingKey = DeriveKey(secret, "sign");

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] cipherBytes;
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encryptionKey;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor())
                {
                    cipherBytes = encryptor.TransformFinalBlock(payloadBytes, 0, payloadBytes.Length);
                }
            }

            var envelope = new DatabaseBackupEnvelope
            {
                FormatVersion = BackupEnvelopeFormatVersion,
                CreatedAtUtc = DateTime.UtcNow,
                CipherAlgorithm = BackupCipherAlgorithm,
                SignatureAlgorithm = BackupSignatureAlgorithm,
                KeyFingerprint = ComputeKeyFingerprint(signingKey),
                InitializationVector = Convert.ToBase64String(iv),
                CipherText = Convert.ToBase64String(cipherBytes)
            };

            envelope.Signature = ComputeSignature(envelope, signingKey);
            return envelope;
        }

        private static byte[] DecryptEnvelopePayload(DatabaseBackupEnvelope envelope)
        {
            if (envelope == null)
            {
                throw new InvalidOperationException("Backup envelope is missing.");
            }

            if (!string.Equals(envelope.FormatVersion, BackupEnvelopeFormatVersion, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Unsupported backup file format.");
            }

            if (string.IsNullOrWhiteSpace(envelope.InitializationVector) ||
                string.IsNullOrWhiteSpace(envelope.CipherText) ||
                string.IsNullOrWhiteSpace(envelope.Signature))
            {
                throw new InvalidOperationException("Backup envelope is incomplete.");
            }

            var secret = GetBackupSecretMaterial();
            var encryptionKey = DeriveKey(secret, "encrypt");
            var signingKey = DeriveKey(secret, "sign");

            var expectedFingerprint = ComputeKeyFingerprint(signingKey);
            if (!string.Equals(envelope.KeyFingerprint ?? string.Empty, expectedFingerprint, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Backup was created with a different encryption key.");
            }

            var expectedSignature = ComputeSignature(envelope, signingKey);
            if (!FixedTimeEqualsBase64(envelope.Signature, expectedSignature))
            {
                throw new InvalidOperationException("Backup integrity check failed.");
            }

            byte[] iv;
            byte[] cipherBytes;
            try
            {
                iv = Convert.FromBase64String(envelope.InitializationVector);
                cipherBytes = Convert.FromBase64String(envelope.CipherText);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Backup payload is invalid.", ex);
            }

            if (iv.Length != 16)
            {
                throw new InvalidOperationException("Backup payload is invalid.");
            }

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encryptionKey;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    try
                    {
                        return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    }
                    catch (CryptographicException ex)
                    {
                        throw new InvalidOperationException("Unable to decrypt backup payload.", ex);
                    }
                }
            }
        }

        private static string ComputeSignature(DatabaseBackupEnvelope envelope, byte[] signingKey)
        {
            var signedData = string.Join(
                "|",
                envelope.FormatVersion ?? string.Empty,
                envelope.CreatedAtUtc.ToString("o", CultureInfo.InvariantCulture),
                envelope.CipherAlgorithm ?? string.Empty,
                envelope.SignatureAlgorithm ?? string.Empty,
                envelope.KeyFingerprint ?? string.Empty,
                envelope.InitializationVector ?? string.Empty,
                envelope.CipherText ?? string.Empty);

            var signedBytes = Encoding.UTF8.GetBytes(signedData);
            using (var hmac = new HMACSHA256(signingKey))
            {
                return Convert.ToBase64String(hmac.ComputeHash(signedBytes));
            }
        }

        private static string ComputeKeyFingerprint(byte[] signingKey)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(signingKey);
                return Convert.ToBase64String(hash);
            }
        }

        private static byte[] DeriveKey(byte[] secret, string purpose)
        {
            using (var hmac = new HMACSHA256(secret))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes("sms-backup|" + purpose + "|v1"));
            }
        }

        private static byte[] GetBackupSecretMaterial()
        {
            var overrideValue = Environment.GetEnvironmentVariable("SMS_BACKUP_KEY");
            if (string.IsNullOrWhiteSpace(overrideValue))
            {
                overrideValue = ConfigurationManager.AppSettings["BackupEncryptionKey"];
            }

            if (!string.IsNullOrWhiteSpace(overrideValue))
            {
                return SHA256Bytes(Encoding.UTF8.GetBytes(overrideValue.Trim()));
            }

            return GetOrCreateLocalBackupSecret();
        }

        private static byte[] GetOrCreateLocalBackupSecret()
        {
            var directory = GetBackupStateDirectory();
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "backup.secret");

            if (File.Exists(path))
            {
                var protectedBytes = File.ReadAllBytes(path);
                try
                {
                    return ProtectedData.Unprotect(protectedBytes, BackupSecretEntropy, DataProtectionScope.CurrentUser);
                }
                catch
                {
                    // Regenerate if key file is invalid/corrupted.
                }
            }

            byte[] secret = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secret);
            }

            var protectedSecret = ProtectedData.Protect(secret, BackupSecretEntropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, protectedSecret);
            return secret;
        }

        private static byte[] SHA256Bytes(byte[] input)
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(input ?? new byte[0]);
            }
        }

        private static bool FixedTimeEqualsBase64(string leftBase64, string rightBase64)
        {
            if (string.IsNullOrWhiteSpace(leftBase64) || string.IsNullOrWhiteSpace(rightBase64))
            {
                return false;
            }

            byte[] left;
            byte[] right;
            try
            {
                left = Convert.FromBase64String(leftBase64);
                right = Convert.FromBase64String(rightBase64);
            }
            catch (FormatException)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
