
using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.Interface.Adaptors
{
    public interface IRedisConnector
    {
        IConnectionMultiplexer Connect();

        void Disconnect();
    }
}
