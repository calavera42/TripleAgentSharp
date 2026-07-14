using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct FrameInfo // ushort
    {
        public FrameImage[] Layers;
        public short AudioIndex;
        public ushort Duration;
        public short ExitFrameIndex;
        public BranchInfo[] Branches;
        public OverlayInfo[] MouthOverlays;
    }
}
