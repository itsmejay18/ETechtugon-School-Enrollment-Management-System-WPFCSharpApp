using NUnit.Framework;
using School_Management_System.Common;

namespace School_Management_System.Tests.Common
{
    [TestFixture]
    public sealed class SensitiveDataProtectorTests
    {
        [Test]
        public void Protect_Unprotect_RoundTripsOriginalValue()
        {
            var plainText = "TestPassword123!";

            var protectedValue = SensitiveDataProtector.Protect(plainText);
            var unprotected = SensitiveDataProtector.Unprotect(protectedValue);

            Assert.That(protectedValue, Is.Not.EqualTo(plainText));
            Assert.That(SensitiveDataProtector.IsProtected(protectedValue), Is.True);
            Assert.That(unprotected, Is.EqualTo(plainText));
        }

        [Test]
        public void Unprotect_PlainTextValue_ReturnsInput()
        {
            const string plainText = "already-plain";
            var result = SensitiveDataProtector.Unprotect(plainText);
            Assert.That(result, Is.EqualTo(plainText));
        }
    }
}

