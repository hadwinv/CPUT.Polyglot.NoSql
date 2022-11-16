using CPUT.Polyglot.NoSql.Models.Mapper;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Mapper
{
    public interface ISchema
    {
        List<MappedSource> Mapper();

        List<USchema> Global();

        List<NSchema> KeyValue();

        List<NSchema> Columnar();

        List<NSchema> Document();

        List<NSchema> Graph();
    }
}
