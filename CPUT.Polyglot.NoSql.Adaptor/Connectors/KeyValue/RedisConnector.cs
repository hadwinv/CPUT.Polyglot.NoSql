using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Adaptors;
using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Adaptor.Connectors.KeyValue
{
    public class RedisConnector : IRedisConnector
    {
        private IConnectionMultiplexer _connection;

        public RedisConnector() { }

        public IConnectionMultiplexer Connect()
        {
            if (_connection == null)
                _connection = ConnectionMultiplexer.Connect("127.0.0.1:6379,allowAdmin=true");

            return _connection;
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
