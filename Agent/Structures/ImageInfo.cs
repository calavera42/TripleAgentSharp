using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    public struct ImageInfo
    {
        public byte Unknown;
        public ushort Width;
        public ushort Height;
        public bool Compressed;
        public Datablock ImageData;
        public RgnData RegionData;
    }
}
