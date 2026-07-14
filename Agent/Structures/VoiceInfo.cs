using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal struct VoiceInfo
    {
        public Guid TTSEngineID;
        public Guid TTSModeID;
        public uint Speed;
        public ushort Pitch;
        public bool ExtraData;

        public ushort LangID;
        public string LanguageDialect;
        public ushort Gender;
        public ushort Age;
        public string Style;
    }
}
