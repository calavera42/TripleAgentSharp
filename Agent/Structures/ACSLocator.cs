using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct ACSLocator
    {
        public uint Offset;
        public uint Size;

        public static uint SizeOf => sizeof(uint) * 2;
    }
}
