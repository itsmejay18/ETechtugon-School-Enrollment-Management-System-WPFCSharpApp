using System;
using NUnit.Framework;
using School_Management_System.DataLayer.Configuration;

namespace School_Management_System.Tests.Configuration
{
    [TestFixture]
    public sealed class ConnectionModeHelperTests
    {
        [TestCase(null, "Online", "Online")]
        [TestCase("", "Local", "Local")]
        [TestCase("network", "Local", "Wired")]
        [TestCase("wifi", "Local", "Wireless")]
        [TestCase("hostinger", "Local", "Online")]
        [TestCase("unknown-mode", "Online", "Online")]
        public void Normalize_ReturnsExpectedMode(string input, string fallbackMode, string expected)
        {
            var actual = ConnectionModeHelper.Normalize(input, fallbackMode);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetDisplayName_ReturnsFriendlyNetworkLabelForWiredMode()
        {
            var label = ConnectionModeHelper.GetDisplayName("Wired");
            Assert.That(label, Is.EqualTo("Network"));
        }

        [Test]
        public void ApplyRuntimeMode_SetsNormalizedProcessEnvironmentVariable()
        {
            var previous = Environment.GetEnvironmentVariable("SMS_DB_MODE", EnvironmentVariableTarget.Process);

            try
            {
                ConnectionModeHelper.ApplyRuntimeMode("network");
                var current = Environment.GetEnvironmentVariable("SMS_DB_MODE", EnvironmentVariableTarget.Process);
                Assert.That(current, Is.EqualTo("Wired"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("SMS_DB_MODE", previous, EnvironmentVariableTarget.Process);
            }
        }
    }
}
