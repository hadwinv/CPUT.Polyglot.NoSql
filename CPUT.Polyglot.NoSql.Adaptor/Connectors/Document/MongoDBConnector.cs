using CPUT.Polyglot.NoSql.Interface.Adaptors;
using MongoDB.Driver;
using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Adaptor.Connectors.Document
{
    public class MongoDBConnector : IMongoDBConnector
    {
        private IMongoDatabase _connection;
        private IMongoClient _client;

        public MongoDBConnector() { }

        public IMongoDatabase Connect()
        {
            if (_connection == null)
            {
                _client = new MongoClient("mongodb://127.0.0.1:27017");
                _connection = _client.GetDatabase("student-db");
            }

            return _connection;
        }

        public void Disconnect()
        {
            if (_client != null)
                _client.Cluster.Dispose();

            if (_connection != null && _connection.Client != null)
                _connection.Client.Cluster.Dispose();
        }
    }
}
