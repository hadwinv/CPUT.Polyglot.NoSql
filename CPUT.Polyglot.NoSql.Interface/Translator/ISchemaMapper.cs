using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Interface.Translator
{
    public interface ISchemaMapper
    {
        public Schemas Validate(BaseExpr baseExpr);

        List<MappedSource> SourceMappings();
    }
}
