using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Models.Translator.Executors
{
    public  class Codex
    {
        public Utils.Database Target { get; set; }

        public List<Model> PropertyModel { get; set; }

        public DataModelExpr? DataModel { get; set; }
    }
}
