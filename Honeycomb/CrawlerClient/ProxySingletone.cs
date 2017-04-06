using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CrawlerClient.CrawlerServer;
using CrawlerClient.ViewModel;
using Serilog;

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
        private MainViewModel InjectedViewModel;

        private ConnectionSingleton()
        {
            _singletoneId = Guid.NewGuid();
        }

        public void StartTestCrawl()
        {
            //Thread.Sleep(5000);

            //var result = new CrawlerResultsDTO();
            //result.ExternalLinksList = new List<ExternalLinkDTO>();
            //result.ExternalLinksList.Add(new ExternalLinkDTO
            //{
            //    LinkAnchor = "Test Run",
            //    LinkPath = "",
            //    OriginalPageLink = "",
            //    PageSeedLink = ""
            //});
            //Task.Factory.StartNew(() =>
            //{
            //    proxy.ReturnCrawlingResults(result);
            //});
        }

        public void InjectViewModel(MainViewModel injectedViewModel)
        {
            InjectedViewModel = injectedViewModel;
        }
        public void StartCrawling(List<SeedDTO> seedList, int maxLevel)
        {
            var crawlerInstance = new CrawlerEngine();
            CrawlerResultsDTO result;
            InjectedViewModel.CrawlerStatus = "Crawling Started";
            try
            {
                result = crawlerInstance.StartCrawlingProcess(seedList, maxLevel);
                result.ConnectionInfo = new ConnectionInfoDTO();
                result.ConnectionInfo.Id = _singletoneId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                Log.CloseAndFlush();
                return;
            }
            InjectedViewModel.CrawlerStatus = "Crawling finished returning results";
            Task.Factory.StartNew(() =>
            {
                proxy.ReturnCrawlingResults(result);
            });
            InjectedViewModel.CrawlerStatus = "All results returned to main server! Waiting for new task";

        }

        public void Disconnect()
        {
            try
            {
                proxy?.Leave(_singletoneId);

            }
            catch (Exception e)
            {
                Log.Error(e, "");
                Log.CloseAndFlush();
            }
        }

        public bool Connect(ClientCrawlerInfo clientCrawlerInfo,string serverAddress)
        {
            try
            {
                var site = new InstanceContext(this);

                var binding = new NetTcpBinding(SecurityMode.None);
                //var address = new EndpointAddress("net.tcp://localhost:22222/chatservice/");

                var address = new EndpointAddress("net.tcp://"+serverAddress+ "/chatservice/");
                var factory = new DuplexChannelFactory<IRemoteCrawler>(site, binding, address);
                
                proxy = factory.CreateChannel();
                ((IContextChannel)proxy).OperationTimeout = new TimeSpan(3, 0, 10);
                
                clientCrawlerInfo.ClientIdentifier = _singletoneId;
                proxy.Join(clientCrawlerInfo);

                return true;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error happened" + ex.Message);
                Log.Error(ex, "");
                return false;
            }

        }
    }

}