using System.ServiceModel;

namespace Honeycomb
{
    public interface ICrawlerClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void Receive(ClientCrawlerInfo sender, string message);

        [OperationContract(IsOneWay = true)]
        void StartCrawling(string siteURL);

    }
}
