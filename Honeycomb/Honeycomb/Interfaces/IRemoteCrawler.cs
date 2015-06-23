using System.ServiceModel;

namespace Honeycomb
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICrawlerClientCallback))]
    interface IRemoteCrawler
    {
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ReturnIntermediateResults(string msg);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ReturnCrawlingResults(Shared.CrawlerResults results);

        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerInfo);

        //[OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        //ClientCrawlerInfo[] WorkaroundMethod(Shared.BadLink bl,Shared.InternalLink il, Shared.ExternalLink el, Shared.Seed sd);


        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)]
        void Leave();
    }
}
