using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator
{
    public class Enquiry
    {
        public int Database { get; set; }

        public Command Command { get; set; }

        public BaseExpr BaseExpr { get; set; }

        public List<MappedSource> Mapper { get; set; }
    }
}
