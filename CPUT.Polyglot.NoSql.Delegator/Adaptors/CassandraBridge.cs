using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;

namespace CPUT.Polyglot.NoSql.Delegator.Adaptors
{
    public class CassandraBridge : ICassandraBridge
    {
        private ISession _connection;

        public CassandraBridge() { }

        public ISession Connect()
        {
            if(_connection == null)
            {
                var cluster = Cluster.Builder().WithDefaultKeyspace("cput").AddContactPoints("127.0.0.1").Build();

                _connection = cluster.Connect();
            }

            return _connection;
        }

        public void Disconnect()
        {
            //if (_connection != null)
            //{
            //    _connection.Dispose();
            //    _connection = 
            //}
                
        }
    }
}
