using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Structures
{
    internal enum TransitionType : byte
    {
        ReturnAnimation = 0,
        ExitBranches = 1,
        None = 2
    }

    internal struct AnimationInfo // uint
    {
        public string Name;
        public TransitionType TransitionType;
        public string ReturnAnimation;
        public FrameInfo[] Frames;

        public readonly FrameInfo this[int index] => Frames[index];
        public readonly int Length => Frames.Length;

        public AnimationInfo(string animationName, TransitionType transitionType, string returnAnimation, FrameInfo[] animationFrames)
        {
            Name = animationName;
            TransitionType = transitionType;
            ReturnAnimation = returnAnimation;
            Frames = animationFrames;
        }
    }
}
