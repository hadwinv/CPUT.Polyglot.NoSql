using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Adaptors;

namespace CPUT.Polyglot.NoSql.Adaptor.Connectors
{
    public class CassandraConnector : ICassandraConnector
    {
        //private ISession _connection;
        //private ICluster _cluster;

        public CassandraConnector()
        {
            //ISession connection, ICluster cluster
            //_connection = connection;
            //_cluster = cluster;
        }

        public ISession Connect()
        {
            //if (_connection == null)
            //{
            //    _cluster = Cluster.Builder()
            //        .AddContactPoints("127.0.0.1")
            //        .Build();

            //    _connection = _cluster.Connect();
            //}

            var _cluster = Cluster.Builder()
                .AddContactPoints("127.0.0.1")
                .Build();

            var _connection = _cluster.Connect();

            return _connection;
        }

        public void Disconnect()
        {
            //if (_cluster != null)
            //    _cluster.Dispose();

            //if (_connection != null)
            //    _connection.Dispose();
        }
    }
}
