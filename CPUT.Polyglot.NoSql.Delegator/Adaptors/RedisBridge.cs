using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Delegator.Adaptors
{
    public class RedisBridge : IRedisBridge
    {

        private IConnectionMultiplexer _connection;
        private IDatabase _database;

        private int _index { get; set; }

        public RedisBridge() 
        {
            _index = 1;
        }

        public IDatabase Connect()
        {
            if (_connection == null)
                _connection = ConnectionMultiplexer.Connect("127.0.0.1:6379,allowAdmin=true");
            
                _database = _connection.GetDatabase(_index);

            return _database;
        }

        public void Flush()
        {
            _connection.GetServer("127.0.0.1:6379").FlushDatabase(_index);
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
