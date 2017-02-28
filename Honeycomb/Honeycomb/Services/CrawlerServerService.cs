using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Honeycomb.Interfaces;
using Honeycomb.Services;
using Honeycomb.ViewModel;

namespace Honeycomb
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RemoteCrawlerService : IRemoteCrawler
    {
        public RemoteCrawlerService(MainViewModel modelLink)
        {
            modelReference = modelLink;
        }
        //callback interface for clients
        ICrawlerClientCallback callback = null;
        private ConcurrentStack<Seed> globalSeedStack = new ConcurrentStack<Seed>();
        private MainViewModel modelReference;
        //delegate used for BroadcastEvent

        public ObservableCollection<ClientCrawlerInfo> ConnectedClientCrawlers
        {
            get { return _connectedClientCrawlers; }
            set { _connectedClientCrawlers = value; }
        }

        private ObservableCollection<ClientCrawlerInfo> _connectedClientCrawlers = new ObservableCollection<ClientCrawlerInfo>();

        public void GiveTestInitialTasks()
        {
            var dbContext = new Crawler_DBEntities();

            globalSeedStack.PushRange(dbContext.Seeds.ToArray());
            var crawlerCounter = 0;
            foreach (var crawlerCallback in _connectedClientCrawlers)
            {

                Seed nextSeed;
                if (globalSeedStack.TryPop(out nextSeed))
                {
                    crawlerCallback.SavedCallback.StartTestCrawl();
                    crawlerCounter++;
                }
                else
                {
                    //throw new Exception("Couldn't pop from stack");
                }

            }
            modelReference.AppendTextToConsole("Test Crawling started for " + crawlerCounter + " crawlers");
        }

        public void GiveInitialTasks()
        {
            var dbContext = new Crawler_DBEntities();
            //  ConvertSeedLinkDTOtoDB(dbContext.Seeds.FirstOrDefault());
            if (!dbContext.Seeds.Any())
            {
                return;
            }

            globalSeedStack.PushRange(dbContext.Seeds.ToArray());

            var crawlerCounter = 0;

            foreach (var crawlerCallback in _connectedClientCrawlers)
            {

                Seed nextSeed;
                if (globalSeedStack.TryPop(out nextSeed))
                {
                    modelReference.AppendTextToConsole("Crawling started for " + crawlerCallback.ClientName + " Seed name is " + nextSeed.SeedDomainName);

                    crawlerCallback.SavedCallback.StartCrawling(Mapper.Map<SeedDTO>(nextSeed));
                    crawlerCounter++;

                }
                else
                {
                    //throw new Exception("Couldn't pop from stack");
                }
            }

        }

        public ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerNewInfo)
        {

            clientCrawlerNewInfo.SavedCallback = OperationContext.Current.GetCallbackChannel<ICrawlerClientCallback>();
            ConnectedClientCrawlers.Add(clientCrawlerNewInfo);
            var dbContext = new Crawler_DBEntities();

            var dbConnInfo = SeedModel.DataAccessService.ConvertToCrawlerConnection(clientCrawlerNewInfo);
            dbConnInfo.ConnectionTime = DateTime.Now;
            dbContext.CrawlerConnections.Add(dbConnInfo);
            modelReference.AppendTextToConsole("New client-crawler added! Name is  " + clientCrawlerNewInfo.ClientName + " Connection time is" + DateTime.Now);

            dbContext.SaveChanges();
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
                var foundcallback = _connectedClientCrawlers.FirstOrDefault(crw => crw.ClientIdentifier == resultsDto.ConnectionInfo.Id);
                foundcallback?.SavedCallback.StartCrawling(Mapper.Map<SeedDTO>(nextSeed));
            }
            else
            {
                //throw new Exception("Couldn't pop from stack");
            }
        }

        private void SaveClientResultsToDatabase(CrawlerResultsDTO resultsDto)
        {



            var dbContext = new Crawler_DBEntities();


            var dbBatch = Mapper.Map<Batch>(resultsDto.BatchInfo);
            dbBatch.CrawlerConnectionId = resultsDto.ConnectionInfo.Id;

            dbContext.Batches.Add(dbBatch);


            foreach (var siteResults in resultsDto.BatchInfo.resultCollection)
            {
                modelReference.AppendTextToConsole("Crawling results from" + siteResults.ProcessedSeed.SeedDomainName + " acquired. Saving onto DB...");

                if (siteResults.InternalLinksList != null)
                {
                    foreach (var internalLink in siteResults.InternalLinksList)
                    {
                        var dbLink = ConvertInternalLinkDTOtoDB(internalLink);
                        dbContext.InternalLinks.Add(dbLink);
                    }
                }
                if (siteResults.ExternalLinksList != null)
                {
                    foreach (var externalLink in siteResults.ExternalLinksList)
                    {
                        var dbLink = ConvertExternalLinkDTOtoDB(externalLink);
                        dbContext.ExternalLinks.Add(dbLink);
                    }
                }
                if (siteResults.BadLinksList != null)
                {
                    foreach (var badLink in siteResults.BadLinksList)
                    {
                        var dbLink = ConvertBadLinkDTOtoDB(badLink);
                        dbContext.BadLinks.Add(dbLink);
                    }
                }
                var seedFromDb = dbContext.Seeds.First(sd => sd.SeedIndex == siteResults.ProcessedSeed.SeedIndex);
                seedFromDb.IsProcessed = true;
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

        public void Leave(Guid clientGuid)
        {
            var clientToRemove = _connectedClientCrawlers.FirstOrDefault(cl => cl.ClientIdentifier == clientGuid);
            if (clientToRemove != null)
            {
                _connectedClientCrawlers.Remove(clientToRemove);
            }
        }
    }
}

