//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Honeycomb
{
    using System;
    using System.Collections.Generic;
    
    public partial class Batch
    {
        public int Id { get; set; }
        public System.Guid CrawlerConnectionId { get; set; }
        public System.DateTime StartTime { get; set; }
        public int CrawlingTime { get; set; }
        public int NumberOfCrawledInternalLinks { get; set; }
        public int NumberOfCrawledExternalLinks { get; set; }
        public string SeedIds { get; set; }
    
        public virtual CrawlerConnection CrawlerConnection { get; set; }
    }
}
