using Neo4j.Driver;

namespace CPUT.Polyglot.NoSql.Interface.Adaptors
{
    public interface INeo4jConnector
    {
        IDriver Connect();
        void Disconnect();
    }
}
