using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using AutoMapper;
using EntityFramework.Utilities;
using Honeycomb.Interfaces;
using Honeycomb.Services;
using Honeycomb.ViewModel;
using Serilog;

namespace Honeycomb
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RemoteCrawlerService : IRemoteCrawler
    {
        public RemoteCrawlerService(MainViewModel modelLink)
        {
            modelReference = modelLink;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Seq("http://193.124.113.235:5341")
                .CreateLogger();
        }

        //callback interface for clients
        ICrawlerClientCallback callback = null;
        private ConcurrentStack<Seed> globalSeedStack = new ConcurrentStack<Seed>();
        private MainViewModel modelReference;
        private Stopwatch timeTracker = new Stopwatch();
        public CrawlingMethod CrawlMode;
        public Dictionary<int, ConcurrentStack<int>> SeedsByBatchAllocation;
        //delegate used for BroadcastEvent

        public ObservableCollection<ClientCrawlerInfo> ConnectedClientCrawlers
        {
            get { return _connectedClientCrawlers; }
            set { _connectedClientCrawlers = value; }
        }

        private ObservableCollection<ClientCrawlerInfo> _connectedClientCrawlers =
            new ObservableCollection<ClientCrawlerInfo>();

        public void GiveTestInitialTasks()
        {
            var dbContext = new Crawler_DBEntities();
            timeTracker.Start();
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
            timeTracker.Start();
            modelReference.AppendTextToConsole("!!!! Starting CRAWLING!!!");
            modelReference.AppendTextToConsole("Time now is" + DateTime.Now);

            var dbContext = new Crawler_DBEntities();
            //  ConvertSeedLinkDTOtoDB(dbContext.Seeds.FirstOrDefault());
            if (!dbContext.Seeds.Any())
            {
                return;
            }

            globalSeedStack.PushRange(dbContext.Seeds.Where(sd => sd.IsProcessed == false).ToArray());

            var crawlerCounter = 0;

            foreach (var crawlerCallback in _connectedClientCrawlers)
            {
                if (CrawlMode == CrawlingMethod.RoundRobin)
                {
                    int batchSize = 1;

                    Seed[] nextSeed1 = new Seed[batchSize];
                    if (globalSeedStack.TryPopRange(nextSeed1, 0, batchSize) > 0)
                    {
                        modelReference.AppendTextToConsole("Crawling started for " + crawlerCallback.ClientName +
                                                           " Seed URL is " + nextSeed1.FirstOrDefault().SeedDomainName);
                        var seedList = new List<SeedDTO>();
                        seedList.AddRange(nextSeed1.Select(it => Mapper.Map<SeedDTO>(it)));
                        crawlerCallback.SavedCallback.StartCrawling(seedList);
                        crawlerCounter++;

                    }
                    else
                    {
                        //throw new Exception("Couldn't pop from stack");
                    }
                }
                else
                {
                    int batchSize = 1;

                    int[] seedNumToCrawl = { };
                    if (SeedsByBatchAllocation[crawlerCallback.BatchNumber].TryPopRange(seedNumToCrawl, 0, batchSize) > 0)
                    {
                        var seedList = new List<SeedDTO>();

                        foreach (var seedNum in seedNumToCrawl)
                        {
                            var dbSeed = dbContext.Seeds.First(_ => _.SeedIndex == seedNum);
                            modelReference.AppendTextToConsole("Crawling started for " + crawlerCallback.ClientName +
                                                      " Seed URL is " + dbSeed.SeedDomainName);
                            seedList.Add(Mapper.Map<SeedDTO>(dbSeed));

                        }
                        crawlerCallback.SavedCallback.StartCrawling(seedList);
                        crawlerCounter++;

                    }
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
            modelReference.AppendTextToConsole("New client-crawler added! Name is  " + clientCrawlerNewInfo.ClientName +
                                               " Connection time is" + DateTime.Now);

            dbContext.SaveChanges();
            return ConnectedClientCrawlers.ToArray();

        }

        public void ReturnIntermediateResults(string msg)
        {
        }

        public void ReturnCrawlingResults(CrawlerResultsDTO resultsDto)
        {
           var stopWatT = new Stopwatch();
            stopWatT.Start();
            SaveClientResultsToDatabase(resultsDto);
            stopWatT.Stop();
            int batchSize = 1;
            Seed[] nextSeed1 = new Seed[batchSize];
            if (CrawlMode == CrawlingMethod.RoundRobin)
            {
                if (globalSeedStack.TryPopRange(nextSeed1, 0, batchSize) > 0)
                {
                    var foundcallback =
                        _connectedClientCrawlers.FirstOrDefault(
                            crw => crw.ClientIdentifier == resultsDto.ConnectionInfo.Id);
                    var seedList = new List<SeedDTO>();
                    seedList.AddRange(nextSeed1.Select(it => Mapper.Map<SeedDTO>(it)));
                    foundcallback?.SavedCallback.StartCrawling(seedList);
                }
                else
                {
                    modelReference.AppendTextToConsole("All Seeds are processed!");

                    modelReference.AppendTextToConsole("Time now is" + DateTime.Now);
                    timeTracker.Stop();
                    modelReference.AppendTextToConsole("Time elapsed" + timeTracker.ElapsedTicks);
                    modelReference.AppendTextToConsole("Time elapsed" + timeTracker.Elapsed);

                    //throw new Exception("Couldn't pop from stack");
                }
            }
            else
            {
                var foundcallback =
                    _connectedClientCrawlers.FirstOrDefault(
                        crw => crw.ClientIdentifier == resultsDto.ConnectionInfo.Id);
                int[] seedNumToCrawl = { };
                if (SeedsByBatchAllocation[foundcallback.BatchNumber].TryPopRange(seedNumToCrawl, 0, batchSize) > 0)
                {
                    var seedList = new List<SeedDTO>();
                    var dbContext = new Crawler_DBEntities();
                    foreach (var seedNum in seedNumToCrawl)
                    {
                        var dbSeed = dbContext.Seeds.First(_ => _.SeedIndex == seedNum);
                        modelReference.AppendTextToConsole("Crawling started for " + foundcallback.ClientName +
                                                  " Seed URL is " + dbSeed.SeedDomainName);
                        seedList.Add(Mapper.Map<SeedDTO>(dbSeed));

                    }
                    foundcallback.SavedCallback.StartCrawling(seedList);

                }
            }
        }

        private void SaveClientResultsToDatabase(CrawlerResultsDTO resultsDto)
        {



            var dbContext = new Crawler_DBEntities();
            dbContext.Configuration.AutoDetectChangesEnabled = false;

            var dbBatch = Mapper.Map<Batch>(resultsDto.BatchInfo);
            dbBatch.CrawlerConnectionId = resultsDto.ConnectionInfo.Id;
            var seedInts = resultsDto.BatchInfo.resultCollection.Select(rs => rs.ProcessedSeed.SeedIndex);
            var seedIds = seedInts.Aggregate("", (current, va) => current + (va + ","));
            dbBatch.SeedIds = seedIds;
            int internalLinks = 0;
            int externalLinks = 0;
            foreach (var result in resultsDto.BatchInfo.resultCollection)
            {
                internalLinks += result.InternalLinksList.Count;
                externalLinks += result.ExternalLinksList.Count;
            }
            dbBatch.NumberOfCrawledExternalLinks = externalLinks;
            dbBatch.NumberOfCrawledInternalLinks = internalLinks;
            dbContext.Batches.Add(dbBatch);

            try
            {
                dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                Log.Error(e, "");
                throw;
            }
            foreach (var siteResults in resultsDto.BatchInfo.resultCollection)
            {
                modelReference.AppendTextToConsole("Crawling results from" + siteResults.ProcessedSeed.SeedDomainName +
                                                   " acquired. Saving onto DB...");

                if (siteResults.InternalLinksList != null)
                {
                    var intLinks = new List<InternalLink>();
                    foreach (var internalLink in siteResults.InternalLinksList)
                    {
                        var dbLink = ConvertInternalLinkDTOtoDB(internalLink);
                        intLinks.Add(dbLink);
                    }
                    using (var db = new Crawler_DBEntities())
                    {
                        EFBatchOperation.For(db, db.InternalLinks).InsertAll(intLinks);
                    }
                    
                    //dbContext.InternalLinks.AddRange(intLinks);
                }
                if (siteResults.ExternalLinksList != null)
                {
                    var extLinks = new List<ExternalLink>();
                    foreach (var externalLink in siteResults.ExternalLinksList)
                    {
                        var dbLink = ConvertExternalLinkDTOtoDB(externalLink);
                        extLinks.Add(dbLink);
                    }
                    using (var db = new Crawler_DBEntities())
                    {
                        EFBatchOperation.For(db, db.ExternalLinks).InsertAll(extLinks);
                    }
                    //dbContext.ExternalLinks.AddRange(extLinks);
                }
                if (siteResults.BadLinksList != null)
                {
                    var badLinks = new List<BadLink>();
                    foreach (var badLink in siteResults.BadLinksList)
                    {
                        var dbLink = ConvertBadLinkDTOtoDB(badLink);
                        badLinks.Add(dbLink);
                    }
                    using (var db = new Crawler_DBEntities())
                    {
                        EFBatchOperation.For(db, db.BadLinks).InsertAll(badLinks);
                    }
                    //dbContext.BadLinks.AddRange(badLinks);
                }
                var seedFromDb = dbContext.Seeds.First(sd => sd.SeedIndex == siteResults.ProcessedSeed.SeedIndex);
                seedFromDb.IsProcessed = true;
            }

            try
            {

                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error(e, "");
                throw;
            }

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

    public enum CrawlingMethod
    {
        RoundRobin,
        BatchMode
    }
}

