using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Schema._data.prep;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos
{
    public class DataFactory : IDataFactory
    {
        
        private readonly IRedisRepo _redisjRepo;
        private readonly ICassandraRepo _cassandraRepo;
        private readonly IMongoRepo _mongoRepo;
        private readonly INeo4jRepo _neo4jRepo;

        private List<UDataset> _dataset;

        public DataFactory(IDataLoader dataLoader, IRedisRepo redisjRepo, ICassandraRepo cassandraRepo, IMongoRepo mongoRepo, INeo4jRepo neo4jRepo)
        {
            _redisjRepo = redisjRepo;
            _cassandraRepo = cassandraRepo;
            _mongoRepo = mongoRepo;
            _neo4jRepo = neo4jRepo;
            
            //load mock data
            _dataset = dataLoader.MockFullDataset();
        }

        public void LoadKeyValue()
        {
            _redisjRepo.CreateKeyValueDB(_dataset);
        }

        public void LoadColumnar()
        {
            _cassandraRepo.CreateColumnarDB(_dataset);
        }

        public void LoadDocument()
        {
            _mongoRepo.CreateDocumentDB(_dataset);
        }

        public void LoadGraph()
        {
            _neo4jRepo.CreateGraphDB(_dataset);
        }
    }
}
