using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private static readonly Lazy<ConnectionSingleton> LazySingleton =
            new Lazy<ConnectionSingleton>(() => new ConnectionSingleton());

        private readonly Guid _singletoneId;
        private IRemoteCrawler proxy;
        public static ConnectionSingleton Instance => LazySingleton.Value;


        private ConnectionSingleton()
        {
            _singletoneId = Guid.NewGuid();
        }

        public void StartTestCrawl()
        {
            Thread.Sleep(5000);

            var result = new CrawlerResultsDTO();
            result.ExternalLinksList = new List<ExternalLinkDTO>();
            result.ExternalLinksList.Add(new ExternalLinkDTO { LinkAnchor = "Test Run", LinkPath  = "",OriginalPageLink ="", PageSeedLink=""});
            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
        }

        public void StartCrawling(SeedDTO seed)
        {
            var crawlerInstance = new CrawlerEngine();
            CrawlerResultsDTO result;

            try
            {
                result = crawlerInstance.StartCrawlingProcess(new[] { seed });
            }
            catch (Exception ex)
            {
                
                return;
            }

            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
        }

        public void Disconnect()
        {
            proxy?.Leave(_singletoneId);
        }

        public bool Connect(ClientCrawlerInfo clientCrawlerInfo)
        {
            try
            {
                var site = new InstanceContext(this);

                var binding = new NetTcpBinding(SecurityMode.None);
                var address = new EndpointAddress("net.tcp://193.124.113.235:22222/chatservice/");
                var factory = new DuplexChannelFactory<IRemoteCrawler>(site, binding, address);
                
                proxy = factory.CreateChannel();
                ((IContextChannel)proxy).OperationTimeout = new TimeSpan(1, 0, 10);
                clientCrawlerInfo.ClientIdentifier = _singletoneId;
                proxy.Join(clientCrawlerInfo);

                return true;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error happened" + ex.Message);
                return false;
            }

        }
    }

}