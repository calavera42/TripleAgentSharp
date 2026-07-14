using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct OverlayInfo // byte
    {
        public MouthOverlay OverlayType;
        public bool ReplaceTopFrameImage;
        public ushort ImageIndex;
        public byte Unknown;
        public bool HasRegionData;
        public short OffsetX;
        public short OffsetY;
        public ushort Width;
        public ushort Height;
        public RgnData RegionData; // ler primeiro um ulong que representa o tamanho.
    }
}
