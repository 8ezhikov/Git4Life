﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Honeycomb.Shared
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CrawlerResultsDTO", Namespace="http://schemas.datacontract.org/2004/07/Honeycomb.Shared")]
    public partial class CrawlerResultsDTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private Honeycomb.Shared.BadLinkDTO[] BadLinksListField;
        
        private Honeycomb.Shared.ExternalLinkDTO[] ExternalLinksListField;
        
        private Honeycomb.Shared.InternalLinkDTO[] InternalLinksListField;
        
        private System.Collections.Generic.StackOfInternalLinkDTOATpcs25C InternalUnprocessedLinksField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Honeycomb.Shared.BadLinkDTO[] BadLinksList
        {
            get
            {
                return this.BadLinksListField;
            }
            set
            {
                this.BadLinksListField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Honeycomb.Shared.ExternalLinkDTO[] ExternalLinksList
        {
            get
            {
                return this.ExternalLinksListField;
            }
            set
            {
                this.ExternalLinksListField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Honeycomb.Shared.InternalLinkDTO[] InternalLinksList
        {
            get
            {
                return this.InternalLinksListField;
            }
            set
            {
                this.InternalLinksListField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.StackOfInternalLinkDTOATpcs25C InternalUnprocessedLinks
        {
            get
            {
                return this.InternalUnprocessedLinksField;
            }
            set
            {
                this.InternalUnprocessedLinksField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BadLinkDTO", Namespace="http://schemas.datacontract.org/2004/07/Honeycomb.Shared")]
    public partial class BadLinkDTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string LinkPathField;
        
        private string OriginalPageLinkField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LinkPath
        {
            get
            {
                return this.LinkPathField;
            }
            set
            {
                this.LinkPathField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OriginalPageLink
        {
            get
            {
                return this.OriginalPageLinkField;
            }
            set
            {
                this.OriginalPageLinkField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExternalLinkDTO", Namespace="http://schemas.datacontract.org/2004/07/Honeycomb.Shared")]
    public partial class ExternalLinkDTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string LinkAnchorField;
        
        private int LinkCountField;
        
        private string LinkPathField;
        
        private int LinkWeightField;
        
        private int OriginalPageLevelField;
        
        private string OriginalPageLinkField;
        
        private string PageSeedLinkField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LinkAnchor
        {
            get
            {
                return this.LinkAnchorField;
            }
            set
            {
                this.LinkAnchorField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int LinkCount
        {
            get
            {
                return this.LinkCountField;
            }
            set
            {
                this.LinkCountField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LinkPath
        {
            get
            {
                return this.LinkPathField;
            }
            set
            {
                this.LinkPathField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int LinkWeight
        {
            get
            {
                return this.LinkWeightField;
            }
            set
            {
                this.LinkWeightField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int OriginalPageLevel
        {
            get
            {
                return this.OriginalPageLevelField;
            }
            set
            {
                this.OriginalPageLevelField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OriginalPageLink
        {
            get
            {
                return this.OriginalPageLinkField;
            }
            set
            {
                this.OriginalPageLinkField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PageSeedLink
        {
            get
            {
                return this.PageSeedLinkField;
            }
            set
            {
                this.PageSeedLinkField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="InternalLinkDTO", Namespace="http://schemas.datacontract.org/2004/07/Honeycomb.Shared")]
    public partial class InternalLinkDTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private bool IsHtmlField;
        
        private bool IsProcessedField;
        
        private int LinkCountField;
        
        private string LinkPathField;
        
        private string OriginalPageLinkField;
        
        private int PageIdSeedSpecificField;
        
        private int PageLevelField;
        
        private string PageLinkField;
        
        private string PageSeedLinkField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsHtml
        {
            get
            {
                return this.IsHtmlField;
            }
            set
            {
                this.IsHtmlField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsProcessed
        {
            get
            {
                return this.IsProcessedField;
            }
            set
            {
                this.IsProcessedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int LinkCount
        {
            get
            {
                return this.LinkCountField;
            }
            set
            {
                this.LinkCountField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LinkPath
        {
            get
            {
                return this.LinkPathField;
            }
            set
            {
                this.LinkPathField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OriginalPageLink
        {
            get
            {
                return this.OriginalPageLinkField;
            }
            set
            {
                this.OriginalPageLinkField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int PageIdSeedSpecific
        {
            get
            {
                return this.PageIdSeedSpecificField;
            }
            set
            {
                this.PageIdSeedSpecificField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int PageLevel
        {
            get
            {
                return this.PageLevelField;
            }
            set
            {
                this.PageLevelField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PageLink
        {
            get
            {
                return this.PageLinkField;
            }
            set
            {
                this.PageLinkField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PageSeedLink
        {
            get
            {
                return this.PageSeedLinkField;
            }
            set
            {
                this.PageSeedLinkField = value;
            }
        }
    }
}
namespace System.Collections.Generic
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="StackOfInternalLinkDTOATpcs25C", Namespace="http://schemas.datacontract.org/2004/07/System.Collections.Generic")]
    public partial class StackOfInternalLinkDTOATpcs25C : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private Honeycomb.Shared.InternalLinkDTO[] _arrayField;
        
        private int _sizeField;
        
        private int _versionField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public Honeycomb.Shared.InternalLinkDTO[] _array
        {
            get
            {
                return this._arrayField;
            }
            set
            {
                this._arrayField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int _size
        {
            get
            {
                return this._sizeField;
            }
            set
            {
                this._sizeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int _version
        {
            get
            {
                return this._versionField;
            }
            set
            {
                this._versionField = value;
            }
        }
    }
}
namespace Honeycomb
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClientCrawlerInfo", Namespace="http://schemas.datacontract.org/2004/07/Honeycomb")]
    public partial class ClientCrawlerInfo : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ClientNameField;
        
        private string ServerIPField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ClientName
        {
            get
            {
                return this.ClientNameField;
            }
            set
            {
                this.ClientNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ServerIP
        {
            get
            {
                return this.ServerIPField;
            }
            set
            {
                this.ServerIPField = value;
            }
        }
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="IRemoteCrawler", CallbackContract=typeof(IRemoteCrawlerCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
public interface IRemoteCrawler
{
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsInitiating=false, Action="http://tempuri.org/IRemoteCrawler/ReturnIntermediateResults")]
    void ReturnIntermediateResults(string msg);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsInitiating=false, Action="http://tempuri.org/IRemoteCrawler/ReturnIntermediateResults")]
    System.Threading.Tasks.Task ReturnIntermediateResultsAsync(string msg);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsInitiating=false, Action="http://tempuri.org/IRemoteCrawler/ReturnCrawlingResults")]
    void ReturnCrawlingResults(Honeycomb.Shared.CrawlerResultsDTO resultsDto);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsInitiating=false, Action="http://tempuri.org/IRemoteCrawler/ReturnCrawlingResults")]
    System.Threading.Tasks.Task ReturnCrawlingResultsAsync(Honeycomb.Shared.CrawlerResultsDTO resultsDto);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRemoteCrawler/Join", ReplyAction="http://tempuri.org/IRemoteCrawler/JoinResponse")]
    Honeycomb.ClientCrawlerInfo[] Join(Honeycomb.ClientCrawlerInfo clientCrawlerInfo);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRemoteCrawler/Join", ReplyAction="http://tempuri.org/IRemoteCrawler/JoinResponse")]
    System.Threading.Tasks.Task<Honeycomb.ClientCrawlerInfo[]> JoinAsync(Honeycomb.ClientCrawlerInfo clientCrawlerInfo);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsTerminating=true, IsInitiating=false, Action="http://tempuri.org/IRemoteCrawler/Leave")]
    void Leave();
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsTerminating=true, IsInitiating=false, Action="http://tempuri.org/IRemoteCrawler/Leave")]
    System.Threading.Tasks.Task LeaveAsync();
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IRemoteCrawlerCallback
{
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IRemoteCrawler/Receive")]
    void Receive(Honeycomb.ClientCrawlerInfo sender, string message);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IRemoteCrawler/StartCrawling")]
    void StartCrawling(string siteURL);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IRemoteCrawlerChannel : IRemoteCrawler, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class RemoteCrawlerClient : System.ServiceModel.DuplexClientBase<IRemoteCrawler>, IRemoteCrawler
{
    
    public RemoteCrawlerClient(System.ServiceModel.InstanceContext callbackInstance) : 
            base(callbackInstance)
    {
    }
    
    public RemoteCrawlerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
            base(callbackInstance, endpointConfigurationName)
    {
    }
    
    public RemoteCrawlerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }
    
    public RemoteCrawlerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }
    
    public RemoteCrawlerClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(callbackInstance, binding, remoteAddress)
    {
    }
    
    public void ReturnIntermediateResults(string msg)
    {
        base.Channel.ReturnIntermediateResults(msg);
    }
    
    public System.Threading.Tasks.Task ReturnIntermediateResultsAsync(string msg)
    {
        return base.Channel.ReturnIntermediateResultsAsync(msg);
    }
    
    public void ReturnCrawlingResults(Honeycomb.Shared.CrawlerResultsDTO resultsDto)
    {
        base.Channel.ReturnCrawlingResults(resultsDto);
    }
    
    public System.Threading.Tasks.Task ReturnCrawlingResultsAsync(Honeycomb.Shared.CrawlerResultsDTO resultsDto)
    {
        return base.Channel.ReturnCrawlingResultsAsync(resultsDto);
    }
    
    public Honeycomb.ClientCrawlerInfo[] Join(Honeycomb.ClientCrawlerInfo clientCrawlerInfo)
    {
        return base.Channel.Join(clientCrawlerInfo);
    }
    
    public System.Threading.Tasks.Task<Honeycomb.ClientCrawlerInfo[]> JoinAsync(Honeycomb.ClientCrawlerInfo clientCrawlerInfo)
    {
        return base.Channel.JoinAsync(clientCrawlerInfo);
    }
    
    public void Leave()
    {
        base.Channel.Leave();
    }
    
    public System.Threading.Tasks.Task LeaveAsync()
    {
        return base.Channel.LeaveAsync();
    }
}
