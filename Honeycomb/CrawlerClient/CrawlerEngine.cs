using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Honeycomb.Shared;
namespace CrawlerClient
{
    class CrawlerEngine
    {
         private int _maxPageLevel;
        public Crawler_DBEntities DbEntities;
        private readonly Hashtable _allLinks = new Hashtable();
        public bool ForceStop;
        private CrawlerStatus _status;
        private string _runingTime;
        private int _internalLinksCounter = 1;
        private const string StartingPageName = "Starting Page";
        private const int MaxLevel = 5;
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public string RuningTime
        {
            get { return _runingTime; }
            set
            {
                _runingTime = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("RuningTime");
            }
        }
        public CrawlerStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Status");
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public CrawlerEngine()
        {
            DbEntities = new Crawler_DBEntities();
            Status = CrawlerStatus.Wating;
        }

        public void StartDaWork()
        {
            var startingTime = DateTime.Now;
            _maxPageLevel = MaxLevel;
            Status = CrawlerStatus.Working;
            ForceStop = false;
            var testSeed = new Seed();
            testSeed.SeedDomainName = "http://webometrics.krc.karelia.ru/";
            var seedCollection = new List<Seed> {testSeed};
            foreach (var seed in seedCollection)
            {
                var startingAdress = seed.SeedDomainName;

                if (DbEntities.InternalLinks.Count(lin => lin.PageSeedLink == startingAdress) > 0)
                {
                    _internalLinksCounter = DbEntities.InternalLinks.Where(lin => lin.PageSeedLink == startingAdress).Max(
                      l => l.PageIdSeedSpecific) + 1;

                }
                else
                {
                    _internalLinksCounter = 1;
                }
                if (startingAdress != string.Empty)
                {
                    FillAllLinks(startingAdress);

                    if (_allLinks[startingAdress] == null)
                    {
                        AddInternalLink(startingAdress, startingAdress, 0, StartingPageName);
                    }

                    ScrapLinks(startingAdress, 0, startingAdress);
                    DbEntities.SaveChanges();
                    var unporcessedLinks = DbEntities.InternalLinks.Where(
                            link => link.PageSeedLink == startingAdress && link.IsProcessed == false).OrderBy(p => p.PageLevel).Take(2);
                    while (unporcessedLinks.Any() & ForceStop != true)
                    {
                        var selectedLink = unporcessedLinks.FirstOrDefault();

                        if (selectedLink != null)
                        {
                            selectedLink.IsProcessed = true;
                            DbEntities.SaveChanges();
                            ScrapLinks(selectedLink.PageLink, selectedLink.PageLevel, startingAdress);
                        }
                    }

                    _allLinks.Clear();
                }
            }
            if (ForceStop)
            {
                Status = CrawlerStatus.Stopped;
                ForceStop = false;
            }
            else
            {
                Status = CrawlerStatus.Finished;

            }

            RuningTime = (DateTime.Now - startingTime).ToString();


        }
        void FillAllLinks(string seedName)
        {
            var allCollectedInternalLinksForDomain =
                DbEntities.InternalLinks.Where(link => link.PageSeedLink == seedName);
            foreach (var internalLink in allCollectedInternalLinksForDomain)
            {
                _allLinks.Add(internalLink.PageLink, true);
            }

            var allCollectedExternalLinksForDomain =
               DbEntities.ExternalLinks.Where(link => link.PageSeedLink == seedName);
            foreach (var internalLink in allCollectedExternalLinksForDomain)
            {
                _allLinks.Add(internalLink.LinkPath, true);
            }
        }


        void ScrapLinks(string startingAdress, int pageLevel, string seedLink)
        {
            try
            {
                if (ForceStop)
                    return;
                var currentPageLevel = pageLevel + 1;

                if (currentPageLevel >= _maxPageLevel)
                    return;

                var htmlWeb = new HtmlWeb();
                var startingAdressUri = new Uri(startingAdress);
                HtmlDocument doc;
                try
                {
                    doc = htmlWeb.Load(startingAdress);
                    
                    if (doc.ParseErrors.Any(error => error.Code == HtmlParseErrorCode.CharsetMismatch))
                    {
                        doc = BeeBot.Shared.Helpers.GetDocumentCustomMode(doc.Encoding, startingAdress);
                    }
                    //doc = BeeBot.Shared.Helpers.GetDocumentCustomMode(Encoding.UTF8, startingAdress);
                }
                catch (Exception)
                {
                    doc = BeeBot.Shared.Helpers.GetDocumentCustomMode(Encoding.GetEncoding("windows-1251"), startingAdress);
                }

                if (pageLevel == 0)
                {
                    var metaTagsList = doc.DocumentNode.SelectNodes("//meta/@content");
                    if (metaTagsList != null)
                    {
                        foreach (var metaTag in metaTagsList)
                        {
                            var contentValue = metaTag.Attributes["content"].Value;

                            contentValue = contentValue.Trim();
                            var indexOfUrl = contentValue.ToLower().IndexOf("url=");
                            if (indexOfUrl != -1)
                            {
                                var redirectionPostfixLink = contentValue.Substring(indexOfUrl + 4);
                                string fullLink;

                                if (redirectionPostfixLink.Substring(0, 4) == "http")
                                {
                                    fullLink = redirectionPostfixLink;
                                }
                                else
                                {
                                    fullLink = startingAdressUri + redirectionPostfixLink;
                                }

                                var isURLexisting = _allLinks[fullLink];
                                if (isURLexisting == null)
                                {
                                    AddInternalLink(fullLink, seedLink, currentPageLevel, startingAdressUri.AbsoluteUri);
                                }
                                else
                                {
                                    IncrementInternalLink(fullLink);
                                }
                            }
                        }
                    }
                }
                DbEntities.SaveChanges();
                var linksList = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linksList != null)
                {
                    FilterLinks(linksList, startingAdressUri, currentPageLevel, seedLink);
                }
                DbEntities.SaveChanges();
                var framesList = doc.DocumentNode.SelectNodes("//frame");
                if (framesList != null)
                {
                    foreach (var frame in framesList)
                    {
                        var frameSource = frame.Attributes["src"].Value;
                        var pathPrefix = startingAdressUri.AbsoluteUri.Substring(0, startingAdressUri.AbsoluteUri.LastIndexOf('/') + 1);
                        string frameLink;

                        if (frameSource.Length > 5 && frameSource.Substring(0, 5) == "http:")
                        {
                            frameLink = frameSource;
                        }
                        else
                        {
                            frameLink = pathPrefix + frameSource;
                        }


                        var isURLexisting = _allLinks[frameLink];
                        if (isURLexisting == null)
                        {
                            AddInternalLink(frameLink, seedLink, currentPageLevel, startingAdressUri.AbsoluteUri);
                        }
                        else
                        {
                            IncrementInternalLink(frameLink);
                        }
                    }
                }
                DbEntities.SaveChanges();

            }
            catch (Exception)
            {
                //throw;
            }
        }

