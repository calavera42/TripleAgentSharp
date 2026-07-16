using Agent.Structures;
using System.Globalization;

namespace Agent
{
    public class AgentFile
    {
        public int Width => _charInfo.Width;
        public int Height => _charInfo.Height;
        public LocalizedInfo LocalizedInfo => GetLocalizedInfo((ushort)CultureInfo.CurrentCulture.LCID);

        public Color TransparencyColor => _charInfo.TransparentColor;
        public Icon? TrayIcon => _charInfo.TrayIcon;

        private readonly ACSReader _reader;
        private CharacterInfo _charInfo;

        public AgentFile(Stream s)
        {
            _reader = new(s);
            _charInfo = _reader.ReadCharInfo();
        }

        public LocalizedInfo GetLocalizedInfo(ushort langId)
        {
            foreach (LocalizedInfo li in _charInfo.LocalizedInfo)
                if (li.LangID == langId)
                    return li;

            return new();
        }

        public string[] GetAnimationsNames() => _reader.GetAnimationNames();
        public string[] GetStateAnimations(string name) => _charInfo.StateInfo[name.ToLowerInvariant()];

        public Bitmap ReadImage(uint index) => _reader.ReadImage(index);
        public MemoryStream ReadAudioStream(uint index) => _reader.ReadAudioStream(index);
        public AnimationInfo ReadAnimation(string name) => _reader.ReadAnimation(name);
    }
}
