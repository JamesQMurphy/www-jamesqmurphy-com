using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class AlertMessageCollection
    {
        private readonly IDictionary<string, object> _backingDictionary;

        public AlertMessageCollection(IDictionary<string, object> backingDictionary)
        {
            _backingDictionary = backingDictionary;
        }

        public void AddAlertMessage(AlertMessage alertMessage)
        {
            var index = GetCount(alertMessage.AlertMessageType);
            _backingDictionary[_getMessageKey(alertMessage.AlertMessageType, index)] = alertMessage.Message;
            _backingDictionary[_getTitleKey(alertMessage.AlertMessageType, index)] = alertMessage.Title;
            _backingDictionary[_getCountKey(alertMessage.AlertMessageType)] = index + 1;
        }
        public void AddAlertMessage(AlertMessageTypes alertMessageType, string message, string title = null) => AddAlertMessage(new AlertMessage { AlertMessageType = alertMessageType, Message = message, Title = title });

        public void AddPrimaryAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Primary, message, title);
        public void AddSecondaryAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Secondary, message, title);
        public void AddSuccessAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Success, message, title);
        public void AddDangerAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Danger, message, title);
        public void AddWarningAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Warning, message, title);
        public void AddInfoAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Info, message, title);
        public void AddLightAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Light, message, title);
        public void AddDarkAlert(string message, string title = null) => AddAlertMessage(AlertMessageTypes.Dark, message, title);

        public int GetCount(AlertMessageTypes alertMessageType)
        {
            var key = _getCountKey(alertMessageType);
            if (_backingDictionary.ContainsKey(key))
            {
                return (int)_backingDictionary[key];
            }
            else
            {
                return 0;
            }
        }
        public IEnumerable<AlertMessage> GetAlertMessages(AlertMessageTypes alertMessageType)
        {
            int count = GetCount(alertMessageType);
            for(int i = 0; i < count; i++)
            {
                yield return new AlertMessage
                {
                    AlertMessageType = alertMessageType,
                    Message = _backingDictionary[_getMessageKey(alertMessageType, i)].ToString(),
                    Title = _backingDictionary[_getTitleKey(alertMessageType, i)].ToString()
                };
            }
        }

        private string _getCountKey(AlertMessageTypes alertMessageType) => $"alertMessages-{alertMessageType}-count";
        private string _getMessageKey(AlertMessageTypes alertMessageType, int index) => $"alertMessages-{alertMessageType}-{index}-message";
        private string _getTitleKey(AlertMessageTypes alertMessageType, int index) => $"alertMessages-{alertMessageType}-{index}-title";
    }
}
