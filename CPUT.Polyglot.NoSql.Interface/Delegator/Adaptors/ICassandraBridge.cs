using Cassandra;

namespace CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors
{
    public interface ICassandraBridge
    {
        ISession Connect();

        void Disconnect();
    }
}