        public static string NormalizeUrl(string url)
        {
            var normalizedUrl = url;
            //if (normalizedUrl.Substring(url.Length - 1, 1) == "/")
            //{
            //    normalizedUrl = normalizedUrl.Substring(0, url.Length - 1);
            //}

            return normalizedUrl;
        }

        void FilterLinks(IEnumerable<HtmlNode> linksList, Uri startingAdressUri, int currentPageLevel, string seedLink)
        {
            try
            {

                foreach (var link in linksList)
                {
                    var attrib = link.Attributes["href"];
                    var rawLinkUrl = attrib.Value;

                    var safeLink = BeeBot.Shared.Helpers.TestLink(rawLinkUrl, startingAdressUri.DnsSafeHost);
                    if (safeLink != null)
                    {
                        if (safeLink.Substring(0, 6) == "mailto")
                            continue;
                        var linkUrl = NormalizeUrl(safeLink);
                        var linkUri = new Uri(linkUrl);


                        var tmplink = linkUri.DnsSafeHost.Replace("www.", "");
                        var tmpSeed = startingAdressUri.DnsSafeHost.Replace("www.", "");

                        if (tmplink == tmpSeed)
                        {
                            var isUrLexisting = _allLinks[linkUrl];
                            if (isUrLexisting == null)
                            {
                                AddInternalLink(linkUrl, seedLink, currentPageLevel, startingAdressUri.AbsoluteUri);
                            }
                            else
                            {
                                IncrementInternalLink(linkUrl);
                            }
                        }
                        else
                        {
                            var isUrLexisting = _allLinks[linkUrl];
                            if (isUrLexisting == null)
                            {
                                var linkPage = new ExternalLink
                                                   {
                                                       LinkAnchor = link.InnerText,
                                                       LinkPath = linkUrl,
                                                       LinkWeight = 1,
                                                       OriginalPageLink = NormalizeUrl(startingAdressUri.AbsoluteUri),
                                                       PageSeedLink = seedLink,
                                                       OriginalPageLevel = currentPageLevel - 1
                                                   };

                                DbEntities.AddToExternalLinks(linkPage);
                                _allLinks.Add(linkPage.LinkPath, true);
                            }
                            else
                            {
                                IncrementExternalLink(linkUrl);
                            }
                        }

                    }
                    else
                    {
                        var linkPage = new BadLink
                        {
                            LinkPath = rawLinkUrl,
                            OriginalPageLink = startingAdressUri.AbsoluteUri
                        };

                        DbEntities.AddToBadLinks(linkPage);
                    }
                }
            }
            catch (Exception e)
            {
                //throw;
            }
        }


        private void AddInternalLink(string linkUrl, string seedLink, int currentPageLevel, string originalPageLink)
        {
            originalPageLink = NormalizeUrl(originalPageLink);
            var linkPage = new InternalLink
            {
                IsHtml = false,
                IsProcessed = false,
                PageLink = linkUrl,
                PageSeedLink = seedLink,
                PageLevel = currentPageLevel,
                OriginalPageLink = originalPageLink,
                PageIdSeedSpecific = _internalLinksCounter


            };
            _internalLinksCounter++;
            DbEntities.AddToInternalLinks(linkPage);
            _allLinks.Add(linkPage.PageLink, true);

        }

        private void IncrementInternalLink(string linkUrl)
        {
            DbEntities.SaveChanges();
            var selectedLink = DbEntities.InternalLinks.First(lk => lk.PageLink == linkUrl);
            selectedLink.LinkCount++;
        }

        private void IncrementExternalLink(string linkUrl)
        {
            var selectedLink = DbEntities.ExternalLinks.First(lk => lk.LinkPath == linkUrl);
            selectedLink.LinkCount++;
        }

        public enum CrawlerStatus
        {
            Wating = 0,
            Working = 1,
            Stopped = 2,
            Finished = 3
        }
    }
    }
}
