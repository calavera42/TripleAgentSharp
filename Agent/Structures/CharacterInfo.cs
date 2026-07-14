using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct CharacterInfo
    {
        public ushort MinorVersion;
        public ushort MajorVersion;
        public LocalizedInfo[] LocalizedInfo;
        public Guid GUID;
        public ushort Width;
        public ushort Height;
        public Color TransparentColor;
        public uint Flags;
        public ushort AnimationMajorVersion;
        public ushort AnimationMinorVersion;
        public VoiceInfo VoiceInfo;
        public BalloonInfo BalloonInfo;
        public ColorPalette Palette;
        public bool SystemTrayIcon;
        public Dictionary<string, string[]> StateInfo;
    }
}
