
using CPUT.Polyglot.NoSql.Models;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Logic
{
    public interface IServiceLogic
    {

        void GenerateData();

        void DataLoad();

        List<Result> Query(string statement);
    }
}
