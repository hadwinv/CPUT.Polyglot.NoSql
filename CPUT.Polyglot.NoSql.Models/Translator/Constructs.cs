using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator
{
    public class Constructs
    {
        public Database Target { get; set; }

        public OutputPart Result { get; set; }

        public string Message { get; set; }

        public bool Success { get; set; }
    }
}
