using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Honeycomb.Shared;
using HtmlAgilityPack;

namespace CrawlerClient
{
    class CrawlerEngine
    {
         private int _maxPageLevel;
        private readonly Hashtable _allLinks = new Hashtable();
        public bool ForceStop;
        private CrawlerStatus _status;
        private string _runingTime;
        private int _internalLinksCounter = 1;
        private const string StartingPageName = "Starting Page";
        private const int MaxLevel = 5;
        private List<InternalLink> InternalLinksList = new List<InternalLink>();
        private List<ExternalLink> ExternalLinksList = new List<ExternalLink>();
        private List<BadLink> BadLinksList = new List<BadLink>();


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
            Status = CrawlerStatus.Wating;
        }

        public void StartDaWork()
        {
            var startingTime = DateTime.Now;
            _maxPageLevel = MaxLevel;
            Status = CrawlerStatus.Working;
            ForceStop = false;
            var testSeed = new Seed { SeedDomainName = "http://mathem.krc.karelia.ru/" };
            var seedCollection = new List<Seed> {testSeed};
            foreach (var seed in seedCollection)
            {
                var startingAdress = seed.SeedDomainName;
                if (InternalLinksList.Count(lin => lin.PageSeedLink == startingAdress) > 0)
                {
                    _internalLinksCounter = InternalLinksList.Where(lin => lin.PageSeedLink == startingAdress).Max(
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
                    var unporcessedLinks = InternalLinksList.Where(
                            link => link.PageSeedLink == startingAdress && link.IsProcessed == false).OrderBy(p => p.PageLevel).Take(2);
                    while (unporcessedLinks.Any() & ForceStop != true)
                    {
                        var selectedLink = unporcessedLinks.FirstOrDefault();

                        if (selectedLink != null)
                        {
                            selectedLink.IsProcessed = true;
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
            MessageBox.Show(RuningTime);

        }
        void FillAllLinks(string seedName)
        {
            var allCollectedInternalLinksForDomain =
                InternalLinksList.Where(link => link.PageSeedLink == seedName);
            foreach (var internalLink in allCollectedInternalLinksForDomain)
            {
                _allLinks.Add(internalLink.PageLink, true);
            }

            var allCollectedExternalLinksForDomain =
               InternalLinksList.Where(link => link.PageSeedLink == seedName);
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
                        doc = GetDocumentCustomMode(doc.Encoding, startingAdress);
                    }
                    //doc = BeeBot.Shared.Helpers.GetDocumentCustomMode(Encoding.UTF8, startingAdress);
                }
                catch (Exception)
                {
                    doc = GetDocumentCustomMode(Encoding.GetEncoding("windows-1251"), startingAdress);
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
                var linksList = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linksList != null)
                {
                    FilterLinks(linksList, startingAdressUri, currentPageLevel, seedLink);
                }
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
        public static string TestLink(string url, string startingAddress)
        {

            if (url == "" || url[0] == '/')
                url = "http://" + startingAddress + url;

            if (url.Length > 7 && url.Substring(0, 7) == "mailto:")
                return null;
            if (url.Contains("javascript:"))
                return null;
            try
            {
                var uri = new Uri(url);
                return uri.AbsoluteUri;
            }
            catch (Exception)
            {
                try
                {
                    url = "http://" + startingAddress + "/" + url;
                    var uri = new Uri(url);
                    return uri.AbsoluteUri;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        void FilterLinks(IEnumerable<HtmlNode> linksList, Uri startingAdressUri, int currentPageLevel, string seedLink)
        {
            try
            {

                foreach (var link in linksList)
                {
                    var attrib = link.Attributes["href"];
                    var rawLinkUrl = attrib.Value;

                    var safeLink = TestLink(rawLinkUrl, startingAdressUri.DnsSafeHost);
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
                                ExternalLinksList.Add(linkPage);
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
                        BadLinksList.Add(linkPage);
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
            InternalLinksList.Add(linkPage);
            _allLinks.Add(linkPage.PageLink, true);

        }

        private void IncrementInternalLink(string linkUrl)
        {
            var selectedLink = InternalLinksList.First(lk => lk.PageLink == linkUrl);
            selectedLink.LinkCount++;
        }

        private void IncrementExternalLink(string linkUrl)
        {
            var selectedLink = ExternalLinksList.First(lk => lk.LinkPath == linkUrl);
            selectedLink.LinkCount++;
        }

        public enum CrawlerStatus
        {
            Wating = 0,
            Working = 1,
            Stopped = 2,
            Finished = 3
        }
        public static HtmlDocument GetDocumentCustomMode(Encoding encoding, string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x";
            var resultingDocument = new HtmlDocument();

            Stream resultingStream = new MemoryStream();
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.Headers["transfer-encoding"] == "chunked")
                    {
                        var sb = new StringBuilder();
                        var buf = new byte[8192];
                        var resStream = response.GetResponseStream();
                        var count = 0;
                        do
                        {
                            if (resStream != null) count = resStream.Read(buf, 0, buf.Length);
                            if (count != 0)
                            {
                                var tmpString = encoding.GetString(buf, 0, count);
                                sb.Append(tmpString);
                            }
                        } while (count > 0);
                        var resultingString = sb.ToString();
                        if (Equals(encoding, Encoding.GetEncoding("windows-1251")))
                        {
                            resultingString = resultingString.Replace("win-1251", "UTF-8");
                        }
                        resultingDocument.LoadHtml(resultingString);

                    }
                    else
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            var buffer = new byte[0x1000];
                            int bytes;
                            while (responseStream != null && (bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                resultingStream.Write(buffer, 0, bytes);
                            }
                        }

                        resultingStream.Seek(0, SeekOrigin.Begin);
                        using (var sr = new StreamReader(resultingStream, encoding))
                        {
                            resultingDocument.LoadHtml(sr.ReadToEnd());
                        }

                    }
                }
                return resultingDocument;
            }
            catch (Exception)
            {
                return new HtmlDocument();
            }
        }
    }
 
    }

