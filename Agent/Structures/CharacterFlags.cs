using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    static class CharacterFlags
    {
        public const uint VoiceDisabled = 1 << 4;
        public const uint VoiceEnabled = 1 << 5;
        public const uint BalloonDisabled = 1 << 8;
        public const uint BalloonEnabled = 1 << 9;
        public const uint BalloonSizeToTextEnabled = 1 << 15;
        public const uint BalloonAutoHide = 1 << 16;
        public const uint BalloonAutoPace = 1 << 17;
        public const uint StandardAnimationSetSupported = 1 << 19;
    }
}
