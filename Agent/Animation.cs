using Agent.Structures;

namespace Agent
{
    internal class Animation
    {
        private AnimationInfo _animationInfo;
        private AnimationState _state = AnimationState.Running;
        private AnimationState _prevState = AnimationState.Running;

        private int _lastValidFrameIdx = 0;
        private int _currentFrameIdx = 0;

        private FrameInfo _currentFrame => _animationInfo.Frames[_currentFrameIdx];

        public FrameInfo Frame => _animationInfo.Frames[_lastValidFrameIdx];
        public AnimationInfo Info => _animationInfo;
        public AnimationState State => _state;

        public event EventHandler<(AnimationState prev, AnimationState current)>? StateChanged;

        public Animation(AnimationInfo ai) => _animationInfo = ai;

        private void AdvanceFrame(bool onlybranch = false)
        {
            int selection = (new Random()).Next(100);
            int count = 0;
            foreach(BranchInfo b in _currentFrame.Branches)
            {
                count += b.Probability;
                if (selection < count)
                {
                    _currentFrameIdx = b.TargetFrameIndex;
                    return;
                }
            }

            if (!onlybranch)
                _currentFrameIdx++;
        }

        public void Update()
        {
            if (_currentFrame.Duration != 0)
                _lastValidFrameIdx = _currentFrameIdx;

            if (_currentFrame.ExitFrameIndex == -2)
            {
                ChangeState(AnimationState.Finished);
                _currentFrameIdx = _lastValidFrameIdx;
                return;
            }

            switch (_state)
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
        }

        private void ChangeState(AnimationState target)
        {
            _prevState = _state;
            _state = target;
            StateChanged?.Invoke(this, (_prevState, _state));
        }

        public void Exit() => ChangeState(AnimationState.Exiting);
    }
}
