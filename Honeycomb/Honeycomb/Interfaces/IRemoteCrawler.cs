using System.ServiceModel;

namespace Honeycomb
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICrawlerClientCallback))]
    interface IRemoteCrawler
    {
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ReturnIntermediateResults(string msg);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ReturnCrawlingResults(string to, string msg);

        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerInfo);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)]
        void Leave();
    }
}
