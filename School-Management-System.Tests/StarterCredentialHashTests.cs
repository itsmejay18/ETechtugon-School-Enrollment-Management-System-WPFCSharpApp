using System;
using NUnit.Framework;
using School_Management_System.BusinessLayer.Security;

namespace School_Management_System.Tests.Security
{
    [TestFixture]
    public sealed class StarterCredentialHashTests
    {
        [TestCase("admin123", "2C8FA907A42255A3FB0F3C02B49C7473", "491B306728DB9AB65DC7504B3EF8DA1293693F2E55A958405C4BB7BB6672A934")]
        [TestCase("registrar123", "474715AC6A02F482A85FA09479BE5C30", "828F4E3EE279C5D766D8C58C257E87C0CB44F4843EDC051D1A41219B72738523")]
        [TestCase("faculty123", "42E2DBAFA5230A27DA3A31C49A8203F0", "DD9B92F18FDBD92BE8126B29ECF7362CA8DD9ACAFEFDCA7803CC62EB46B945AF")]
        public void DocumentedStarterPasswords_MatchSeededHashes(string password, string saltHex, string hashHex)
        {
            var salt = HexToBytes(saltHex);
            var hash = HexToBytes(hashHex);

            Assert.That(PasswordHasher.Verify(password, salt, hash), Is.True);
        }

        private static byte[] HexToBytes(string hex)
        {
            if (hex == null) throw new ArgumentNullException(nameof(hex));
            if (hex.Length % 2 != 0) throw new ArgumentException("Hex string length must be even.", nameof(hex));

            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}
