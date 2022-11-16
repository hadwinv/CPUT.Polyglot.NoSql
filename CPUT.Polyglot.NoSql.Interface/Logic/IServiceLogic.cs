
using CPUT.Polyglot.NoSql.Models;

namespace CPUT.Polyglot.NoSql.Interface.Logic
{
    public interface IServiceLogic
    {
        void DataLoad();

        Result Query(string statement);
    }
}
