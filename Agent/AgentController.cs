using Agent.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    internal enum AgentState
    {
        Showing,
        Hiding,
        GesturingLeft,
        GesturingRight,
        GesturingUp,
        GesturingDown,
        Listening,
        Hearing,
        IdlingLevel1,
        IdlingLevel2,
        IdlingLevel3,
        MovingLeft,
        MovingRight,
        MovingUp,
        MovingDown,
        Speaking
    }

    internal class AgentController
    {
        private Animation _animation;
        private AgentFile _agentFile;
        private IAgentDisplay _display;

        private System.Timers.Timer _timer;

        public AgentController(AgentFile af, IAgentDisplay iad)
        {
            _agentFile = af;
            _display = iad;
            _animation = new(GetAnimationFromState(AgentState.Showing));

            _timer = new();
            _timer.Interval = 10;
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if(_animation.Update())
            {
                FrameInfo fi = _animation.Frame;
                _display.SetFrame(fi);
            }
        }

        private AnimationInfo GetAnimationFromState(AgentState state)
        {
            string stateName = state.ToString();
            string[] animations = _agentFile.GetStateAnimations(stateName);
            int i = Random.Shared.Next(0, animations.Length);
            return _agentFile.ReadAnimation(animations[i]);
        }
    }
}
