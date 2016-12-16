using System;
using System.Collections.Generic;
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
            result.ExternalLinksList.Add(new ExternalLinkDTO {LinkAnchor = "Test Run"});
            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
        }

        public void StartCrawling(string urlToCrawl)
        {
            var crawlerInstance = new CrawlerEngine();
            var seedToCrawl = new SeedDTO {SeedDomainName = urlToCrawl};
            var result = crawlerInstance.StartCrawlingProcess(new[] {seedToCrawl});

            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
        }

        public void Disconnect()
        {
            proxy.Leave(_singletoneId);
        }

        public bool Connect(ClientCrawlerInfo clientCrawlerInfo)
        {
            try
            {
                var site = new InstanceContext(this);

                var binding = new NetTcpBinding(SecurityMode.None);
                var address = new EndpointAddress("net.tcp://188.143.161.41:22222/chatservice/");
                var factory = new DuplexChannelFactory<IRemoteCrawler>(site, binding, address);

                proxy = factory.CreateChannel();
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