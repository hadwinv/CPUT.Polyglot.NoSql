using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator.Parts
{
    public class CreatePart
    {
        public BaseExpr BaseExpr { get; set; }
        
        public Command Command { get; set;  }
        
        public List<USchema> USchema { get; set; }

        public List<NSchema> NSchema { get; set; }
    }
}
