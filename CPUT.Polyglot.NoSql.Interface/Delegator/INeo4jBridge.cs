using Neo4j.Driver;

namespace CPUT.Polyglot.NoSql.Interface.Delegator
{
    public interface INeo4jBridge
    {
        IDriver Connect();
        void Disconnect();
    }
}
