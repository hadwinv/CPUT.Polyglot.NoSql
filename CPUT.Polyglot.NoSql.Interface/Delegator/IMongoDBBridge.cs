using MongoDB.Driver;

namespace CPUT.Polyglot.NoSql.Interface.Delegator
{
    public interface IMongoDBBridge
    {
        IMongoDatabase Connect();

        void Disconnect();
    }
}
