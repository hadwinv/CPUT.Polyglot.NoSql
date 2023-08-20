using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator.Executors
{
    public class QueryDirective
    {
        public Command Command { get; set; }

        public string Executable { get; set; }

        public Codex Codex { get; set; }
    }
}
