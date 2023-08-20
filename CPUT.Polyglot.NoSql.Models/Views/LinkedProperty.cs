using CPUT.Polyglot.NoSql.Models.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Models.Views
{
    public class LinkedProperty
    {
        public string Property { get; set; }
        
        public string AliasIdentifier { get; set; }

        public string AliasName { get; set; }

        public AggregateType? AggregateType { get; set; }

        public Link? Link { get; set; }

        public Type Type { get; set; }

        public string SourceReference { get; set; }


    }
}
