using MongoDB.Driver;

namespace CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors
{
    public interface IMongoDBBridge
    {
        IMongoDatabase Connect();

        void Disconnect();
    }
}
