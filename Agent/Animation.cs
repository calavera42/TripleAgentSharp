using Agent.Structures;

namespace Agent
{
    internal class Animation
    {
        private AnimationInfo _animationInfo;
        private AnimationState _state;

        private int _lastValidFrameIdx = 0;
        private int _currentFrameIdx = 0;

        private DateTime _lastFrameChange = DateTime.Now;

        private FrameInfo _currentFrame => _animationInfo.Frames[_currentFrameIdx];

        public FrameInfo Frame => _animationInfo.Frames[_lastValidFrameIdx];
        public AnimationState State => _state;

        public Animation(AnimationInfo ai) => _animationInfo = ai;

        private void AdvanceFrame(bool onlybranch = false)
        {
            int selection = (new Random()).Next(100);
            int count = 0;
            foreach(BranchInfo b in _currentFrame.Branches)
            {
                count += b.TargetFrameIndex;
                if (selection < count)
                {
                    _currentFrameIdx = b.TargetFrameIndex;
                    return;
                }
            }

            if (!onlybranch)
                _currentFrameIdx++;
        }

        /// <summary>
        /// Chamar pelo menos uma vez a cada 10 ms
        /// </summary>
        public bool Update()
        {
            if ((DateTime.UtcNow - _lastFrameChange).TotalMilliseconds < _currentFrame.Duration * 10
                || _state == AnimationState.Finished)
                return false;

            bool output = false;

            switch(_state)
            {
                case AnimationState.Exiting:
                    int targetFrame = _currentFrame.ExitFrameIndex;
                    if (targetFrame != -1)
                        _currentFrameIdx = targetFrame;
                    break;
                case AnimationState.Running:
                    AdvanceFrame();
                    break;
            }

            if (_currentFrame.Duration != 0)
            {
                _lastValidFrameIdx = _currentFrameIdx;
                output = true;
            }

            _lastFrameChange = DateTime.UtcNow;
            return output;
        }

        public void Exit() => _state = AnimationState.Exiting;
    }
}
