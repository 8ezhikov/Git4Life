using System.Collections.ObjectModel;

namespace Honeycomb.Services
{
    public class SeedModel
    {
        public interface IDataAccessService
        {
            ObservableCollection<Seed> GetSeeds();
            int CreateSeed(Seed newSeed);
        }

        public class DataAccessService : IDataAccessService
        {
            CrawlerEntities context;
            public DataAccessService()
            {
                context = new CrawlerEntities();
            }
            public ObservableCollection<Seed> GetSeeds()
            {
                return context.Seeds.Local;
            }

            public int CreateSeed(Seed newSeed)
            {
                context.Seeds.Add(newSeed);
                return context.SaveChanges();
            }

        }

    }
}
