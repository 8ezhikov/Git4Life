using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Honeycomb
{
    [DataContract]
    public class CrawlerResultsDTO
    {
    
        [DataMember]
        public BatchDTO BatchInfo;
        [DataMember]
        public ConnectionInfoDTO ConnectionInfo;
    }

    [DataContract]
    public class BatchDTO
    {
        [DataMember]
        public Guid CrawlerConnectionId;
        [DataMember]
        public int CrawlingTime;
        [DataMember]
        public DateTime StartTime;
        [DataMember]
        public int NumberOfCrawledInternalLinks;
        [DataMember]
        public int NumberOfCrawledExternalLinks;
        [DataMember]
        public int Id;
        [DataMember]
        public DateTime EndTime;


        [DataMember]

        public List<SiteResults> resultCollection;


    }

    [DataContract]
    public class SiteResults
    {
        [DataMember]
        public List<InternalLinkDTO> InternalLinksList;
        [DataMember]
        public List<ExternalLinkDTO> ExternalLinksList;
        [DataMember]
        public List<BadLinkDTO> BadLinksList;
        [DataMember]
        public Stack<InternalLinkDTO> InternalUnprocessedLinks;
        [DataMember]
        public SeedDTO ProcessedSeed;
        [DataMember]
        public TimeSpan SiteCrawlingTime;
    }

    [DataContract]
    public class ConnectionInfoDTO
    {
        [DataMember]
        public Guid Id;
        [DataMember]
        public string CrawlerName;
        [DataMember]
        public DateTime ConnectionTime;
        [DataMember]
        public string CrawlerIP;
    }


    [DataContract]
    public class SeedDTO
    {
        [DataMember]
        public string SeedDomainName;

        [DataMember]
        public int SeedIndex;
    }
    [DataContract]
    public class InternalLinkDTO
    {
        [DataMember]
        public string PageSeedLink;
        [DataMember]
        public int PageIdSeedSpecific;
        [DataMember]
        public bool IsProcessed;
        [DataMember]
        public int PageLevel;
        [DataMember]
        public string PageLink;
        [DataMember]
        public string LinkPath;
        [DataMember]
        public bool IsHtml;
        [DataMember]
        public string OriginalPageLink;
        [DataMember]
        public int LinkCount;

    }

    [DataContract]
    public class ExternalLinkDTO
    {
        [DataMember]
        public string LinkAnchor;
        [DataMember]
        public string LinkPath;
        [DataMember]
        public int LinkWeight = 1;
        [DataMember]
        public string OriginalPageLink;
        [DataMember]
        public string PageSeedLink;
        [DataMember]
        public int OriginalPageLevel;
        [DataMember]
        public int LinkCount;
    }

    [DataContract]
    public class BadLinkDTO
    {
        [DataMember]
        public string LinkPath;
        [DataMember]
        public string OriginalPageLink;
    }
}
