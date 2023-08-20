using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors
{
    public interface IRedisBridge
    {
        IDatabase Connect();

        void Flush();

        void Disconnect();
    }
}
