using CPUT.Polyglot.NoSql.Models.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Models.Views
{
    public class LinkedProperty
    {
        public string Property { get; set; }
        
        public string AliasIdentifier { get; set; }

        public string AliasName { get; set; }

        public Link? Link { get; set; }

        public Type Type { get; set; }
    }
}
