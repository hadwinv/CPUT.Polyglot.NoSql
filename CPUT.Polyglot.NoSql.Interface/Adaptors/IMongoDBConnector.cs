using MongoDB.Driver;
using System.Data.Common;

namespace CPUT.Polyglot.NoSql.Interface.Adaptors
{
    public interface IMongoDBConnector
    {
        IMongoDatabase Connect();

        void Disconnect();
    }
}
