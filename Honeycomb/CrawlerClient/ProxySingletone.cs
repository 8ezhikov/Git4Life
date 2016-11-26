using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CrawlerClient.CrawlerServer;

namespace CrawlerClient
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class ConnectionSingleton : IRemoteCrawlerCallback
    {
        private static readonly Lazy<ConnectionSingleton> LazySingleton = new Lazy<ConnectionSingleton>(() => new ConnectionSingleton());
        private Guid _singletoneId;
        private IRemoteCrawler proxy;

        private ConnectionSingleton()
        {
            _singletoneId =  Guid.NewGuid();
        }

        public void StartTestCrawl()
        {
            Thread.Sleep(5000);

            var result = new CrawlerResultsDTO();
            result.ExternalLinksList.Add(new ExternalLinkDTO { LinkAnchor = "Test Run" });
            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
        }

        public void StartCrawling(string urlToCrawl)
        {

            var crawlerInstance = new CrawlerEngine();
            var seedToCrawl = new  SeedDTO { SeedDomainName = urlToCrawl };
            var result = crawlerInstance.StartCrawlingProcess(new[] {seedToCrawl});


            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
        }

        public static ConnectionSingleton Instance => LazySingleton.Value;


        public void Connect(ClientCrawlerInfo clientCrawlerInfo)
        {
            var site = new InstanceContext(this);

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            
            EndpointAddress address = new EndpointAddress("net.tcp://188.143.161.41:22222/chatservice/");
            var factory = new DuplexChannelFactory<IRemoteCrawler>( site,binding, address);
            
            proxy =  factory.CreateChannel();
            clientCrawlerInfo.ClientIdentifier = _singletoneId;
            proxy.Join(clientCrawlerInfo);

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
    }

}