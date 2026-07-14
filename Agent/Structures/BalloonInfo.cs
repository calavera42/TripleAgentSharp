using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct BalloonInfo
    {
        public byte NumberTextLines;
        public byte CharsPerLine;
        public Color ForegroundColor;
        public Color BackgroundColor;
        public Color BorderColor;
        public string FontName;
        public int FontHeight;
        public int FontWeight;
        public bool Italic;
        public byte Reserved;

    }
}
