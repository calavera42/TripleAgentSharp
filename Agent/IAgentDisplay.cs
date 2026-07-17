using Agent.Structures;

namespace Agent
{
    internal interface IAgentDisplay
    {
        void PlayAudio(int index);
        void SetFrame(FrameInfo fi);
        void SetMouth(OverlayInfo oi);
        void Move(int x, int y);

        void Show();
        void Hide();

        bool IsReady();

        Rect GetRect();
    }
}
