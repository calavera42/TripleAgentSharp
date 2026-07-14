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

        public AgentController(AgentFile af)
        {
            _agentFile = af;
            _animation = new();
        }


        private AnimationInfo GetAnimationFromState(AgentState state)
        {
            string name = GetStateName(state);
            string stateName = GetStateName(state);

            string[] animations = _agentFile.GetStateAnimations(stateName);
            int chosenIndex = 

            return _agentFile.ReadAnimation(animations[])
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
