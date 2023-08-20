using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator.Parts
{
    public class MetaPart
    {
        public Command Action { get; set; }

        public Dictionary<string, string> View { get; set; }
    }
}
