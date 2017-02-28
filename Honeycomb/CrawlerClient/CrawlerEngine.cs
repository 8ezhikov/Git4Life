using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using CrawlerClient.CrawlerServer;
using HtmlAgilityPack;

namespace CrawlerClient
{
    class CrawlerEngine
    {
        private int _maxPageLevel;
        //private readonly HashSet<string> _allLinks = new HashSet<string>();
        private bool _forceStop;
        private int _internalLinksIdCounter = 1;
        private const string StartingPageName = "Starting Page";
        private readonly Dictionary<string, InternalLinkDTO> _internalLinksDictionary = new Dictionary<string, InternalLinkDTO>();
        private readonly Dictionary<string, ExternalLinkDTO> _externalLinksDictionary = new Dictionary<string, ExternalLinkDTO>();
        private readonly List<BadLinkDTO> _badLinksList = new List<BadLinkDTO>();
        private readonly Stack<InternalLinkDTO> _internalUnprocessedLinks = new Stack<InternalLinkDTO>();


        public void ForceCrawlerStop()
        {
            _forceStop = true;
        }

        private  void CleanCrawler()
        {
            _internalLinksDictionary.Clear();
            _externalLinksDictionary.Clear();
            _badLinksList.Clear();
            _internalUnprocessedLinks.Clear();
        }

        public CrawlerResultsDTO StartCrawlingProcess(IEnumerable<SeedDTO> seedsToCrawl, int maxCrawlLevel = 2)
        {
            var timetracker = new Stopwatch();

            timetracker.Start();

            var startTime = DateTime.Now;

            _maxPageLevel = maxCrawlLevel;
            _forceStop = false;
            var resultCollection = new List<SiteResults>();
            foreach (var seed in seedsToCrawl)
            {
                var startingAddress = seed.SeedDomainName;

                _internalLinksIdCounter = 1;
                if (startingAddress == string.Empty)
                {
                    continue;
                }

                //if (_allLinks.Contains(startingAddress))
                //{
                //    AddInternalLink(startingAddress, startingAddress, 0, StartingPageName);
                //}

                FindLinks(startingAddress, 0, startingAddress);

                while (_internalUnprocessedLinks.Count > 0 & _forceStop != true)
                {
                    var selectedLink = _internalUnprocessedLinks.Pop();

                    selectedLink.IsProcessed = true;
                    FindLinks(selectedLink.PageLink, selectedLink.PageLevel, startingAddress);

                }

                //_allLinks.Clear();

                if (_forceStop)
                {
                    _forceStop = false;
                }
                var siteResult = new SiteResults();
                siteResult.ProcessedSeed = seed;
                siteResult.BadLinksList = _badLinksList.ToList();
                siteResult.ExternalLinksList = _externalLinksDictionary.Select(pair => pair.Value).ToList();
                siteResult.InternalLinksList = _internalLinksDictionary.Select(pair => pair.Value).ToList();

                resultCollection.Add(siteResult);
            }

            timetracker.Stop();
            var runingTime = timetracker.Elapsed.Seconds;
            //MessageBox.Show(RuningTime);
            var batchInfo = new BatchDTO();
            batchInfo.CrawlingTime = runingTime;
            batchInfo.StartTime = startTime;
            //batchInfo.NumberOfCrawledExternalLinks = _externalLinksDictionary.Count;
            //batchInfo.NumberOfCrawledInternalLinks = _internalLinksIdCounter;
            batchInfo.resultCollection = resultCollection;



            var result = new CrawlerResultsDTO
            {
                BatchInfo = batchInfo,
            };

            return result;
        }

