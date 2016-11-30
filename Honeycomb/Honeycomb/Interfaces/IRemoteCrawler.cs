using System.ServiceModel;
using Honeycomb.Interfaces;
using Honeycomb.Models;

namespace Honeycomb
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICrawlerClientCallback))]
    interface IRemoteCrawler
    {
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ReturnIntermediateResults(string msg);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        void ReturnCrawlingResults(CrawlerResultsDTO resultsDto);

        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerInfo);

        //[OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        //ClientCrawlerInfo[] WorkaroundMethod(Shared.BadLinkDTO bl,Shared.InternalLinkDTO il, Shared.ExternalLinkDTO el, Shared.SeedDTO sd);


        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)]
        void Leave();
    }
}
