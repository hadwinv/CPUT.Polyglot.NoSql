using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Models.Translator.Executors
{
    public class Model
    {
        public string Name { get; set;  }

        public Dictionary<FromProperty, ToProperty> Views { get; set; }
    }
}
