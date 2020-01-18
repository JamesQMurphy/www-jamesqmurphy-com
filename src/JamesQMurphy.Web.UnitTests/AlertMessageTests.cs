using JamesQMurphy.Web.Models;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Tests
{
    public class AlertMessageTests
    {
        AlertMessageCollection _alertMessageCollection;
        Dictionary<string, object> _backingCollection;

        [SetUp]
        public void Setup()
        {
            _backingCollection = new Dictionary<string, object>();
            _alertMessageCollection = new AlertMessageCollection(_backingCollection);
        }

        [Test]
        public void DefaultsToZeroAlerts()
        {
            foreach (var messageType in (AlertMessageTypes[])Enum.GetValues(typeof(AlertMessageTypes)))
            {
                Assert.AreEqual(0, _alertMessageCollection.GetCount(messageType));
                Assert.IsEmpty(_alertMessageCollection.GetAlertMessages(messageType));
            }
        }

        [Test]
        public void OtherElementsDontInterfere()
        {
            _backingCollection.Add("some_bizzare_key", new object());
            foreach (var messageType in (AlertMessageTypes[])Enum.GetValues(typeof(AlertMessageTypes)))
            {
                Assert.AreEqual(0, _alertMessageCollection.GetCount(messageType));
                Assert.IsEmpty(_alertMessageCollection.GetAlertMessages(messageType));
            }
        }

        [Test]
        public void GetSingleAlert()
        {
            var message = "Something bad happened";
            var title = "Error";
            _alertMessageCollection.AddDangerAlert(message, title);

            foreach (var messageType in (AlertMessageTypes[])Enum.GetValues(typeof(AlertMessageTypes)))
            {
                if (messageType == AlertMessageTypes.Danger)
                {
                    Assert.AreEqual(1, _alertMessageCollection.GetCount(messageType));
                    var messages = _alertMessageCollection.GetAlertMessages(messageType).ToList();
                    Assert.AreEqual(1, messages.Count);
                    Assert.AreEqual(message, messages[0].Message);
                    Assert.AreEqual(title, messages[0].Title);
                }
                else
                {
                    Assert.AreEqual(0, _alertMessageCollection.GetCount(messageType));
                    Assert.IsEmpty(_alertMessageCollection.GetAlertMessages(messageType));
                }
            }
        }
    }
}