        private void FindLinks(string startingAddress, int pageLevel, string seedLink)
        {
            try
            {
                if (_forceStop)
                    return;

                if (pageLevel > _maxPageLevel)
                    return;

                var currentPageLevel = pageLevel + 1;


                var doc = CrawlerHelpers.LoadDocumentByUrl(startingAddress);
                var startingAddressUri = new Uri(startingAddress);


                if (pageLevel == 0)
                {
                    FindRedirects(seedLink, doc, startingAddressUri, currentPageLevel);
                }
                var linksList = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linksList != null)
                {
                    FilterLinks(linksList, startingAddressUri, currentPageLevel, seedLink);
                }
                var framesList = doc.DocumentNode.SelectNodes("//frame");
                if (framesList == null) return;
                foreach (var frame in framesList)
                {
                    var frameSource = frame.Attributes["src"].Value;
                    var pathPrefix = startingAddressUri.AbsoluteUri.Substring(0, startingAddressUri.AbsoluteUri.LastIndexOf('/') + 1);
                    string frameLink;

                    if (frameSource.Length > 5 && frameSource.Substring(0, 5) == "http:")
                    {
                        frameLink = frameSource;
                    }
                    else
                    {
                        frameLink = pathPrefix + frameSource;
                    }

                    AddInternalLink(frameLink, seedLink, currentPageLevel, startingAddressUri.AbsoluteUri);
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        private void FindRedirects(string seedLink, HtmlDocument doc, Uri startingAddressUri, int currentPageLevel)
        {
            var metaTagsList = doc.DocumentNode.SelectNodes("//meta/@content");
            if (metaTagsList == null) return;
            foreach (var metaTag in metaTagsList)
            {
                var contentValue = metaTag.Attributes["content"].Value;

                contentValue = contentValue.Trim();
                var indexOfUrl = contentValue.ToLower().IndexOf("url=", StringComparison.Ordinal);
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
                        fullLink = startingAddressUri + redirectionPostfixLink;
                    }

                    AddInternalLink(fullLink, seedLink, currentPageLevel, startingAddressUri.AbsoluteUri);
                }
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

        private static string TestLink(string url, string startingAddress)
        {

            if (url == "" || url[0] == '/')
                url = "http://" + startingAddress + url;

            if (url.Length > 7 && url.Substring(0, 7) == "mailto:")
                return null;
            if (url.Contains("javascript:"))
                return null;
            //try
            //{
            //    var uri = new Uri(url);
            //    return uri.AbsoluteUri;
            //}
            //catch (Exception)
            //{
            //    try
            //    {
            //        url = "http://" + startingAddress + "/" + url;
            //        var uri = new Uri(url);
            //        return uri.AbsoluteUri;
            //    }
            //    catch (Exception e)
            //    {
            //        LogException(e);
            //        return null;
            //    }
            //}
            try
            {
                Uri uriResult;
                var result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result)
                {
                    return uriResult.AbsoluteUri;
                }
                url = "http://" + startingAddress + "/" + url;
                var uri = new Uri(url);
                return uri.AbsoluteUri;
            }
            catch (Exception e)
            {
                LogException(e);
                return null;
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
                            AddInternalLink(linkUrl, seedLink, currentPageLevel, startingAdressUri.AbsoluteUri);

                        }
                        else
                        {
                            AddExternalLink(startingAdressUri, currentPageLevel, seedLink, linkUrl, link);
                        }
                    }
                    else
                    {
                        var linkPage = new BadLinkDTO
                        {
                            LinkPath = rawLinkUrl,
                            OriginalPageLink = startingAdressUri.AbsoluteUri
                        };
                        _badLinksList.Add(linkPage);
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
                //throw;
            }
        }

        private void AddExternalLink(Uri startingAdressUri, int currentPageLevel, string seedLink, string linkUrl, HtmlNode link)
        {
            if (!_externalLinksDictionary.ContainsKey(linkUrl))
            {
                var linkPage = new ExternalLinkDTO
                {
                    LinkAnchor = link.InnerText,
                    LinkPath = linkUrl,
                    LinkWeight = 1,
                    OriginalPageLink = NormalizeUrl(startingAdressUri.AbsoluteUri),
                    PageSeedLink = seedLink,
                    OriginalPageLevel = currentPageLevel - 1
                };
                _externalLinksDictionary.Add(linkPage.LinkPath, linkPage);
            }
            else
            {
                _externalLinksDictionary[linkUrl].LinkCount++;
            }
        }

        private void AddInternalLink(string linkUrl, string seedLink, int currentPageLevel, string originalPageLink)
        {
            if (!_internalLinksDictionary.ContainsKey(linkUrl))
            {
                originalPageLink = NormalizeUrl(originalPageLink);
                var linkPage = new InternalLinkDTO
                {
                    IsHtml = false,
                    IsProcessed = false,
                    PageLink = linkUrl,
                    PageSeedLink = seedLink,
                    PageLevel = currentPageLevel,
                    OriginalPageLink = originalPageLink,
                    PageIdSeedSpecific = _internalLinksIdCounter
                };

                _internalLinksIdCounter++;
                _internalUnprocessedLinks.Push(linkPage);
                _internalLinksDictionary.Add(linkPage.PageLink, linkPage);
            }
            else
            {
                _internalLinksDictionary[linkUrl].LinkCount++;
            }
        }

        public static void LogException(Exception e)
        {
            
            MessageBox.Show(e.Message);
        }
    }
}

