using CPUT.Polyglot.NoSql.Interface.Adaptors;
using Neo4j.Driver;

namespace CPUT.Polyglot.NoSql.Adaptor.Connectors
{
    public class Neo4jConnector : INeo4jConnector
    {
        private IDriver _connection;

        public Neo4jConnector() { }

        public IDriver Connect()
        {
            if (_connection == null)
                _connection = GraphDatabase.Driver("neo4j://localhost:7687/enrollmentdb", AuthTokens.Basic("neo4j", "#H@dw1n_graph"));

            return _connection;
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection?.Dispose();
        }
    }
}