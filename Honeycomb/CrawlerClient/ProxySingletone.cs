using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using CrawlerClient.CrawlerServer;
using Honeycomb.Shared;
using Honeycomb;

namespace CrawlerClient
{

    #region Public enums/event args

    public enum CallBackType
    {
        Receive,
        ReceiveWhisper,
        UserEnter,
        UserLeave
    };

    public class ProxyEventArgs : EventArgs
    {
        public ClientCrawlerInfo[] list;
    }

    /// <summary>
    ///     Proxy callback event args
    /// </summary>
    public class ProxyCallBackEventArgs : EventArgs
    {
        /// <summary>
        ///     Callback type <see cref="CallBackType">CallBackType</see>
        /// </summary>
        public CallBackType callbackType;

        /// <summary>
        ///     The incoming message
        /// </summary>
        public string message = "";

        public ClientCrawlerInfo person = null;
    }

    #endregion

    #region ConnectionSingleton class

    public sealed class ConnectionSingleton : IRemoteCrawlerCallback
    {
        #region Instance Fields

        public delegate void ProxyCallBackEventHandler(object sender, ProxyCallBackEventArgs e);

        public delegate void ProxyEventHandler(object sender, ProxyEventArgs e);

        private static readonly Lazy<ConnectionSingleton> LazySingleton =
            new Lazy<ConnectionSingleton>(() => new ConnectionSingleton());

        private RemoteCrawlerClient proxy;

        //main proxy event

        public event ProxyEventHandler ProxyEvent;
        //callback proxy event

        public event ProxyCallBackEventHandler ProxyCallBackEvent;

        #endregion

        private ConnectionSingleton()
        {
        }

        #region Public Methods

        #region IChatCallback implementation

        public void Receive(ClientCrawlerInfo sender, string message)
        {
            Receive(sender, message, CallBackType.Receive);
        }

        public void StartCrawling( string urlToCrawl)
        {

            var crawlerInstance = new CrawlerEngine();
            var result = crawlerInstance.StartCrawlingProcess(urlToCrawl);

          proxy.ReturnCrawlingResults(result);


            //Here we crawl. Crawl and crawl.


           //And then we want to return results
         //   Receive(sender, message, CallBackType.ReceiveWhisper);
        }
        
        public void UserEnter(ClientCrawlerInfo person)
        {
            UserEnterLeave(person, CallBackType.UserEnter);
        }

        public void UserLeave(ClientCrawlerInfo person)
        {
            UserEnterLeave(person, CallBackType.UserLeave);
        }

        private void Receive(ClientCrawlerInfo sender, string message, CallBackType callbackType)
        {
            var e = new ProxyCallBackEventArgs();
            e.message = message;
            e.callbackType = callbackType;
            e.person = sender;
            OnProxyCallBackEvent(e);
        }

        private void UserEnterLeave(ClientCrawlerInfo person, CallBackType callbackType)
        {
            var e = new ProxyCallBackEventArgs();
            e.person = person;
            e.callbackType = callbackType;
            OnProxyCallBackEvent(e);
        }

        #endregion

        public static ConnectionSingleton Instance
        {
            get { return LazySingleton.Value; }
        }

    
        public void Connect(ClientCrawlerInfo p)
        {
            var site = new InstanceContext(this);

            //var anotherProxy = new CrawlerServer.RemoteCrawlerClient(site);

            proxy = new RemoteCrawlerClient(site);

             //proxy = new 
            ClientCrawlerInfo[] list = proxy.Join(p);
            MessageBox.Show("Great Success!" + list.Count());
            // HandleEndJoin(list);
            //IAsyncResult ee = proxy.JoinAsync(p);
            //ee.AsyncWaitHandle = await 
            //IAsyncResult iar = proxy.BeginJoin(p, new AsyncCallback(OnEndJoin), null);
        }

        private void HandleEndJoin(ClientCrawlerInfo[] list)
        {
            if (list == null)
            {
                MessageBox.Show("Error: List is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitChatSession();
            }
            else
            {
                var e = new ProxyEventArgs();
                e.list = list;
                OnProxyEvent(e);
            }
        }

        /// <summary>
        ///     Raises the event for connected subscribers
        /// </summary>
        /// <param name="e"><see cref="ProxyCallBackEventArgs">ProxyCallBackEventArgs</see> event args</param>
        private void OnProxyCallBackEvent(ProxyCallBackEventArgs e)
        {
            if (ProxyCallBackEvent != null)
            {
                // Invokes the delegates. 
                ProxyCallBackEvent(this, e);
            }
        }

        /// <summary>
        ///     Raises the event for connected subscribers
        /// </summary>
        /// <param name="e"><see cref="ProxyEventArgs">ProxyEventArgs</see> event args</param>
        private void OnProxyEvent(ProxyEventArgs e)
        {
            if (ProxyEvent != null)
            {
                // Invokes the delegates. 
                ProxyEvent(this, e);
            }
        }
     
        public void SayAndClear(string to, string msg, bool pvt)
        {
            //if (!pvt)
            //    //proxy.ReturnCrawlingResults(msg,msg);
        }

        public void ExitChatSession()
        {
            try
            {
                proxy.Leave();
            }
            catch
            {
            }
            finally
            {
                AbortProxy();
            }
        }

        public void AbortProxy()
        {
            if (proxy != null)
            {
                proxy.Abort();
                proxy.Close();
                proxy = null;
            }
        }

        //public void Receive(Honeycomb.ClientCrawlerInfo sender, string message)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }

    #endregion
}