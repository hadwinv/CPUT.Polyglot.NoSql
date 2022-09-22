using Cassandra;

namespace CPUT.Polyglot.NoSql.Interface.Adaptors
{
    public interface ICassandraConnector
    {
        ISession Connect();

        void Disconnect();
    }
}
