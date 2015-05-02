using System.ComponentModel;
using System.Runtime.Serialization;

namespace Honeycomb
{
    #region ClientCrawlerInfo class
    /// <summary>
    /// This class represnts a single chat user that can participate in this chat application
    /// This class implements INotifyPropertyChanged to support one-way and two-way
    /// WPF bindings (such that the UI element updates when the source has been changed
    /// dynamically)
    /// [DataContract] specifies that the type defines or implements a data contract
    /// and is serializable by a serializer, such as the DataContractSerializer
    /// </summary>
    [DataContract]
    public class ClientCrawlerInfo : INotifyPropertyChanged
    {
        #region Instance Fields
        private string _serverIP;
        private string _clientName;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region Ctors
        ///// <summary>
        ///// Blank constructor
        ///// </summary>
        //public ClientCrawlerInfo()
        //{
        //}

        /// <summary>
        /// Assign constructor
        /// </summary>
        /// <param name="serverIp">Image url to allow a picture to be created for this chatter</param>
        /// <param name="clientName">The ClientName to use for this chatter</param>
        public ClientCrawlerInfo(string serverIp, string clientName)
        {
             ServerIP = serverIp;
             ClientName = clientName;
        }
        #endregion
        #region Public Properties
        /// <summary>
        /// The chatters image url
        /// </summary>
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

        /// <summary>
        /// The chatters ClientName
        /// </summary>
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
        #endregion
        #region OnPropertyChanged (for correct well behaved databinding)
        /// <summary>
        /// Notifies the parent bindings (if any) that a property
        /// value changed and that the binding needs updating
        /// </summary>
        /// <param name="propValue">The property which changed</param>
        protected void OnPropertyChanged(string propValue)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propValue));
            }
        }
        #endregion
    }
    #endregion
}
