using App.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CPUT.Polyglot.NoSql.Common.Reporting
{
    public class MemoryUsage
    {
        private static Process _process;

        public long _physical { get; private set; }

        public long _virtual { get; private set; }

        public double TotalPhysical { get; private set; }

        public double TotalVirtual { get; private set; }

        public void CallMemory()
        {
            TotalPhysical = _process.WorkingSet64 - _physical;

            TotalVirtual = _process.VirtualMemorySize64 - _virtual;
        }

        public void Start()
        {
            _process = Process.GetCurrentProcess();
            _physical = _process.WorkingSet64;
            _virtual = _process.VirtualMemorySize64;
        }
    }
}
