using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Honeycomb.Interfaces;

namespace Honeycomb
{
    
    [DataContract]
    public class ClientCrawlerInfo : INotifyPropertyChanged
    {
        private string _serverIP;
        private string _clientName;
        private Guid _clientIdentifier;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICrawlerClientCallback SavedCallback;


        public ClientCrawlerInfo(string serverIp, string clientName, Guid clientIdentifier)
        {
             ServerIP = serverIp;
             ClientName = clientName;
            _clientIdentifier = clientIdentifier;
        }
        [DataMember]
        public string ServerIP
        {
            get { return _serverIP; }
            set
            {
                _serverIP = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("serverIP");
            }
        }

        [DataMember]
        public string ClientName
        {
            get { return _clientName; }
            set
            {
                _clientName = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClientName");
            }
        }


        [DataMember]
        public Guid ClientIdentifier
        {
            get { return _clientIdentifier; }
            set
            {
                _clientIdentifier = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ClientName");
            }
        }


        protected void OnPropertyChanged(string propValue)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propValue));
        }
    }
}
