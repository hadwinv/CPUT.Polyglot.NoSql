using Cassandra;

namespace CPUT.Polyglot.NoSql.Interface.Delegator
{
    public interface ICassandraBridge
    {
        ISession Connect();

        void Disconnect();
    }
}
