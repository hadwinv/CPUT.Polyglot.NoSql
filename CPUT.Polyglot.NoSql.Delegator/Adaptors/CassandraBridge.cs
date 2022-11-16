using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Delegator;

namespace CPUT.Polyglot.NoSql.Delegator.Adaptors
{
    public class CassandraBridge : ICassandraBridge
    {
        private ISession _connection;
        //private ICluster _cluster;

        public CassandraBridge()
        {
            //ISession connection, ICluster cluster
            //_connection = connection;
            //_cluster = cluster;
        }

//​
//            var keyspaceNames = session
//                .Execute("SELECT * FROM system_schema.keyspaces")
//                .Select(row => row.GetValue<string>("keyspace_name"));
//​
//            Console.WriteLine("Found keyspaces:");
//            foreach (var name in keyspaceNames)
//            {
//                Console.WriteLine("- {0}", name);
//            }

    
        public ISession Connect()
        {
            if(_connection == null)
            {
                var cluster = Cluster.Builder()
                .AddContactPoints("127.0.0.1")
                                   //.WithPort(9042)
                                   .Build();

                _connection = cluster.Connect();
            }

            return _connection;
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
