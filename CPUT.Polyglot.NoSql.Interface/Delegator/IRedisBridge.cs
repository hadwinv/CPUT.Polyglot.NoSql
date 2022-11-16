using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Interface.Delegator
{
    public interface IRedisBridge
    {
        IDatabase Connect();

        void Flush();

        void Disconnect();
    }
}
