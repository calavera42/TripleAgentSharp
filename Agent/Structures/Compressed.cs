using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    public struct Compressed
    {
        public bool IsCompressed => CompressedSize != UncompressedSize;

        public uint CompressedSize;
        public uint UncompressedSize;
        public byte[] Data;
    }
}
