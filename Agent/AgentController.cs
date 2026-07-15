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

    internal interface IAgentDisplay
    {
        void PlayAudio(int index);
        void SetFrame(FrameInfo fi);
        void SetMouth(OverlayInfo oi);
        void Move(int x, int y);

        void Show();
        void Hide();

        Rect GetRect();
    }

    internal class AgentController
    {
        private Animation _animation;
        private AgentFile _agentFile;
        private IAgentDisplay _display;

        public AgentController(AgentFile af, IAgentDisplay iad)
        {
            _agentFile = af;
            _display = iad;
            _animation = new(GetAnimationFromState(AgentState.Showing));
        }

        private AnimationInfo GetAnimationFromState(AgentState state)
        {
            string stateName = GetStateName(state);
            string[] animations = _agentFile.GetStateAnimations(stateName);
            int i = Random.Shared.Next(0, animations.Length);
            return _agentFile.ReadAnimation(animations[i]);
        }

        private static string GetStateName(AgentState state)
        {
            string[] labels =
            {
                "Showing",
                "Hiding",
                "GesturingLeft",
                "GesturingRight",
                "GesturingUp",
                "GesturingDown",
                "Listening",
                "Hearing",
                "IdlingLevel1",
                "IdlingLevel2",
                "IdlingLevel3",
                "MovingLeft",
                "MovingRight",
                "MovingUp",
                "MovingDown",
                "Speaking"
            };
            return labels[(int)state];
        }
    }
}
