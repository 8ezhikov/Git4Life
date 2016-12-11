using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using Honeycomb.Interfaces;

namespace Honeycomb
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RemoteCrawlerService : IRemoteCrawler
    {
        //callback interface for clients
        ICrawlerClientCallback callback = null;
        private ConcurrentStack<Seed> globalSeedStack = new ConcurrentStack<Seed>();
        //delegate used for BroadcastEvent

        public ObservableCollection<ClientCrawlerInfo> ConnectedClientCrawlers
        {
            get { return _connectedClientCrawlers; }
            set { _connectedClientCrawlers = value; }
        }

        private ObservableCollection<ClientCrawlerInfo> _connectedClientCrawlers = new ObservableCollection<ClientCrawlerInfo>();

        public void GiveTestInitialTasks()
        {
            var dbContext = new CrawlerEntities();

            globalSeedStack.PushRange(dbContext.Seeds.ToArray());
            foreach (var crawlerCallback in _connectedClientCrawlers)
            {

                Seed nextSeed;
                if (globalSeedStack.TryPop(out nextSeed))
                {
                    crawlerCallback.SavedCallback.StartTestCrawl();
                }
                else
                {
                    throw new Exception("Couldn't pop from stack");
                }

            }
        }

        public void GiveInitialTasks()
        {
            var dbContext = new CrawlerEntities();

            globalSeedStack.PushRange(dbContext.Seeds.ToArray());
            foreach (var crawlerCallback in _connectedClientCrawlers)
            {

                Seed nextSeed;
                if (globalSeedStack.TryPop(out nextSeed))
                {
                    crawlerCallback.SavedCallback.StartCrawling(nextSeed.SeedDomainName);
                }
                else
                {
                    throw new Exception("Couldn't pop from stack");
                }

            }
        }

        public ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerNewInfo)
        {

            clientCrawlerNewInfo.SavedCallback = OperationContext.Current.GetCallbackChannel<ICrawlerClientCallback>();
            ConnectedClientCrawlers.Add(clientCrawlerNewInfo);

            return ConnectedClientCrawlers.ToArray();

        }

        public void ReturnIntermediateResults(string msg)
        {
        }

        public void ReturnCrawlingResults(CrawlerResultsDTO resultsDto)
        {
            SaveClientResultsToDatabase(resultsDto);


            Seed nextSeed;
            if (globalSeedStack.TryPop(out nextSeed))
            {
                //crawlerCallback.StartCrawling(nextSeed.SeedDomainName);
            }
            else
            {
                throw new Exception("Couldn't pop from stack");
            }
        }

        private void SaveClientResultsToDatabase(CrawlerResultsDTO resultsDto)
        {

            var dbContext = new CrawlerEntities();
            if (resultsDto.InternalLinksList != null)
            {
                foreach (var internalLink in resultsDto.InternalLinksList)
                {
                    var dbLink = ConvertInternalLinkDTOtoDB(internalLink);
                    dbContext.InternalLinks.Add(dbLink);
                }
            }
            if (resultsDto.ExternalLinksList != null)
            {
                foreach (var externalLink in resultsDto.ExternalLinksList)
                {
                    var dbLink = ConvertExternalLinkDTOtoDB(externalLink);
                    dbContext.ExternalLinks.Add(dbLink);
                }
            }
            if (resultsDto.BadLinksList != null)
            {
                foreach (var badLink in resultsDto.BadLinksList)
                {
                    var dbLink = ConvertBadLinkDTOtoDB(badLink);
                    dbContext.BadLinks.Add(dbLink);
                }
            }
            dbContext.SaveChanges();

           
        }

        private InternalLink ConvertInternalLinkDTOtoDB(InternalLinkDTO internalLinkDto)
        {
            var internalLinkDb = new InternalLink
            {
                PageSeedLink = internalLinkDto.PageSeedLink,
                PageIdSeedSpecific = internalLinkDto.PageIdSeedSpecific,
                IsProcessed = internalLinkDto.IsProcessed,
                PageLevel = internalLinkDto.PageLevel,
                PageLink = internalLinkDto.PageLink,
                IsHtml = internalLinkDto.IsHtml,
                OriginalPageLink = internalLinkDto.OriginalPageLink,
                LinkCount = internalLinkDto.LinkCount
            };
            return internalLinkDb;
        }


        private ExternalLink ConvertExternalLinkDTOtoDB(ExternalLinkDTO linkDto)
        {
            var linkDb = new ExternalLink
            {
                PageSeedLink = linkDto.PageSeedLink,
                LinkAnchor = linkDto.LinkAnchor,
                LinkPath = linkDto.LinkPath,
                LinkWeight = linkDto.LinkWeight,
                OriginalPageLevel = linkDto.OriginalPageLevel,
                OriginalPageLink = linkDto.OriginalPageLink,
                LinkCount = linkDto.LinkCount
            };

            return linkDb;
        }
        private BadLink ConvertBadLinkDTOtoDB(BadLinkDTO linkDto)
        {
            var linkDb = new BadLink
            {
                LinkPath = linkDto.LinkPath,
                OriginalPageLink = linkDto.OriginalPageLink
            };

            return linkDb;
        }

        public void Leave()
        {
          
        }
    }
}

