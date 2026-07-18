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

    internal enum ControllerState
    {
        Idling,
        RestPose,
        SpeakPose,

    }

    internal class AgentController
    {
        private Animation _animation;
        private AgentFile _agentFile;
        private IAgentDisplay _display;

        private Thread _agentThread;
        private CancellationToken _cancellationToken;



        public AgentController(AgentFile af, IAgentDisplay iad)
        {
            _agentFile = af;
            _display = iad;
            _animation = new(GetAnimationFromState(AgentState.MovingLeft));

            _display.Show();

            Thread.Sleep(1000);

            _animation.StateChanged += AnimationStateChanged;

            _agentThread = new(Update);
            _agentThread.Start();
        }

        private void AnimationStateChanged(object? sender, (AnimationState prev, AnimationState current) e)
        {
            //if (e.current == AnimationState.Finished && 
            //    _animation.Info.TransitionType == TransitionType.ExitBranches && 
            //    e.prev == AnimationState.Running)
            //    _animation.Exit();
                
        }

        public void Update()
        {
            while(true)
            {
                FrameInfo e = _animation.Frame;
                if (_display.IsReady() && _animation.State != AnimationState.Finished)
                {
                    _display.SetFrame(e);
                    if (e.AudioIndex != -1)
                        _display.PlayAudio(e.AudioIndex);

                    _animation.Update();
                } 

                if (_cancellationToken.IsCancellationRequested) // TODO: tocar a animação de hide
                    break;
                Thread.Sleep(e.Duration * 9);
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
