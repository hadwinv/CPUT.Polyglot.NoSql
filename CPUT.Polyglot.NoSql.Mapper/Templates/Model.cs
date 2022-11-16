using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Mapper.Templates
{
    public class Model
    {
        public string Name { get; set; }

        public List<Properties> Properties { get; set; }

        public List<Relations> Relations { get; set; }

    }
}
