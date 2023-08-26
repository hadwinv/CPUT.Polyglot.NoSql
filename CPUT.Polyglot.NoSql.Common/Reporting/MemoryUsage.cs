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

        private long _physical { get; set; }

        private long _virtual { get; set; }

        public double TotalPhysicalMemory { get; private set; }

        public double TotalVirtualMemory { get; private set; }

        public void VirtualMemoryUsage()
        {
            TotalVirtualMemory = _process.VirtualMemorySize64 - _virtual;
        }

        public void PhysicalMemoryUsage()
        {
            TotalPhysicalMemory = _process.WorkingSet64 - _physical;
        }

        public void Start()
        {
            _process = Process.GetCurrentProcess();

            _physical = _process.WorkingSet64;
            _virtual = _process.VirtualMemorySize64;
        }
    }
}
