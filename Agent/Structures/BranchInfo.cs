using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct BranchInfo // byte
    {
        public ushort TargetFrameIndex;
        public ushort Probability;
    }
}
