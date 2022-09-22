using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Adaptors;

namespace CPUT.Polyglot.NoSql.Adaptor.Connectors.Columnar
{
    public class CassandraConnector : ICassandraConnector
    {
        private ISession _connection;
        private Cluster _cluster;

        public CassandraConnector()
        {
            //cassandra
            _cluster = Cluster.Builder()
                .AddContactPoints("127.0.0.1")
                .Build();
        }

        public ISession Connect()
        {
            if(_connection == null)
                _connection = _cluster.Connect();

            return _connection;
        }

        public void Disconnect()
        {
            if (_cluster != null)
                _cluster.Dispose();

            if (_connection != null)
                _connection.Dispose();
        }
    }
}
