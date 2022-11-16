using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Adaptors;
using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Adaptor.Connectors
{
    public class RedisAdaptor : IRedisAdaptor
    {

        private IConnectionMultiplexer _connection;

        private int _index;

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        public RedisAdaptor() { }

        public IConnectionMultiplexer Connect()
        {
            if (_connection == null)
                _connection = ConnectionMultiplexer.Connect("127.0.0.1:6379,allowAdmin=true");

            return _connection;
        }

        public IServer? Server ()
        {
             if (_connection != null)
                    return _connection.GetServer("127.0.0.1:6379");

                return null;
        }

        public IDatabase? Database()
        {
            if (_connection != null)
                return _connection.GetDatabase(1);

            return null;
        }

        public void SetPartition(int index)
        {
            _index = index;
        }

        public int GetPartition()
        {
            return Index;
        }

        public void Flush(int index)
        {
            if(index > 0)
            {
                _connection.GetServer("127.0.0.1:6379").FlushDatabase(index);
            }
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
