using System.ServiceModel;

namespace Honeycomb.Interfaces
{
    interface ICrawlerClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void Receive(ClientCrawlerInfo sender, string message);

        [OperationContract(IsOneWay = true)]
        void StartCrawling(ClientCrawlerInfo sender, string message);

    }
}
