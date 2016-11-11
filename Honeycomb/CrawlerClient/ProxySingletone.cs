using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using CrawlerClient.CrawlerServer;

namespace CrawlerClient
{
    //public class ProxyEventArgs : EventArgs
    //{
    //    public ClientCrawlerInfo[] list;
    //}

    ///// <summary>
    /////     Proxy callback event args
    ///// </summary>
    //public class ProxyCallBackEventArgs : EventArgs
    //{
    //    public ClientCrawlerInfo person = null;
    //}

 

    public sealed class ConnectionSingleton : IRemoteCrawlerCallback
    {

        //public delegate void ProxyCallBackEventHandler(object sender, ProxyCallBackEventArgs e);

        //public delegate void ProxyEventHandler(object sender, ProxyEventArgs e);

        private static readonly Lazy<ConnectionSingleton> LazySingleton =
            new Lazy<ConnectionSingleton>(() => new ConnectionSingleton());

        private RemoteCrawlerClient proxy;

        //main proxy event

        //public event ProxyEventHandler ProxyEvent;
        ////callback proxy event

        //public event ProxyCallBackEventHandler ProxyCallBackEvent;


        private ConnectionSingleton()
        {
        }


        public void Receive(ClientCrawlerInfo sender, string message)
        {
            //Receive(sender, message);
        }

   

        public void StartCrawling(string urlToCrawl)
        {

            var crawlerInstance = new CrawlerEngine();
            var result = crawlerInstance.StartCrawlingProcess(urlToCrawl);

            proxy.ReturnCrawlingResults(result);


            //Here we crawl. Crawl and crawl.


            //And then we want to return results
            //   Receive(sender, message, CallBackT ype.ReceiveWhisper);
        }

        //private void Receive(ClientCrawlerInfo sender, string message, CallBackType callbackType)
        //{
        //    var e = new ProxyCallBackEventArgs();
        //    e.message = message;
        //    e.callbackType = callbackType;
        //    e.person = sender;
        //    //OnProxyCallBackEvent(e);
        //}



        public static ConnectionSingleton Instance => LazySingleton.Value;


        public void Connect(ClientCrawlerInfo p)
        {
            var site = new InstanceContext(this);

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            EndpointAddress address = new EndpointAddress("net.tcp://188.143.161.41:22222/chatservice/");
            var factory = new DuplexChannelFactory<IRemoteCrawler>( site,binding, address);
            var yourInterface = factory.CreateChannel();
            yourInterface.Join( p);

            ////var anotherProxy = new CrawlerServer.RemoteCrawlerClient(site);

            //proxy = new RemoteCrawlerClient(site);

            ////proxy = new 
            //ClientCrawlerInfo[] list = proxy.Join(p);
            MessageBox.Show("Great Success!" );
            // HandleEndJoin(list);
            //IAsyncResult ee = proxy.JoinAsync(p);
            //ee.AsyncWaitHandle = await 
            //IAsyncResult iar = proxy.BeginJoin(p, new AsyncCallback(OnEndJoin), null);
        }

        //private void HandleEndJoin(ClientCrawlerInfo[] list) 
        //{
        //    if (list == null)
        //    {
        //        MessageBox.Show("Error: List is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        ExitChatSession();
        //    }
        //    else
        //    {
        //        var e = new ProxyEventArgs();
        //        e.list = list;
        //        OnProxyEvent(e);
        //    }
        //}

        /// <summary>
        /////     Raises the event for connected subscribers
        ///// </summary>
        ///// <param name="e"><see cref="ProxyCallBackEventArgs">ProxyCallBackEventArgs</see> event args</param>
        //private void OnProxyCallBackEvent(ProxyCallBackEventArgs e)
        //{
        //    if (ProxyCallBackEvent != null)
        //    {
        //        // Invokes the delegates. 
        //        ProxyCallBackEvent(this, e);
        //    }
        //}

        ///// <summary>
        /////     Raises the event for connected subscribers
        ///// </summary>
        ///// <param name="e"><see cref="ProxyEventArgs">ProxyEventArgs</see> event args</param>
        //private void OnProxyEvent(ProxyEventArgs e)
        //{
        //    if (ProxyEvent != null)
        //    {
        //        // Invokes the delegates. 
        //        ProxyEvent(this, e);
        //    }
        //}

    }

}