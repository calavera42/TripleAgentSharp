using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    public struct RgnData
    {
        public uint Size;
        public uint RegionType;
        public uint RectCount;
        public uint RectBufferSize;
        public Rect Region;

        public Rect[] Rects;
    }
}
