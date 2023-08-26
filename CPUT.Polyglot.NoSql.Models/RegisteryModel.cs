using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Models
{
    public class RegisteryModel
    {
        public int No { get; set; }

        public string Command { get; set; }

        public string Description { get; set; }

        public int ExecutionTimes { get; set; }

        public string Target { get; set; }

        public string Script { get; set; }

        public bool Active { get; set; }
    }
}
