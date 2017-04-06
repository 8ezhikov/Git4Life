using System.Collections.Generic;
using System.ServiceModel;

namespace Honeycomb.Interfaces
{
    public interface ICrawlerClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void StartTestCrawl();

        [OperationContract(IsOneWay = true)]
        void StartCrawling(List<SeedDTO> SelectedSeedDTO, int maxLevel);
    }
}
