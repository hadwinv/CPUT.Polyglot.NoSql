using CPUT.Polyglot.NoSql.Models.Views;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Mapper
{
    public interface ISchema
    {
        List<USchema> UnifiedView();

        List<NSchema> KeyValue();

        List<NSchema> Columnar();

        List<NSchema> Document();

        List<NSchema> Graph();
    }
}
