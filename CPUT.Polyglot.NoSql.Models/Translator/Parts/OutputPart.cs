using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Models.Translator.Parts
{
    public class OutputPart
    {
        public string Query { get; set; }

        public Codex Codex { get; set; }
    }
}
