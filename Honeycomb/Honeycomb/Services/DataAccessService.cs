using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace Honeycomb.Services
{
    public class SeedModel
    {
        public interface IDataAccessService
        {
            ObservableCollection<Seed> GetSeeds();
            int CreateSeed(Seed newSeed);

            void DeleteSeed(Seed selectedSeed);
        }

        public class DataAccessService : IDataAccessService
        {


            Crawler_DBEntities context;
            public DataAccessService()
            {

                context = new Crawler_DBEntities();
                
            }
            public ObservableCollection<Seed> GetSeeds()
            {
                context.Seeds.Load();
                return context.Seeds.Local;
            }

            public int CreateSeed(Seed newSeed)
            {
                context.Seeds.Add(newSeed);
                return context.SaveChanges();
            }

            public void DeleteSeed(Seed selectedSeed)
            {
                context.Seeds.Remove(selectedSeed);
                context.SaveChanges();
            }

            public static CrawlerConnection ConvertToCrawlerConnection(ClientCrawlerInfo connection)
            {
                var result = new CrawlerConnection();

                result.CrawlerIP = connection.ServerIP;
                result.CrawlerName = connection.ClientName;
                result.Id = connection.ClientIdentifier;

                return result;
            }

        }

    }
}
