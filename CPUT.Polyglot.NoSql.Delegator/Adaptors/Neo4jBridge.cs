using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using Neo4j.Driver;

namespace CPUT.Polyglot.NoSql.Delegator.Adaptors
{
    public class Neo4jBridge : INeo4jBridge
    {
        private IDriver _connection;

        public Neo4jBridge() { }

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