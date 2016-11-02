using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Honeycomb.Shared
{
    [DataContract]
    public class CrawlerResultsDTO
    {
        [DataMember]
        public List<InternalLinkDTO> InternalLinksList;
        [DataMember]
        public List<ExternalLinkDTO> ExternalLinksList;
        [DataMember]
        public List<BadLinkDTO> BadLinksList;
        [DataMember]
        public Stack<InternalLinkDTO> InternalUnprocessedLinks;

        [DataMember] public SeedDTO ProcessedSeed;
    }


    [DataContract]
    public class SeedDTO
    {
        [DataMember]
        public string SeedDomainName;
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
