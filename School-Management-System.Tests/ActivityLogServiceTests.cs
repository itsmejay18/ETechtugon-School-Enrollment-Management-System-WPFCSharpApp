using System;
using System.Collections.Generic;
using NUnit.Framework;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.Tests.Services
{
    [TestFixture]
    public sealed class ActivityLogServiceTests
    {
        [TearDown]
        public void TearDown()
        {
            UserSession.End();
        }

        [Test]
        public void LogTransaction_UsesCurrentUserAndRaisesActivityLogged()
        {
            var fakeData = new FakeActivityLogData();
            var service = new ActivityLogService(fakeData);
            ActivityLog raisedEntry = null;

            UserSession.Start(new User
            {
                UserId = 42,
                Username = "admin",
                DisplayName = "System Administrator"
            });

            service.ActivityLogged += (sender, args) => raisedEntry = args.Entry;

            service.LogTransaction(
                AppConstants.ActivityActions.Create,
                AppConstants.Entities.Student,
                123,
                "Created student record.");

            Assert.That(fakeData.Entries, Has.Count.EqualTo(1));
            Assert.That(fakeData.Entries[0].UserId, Is.EqualTo(42));
            Assert.That(fakeData.Entries[0].Action, Is.EqualTo(AppConstants.ActivityActions.Create));
            Assert.That(fakeData.Entries[0].Entity, Is.EqualTo(AppConstants.Entities.Student));
            Assert.That(fakeData.Entries[0].EntityId, Is.EqualTo(123));
            Assert.That(fakeData.Entries[0].Details, Is.EqualTo("Created student record."));
            Assert.That(fakeData.Entries[0].MachineName, Is.EqualTo(Environment.MachineName));
            Assert.That(raisedEntry, Is.SameAs(fakeData.Entries[0]));
        }

        private sealed class FakeActivityLogData : IActivityLogData
        {
            public FakeActivityLogData()
            {
                Entries = new List<ActivityLog>();
            }

            public List<ActivityLog> Entries { get; private set; }

            public void Add(ActivityLog entry)
            {
                Entries.Add(entry);
            }

            public IList<ActivityLog> Search(DateTime? fromUtcInclusive, DateTime? toUtcExclusive, string usernameLike, string action, int maxRows)
            {
                return Entries;
            }
        }
    }
}
