using CPUT.Polyglot.NoSql.Interface.Delegator;
using MongoDB.Driver;

namespace CPUT.Polyglot.NoSql.Delegator.Adaptors
{
    public class MongoDbBridge : IMongoDBBridge
    {
        private IMongoDatabase _connection;
        private IMongoClient _client;

        public MongoDbBridge() { }

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
