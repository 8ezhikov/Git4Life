using System;
using System.Collections.Generic;
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
        private readonly HashSet<string> _allLinks = new HashSet<string>();
        private bool _forceStop;
        private int _internalLinksCounter = 1;
        private const string StartingPageName = "Starting Page";
        private readonly Dictionary<string,InternalLinkDTO> _internalLinksDictionary = new Dictionary<string, InternalLinkDTO>();
        private readonly Dictionary<string, ExternalLinkDTO> _externalLinksDictionary = new Dictionary<string, ExternalLinkDTO>();
        private readonly List<BadLinkDTO> _badLinksList = new List<BadLinkDTO>();
        private readonly Stack<InternalLinkDTO> _internalUnprocessedLinks = new Stack<InternalLinkDTO>();




        public CrawlerResultsDTO StartCrawlingProcess(string startingSeedName, int maxCrawlLevel= 5)
        {
            _maxPageLevel = maxCrawlLevel;
            _forceStop = false;
            var startingSeed = new SeedDTO { SeedDomainName = startingSeedName };
            var seedCollection = new List<SeedDTO> { startingSeed };
            foreach (var seed in seedCollection)
            {
                var startingAddress = seed.SeedDomainName;
              
                _internalLinksCounter = 1;
                if (startingAddress == string.Empty)
                {
                    continue;
                }
                FillAllLinks(startingAddress);

                if (_allLinks.Contains(startingAddress))
                {
                    AddInternalLink(startingAddress, startingAddress, 0, StartingPageName);
                }

                ScrapLinks(startingAddress, 0, startingAddress);
              
                while (_internalUnprocessedLinks.Count > 0 & _forceStop != true)
                {
                    var selectedLink = _internalUnprocessedLinks.Pop();

                    if (selectedLink != null)
                    {
                        selectedLink.IsProcessed = true;
                        ScrapLinks(selectedLink.PageLink, selectedLink.PageLevel, startingAddress);
                    }
                }

                _allLinks.Clear();
            }
            if (_forceStop)
            {
                _forceStop = false;
            }
        

            //RuningTime = (DateTime.Now - startingTime).ToString();
            //MessageBox.Show(RuningTime);

            var result = new CrawlerResultsDTO
            {
                BadLinksList = _badLinksList.ToList(),
                ExternalLinksList = _externalLinksDictionary.Select(pair => pair.Value).ToList(),
                InternalLinksList = _internalLinksDictionary.Select(pair => pair.Value).ToList()
            };

            return result;
        }
        void FillAllLinks(string seedDomainName)
        {
            var allCollectedInternalLinksForDomain =
                _internalLinksDictionary.Where(link => link.Value.PageSeedLink == seedDomainName);
            foreach (var internalLink in allCollectedInternalLinksForDomain)
            {
                _allLinks.Add(internalLink.Key);
            }

            var allCollectedExternalLinksForDomain =
               _internalLinksDictionary.Where(link => link.Value.PageSeedLink == seedDomainName);
            foreach (var internalLink in allCollectedExternalLinksForDomain)
            {
                _allLinks.Add(internalLink.Value.LinkPath);
            }
        }


        private void ScrapLinks(string startingAddress, int pageLevel, string seedLink)
        {
            try
            {
                if (_forceStop)
                    return;
                var currentPageLevel = pageLevel + 1;

                if (currentPageLevel >= _maxPageLevel)
                    return;

                var htmlWeb = new HtmlWeb();
                var startingAdressUri = new Uri(startingAddress);
                HtmlDocument doc;
                try
                {
                    doc = htmlWeb.Load(startingAddress);

                    if (doc.ParseErrors.Any(error => error.Code == HtmlParseErrorCode.CharsetMismatch))
                    {
                        doc = GetDocumentCustomMode(doc.Encoding, startingAddress);
                    }
                    //doc = BeeBot.Shared.Helpers.GetDocumentCustomMode(Encoding.UTF8, startingAddress);
                }
                catch (Exception)
                {
                    doc = GetDocumentCustomMode(Encoding.GetEncoding("windows-1251"), startingAddress);
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

                                if (_allLinks.Contains(fullLink))
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


                        if (_allLinks.Contains(frameLink))
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
            catch (Exception e)
            {
                LogException(e);
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
                catch (Exception e)
                {
                    LogException(e);
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
                            if (_allLinks.Contains(linkUrl))
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
                            if (_allLinks.Contains(linkUrl))
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
                                _externalLinksDictionary.Add(linkPage.LinkPath,linkPage);
                                _allLinks.Add(linkPage.LinkPath);
                            }
                            else
                            {
                                IncrementExternalLink(linkUrl);
                            }
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


        private void AddInternalLink(string linkUrl, string seedLink, int currentPageLevel, string originalPageLink)
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
                PageIdSeedSpecific = _internalLinksCounter


            };
            _internalLinksCounter++;
            _internalUnprocessedLinks.Push(linkPage);
            _internalLinksDictionary.Add(linkPage.PageLink,linkPage);
            _allLinks.Add(linkPage.PageLink);

        }

        private void IncrementInternalLink(string linkUrl)
        {
            _internalLinksDictionary[linkUrl].LinkCount++;
        }

        private void IncrementExternalLink(string linkUrl)
        {
            _externalLinksDictionary[linkUrl].LinkCount++;
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
            catch (Exception e)
            {
                LogException(e);
                return new HtmlDocument();
            }
        }

        public static void LogException(Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}

