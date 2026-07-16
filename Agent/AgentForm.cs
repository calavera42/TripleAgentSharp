using Agent.Structures;
using System.Media;

namespace Agent
{
    public partial class AgentForm : Form, IAgentDisplay
    {
        private SoundPlayer _player = new();
        private AgentFile _agentFile;

        private System.Windows.Forms.Timer _moveTimer;

        private PointF _moveDelta;
        private int _stepsTaken;
        private int _moveSteps;

        private FrameInfo _currentFrame;
        private OverlayInfo _mouth;

        private Bitmap? _drewnAgent;

        public AgentForm(AgentFile af)
        {
            InitializeComponent();
            _agentFile = af;

            _moveTimer = new();
            _moveTimer.Interval = 100;
            _moveTimer.Tick += MoveTick;

            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            Width = af.Width;
            Height = af.Height;
            TransparencyKey = af.TransparencyColor;

            Invalidated += AgentFormInvalidated;
        }

        private void AgentFormInvalidated(object? sender, InvalidateEventArgs e) => _drewnAgent = null;

        private void MoveTick(object? sender, EventArgs e)
        {
            if (_stepsTaken >= _moveSteps)
            {
                _moveTimer.Stop();
                return;
            }
            _stepsTaken++;
            Location = new((int)MathF.Round(_moveDelta.X * _stepsTaken), (int)MathF.Round(_moveDelta.Y * _stepsTaken));
        }

        public void PlayAudio(int index)
        {
            _player.Stream = _agentFile.ReadAudioStream((uint)index);
            _player.Play();
        }

        public new void Move(int x, int y)
        {
            const int stepSize = 10;
            int b = Math.Abs(x - Location.X);
            int c = Math.Abs(y - Location.Y);

            float steps = MathF.Round(MathF.Sqrt(b * b + c * c)) / stepSize;

            _moveDelta = new(b / steps, c / steps);
            _moveSteps = (int)Math.Round(steps);
            _stepsTaken = 0;

            _moveTimer.Start();
        }

        public void SetFrame(FrameInfo fi)
        {
            _currentFrame = fi;
            Invalidate();
        }

        public void SetMouth(OverlayInfo oi)
        {
            _mouth = oi;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if(_drewnAgent != null)
            {
                g.DrawImage(_drewnAgent, 0, 0);
                return;
            }

            bool mouthOverlay = _mouth.OverlayType != MouthOverlay.Closed;

            g.Clear(_agentFile.TransparencyColor);
            for (int i = _currentFrame.Layers.Length; i >= 0; i--) // é desenhado de trás pra frente
            {
                if (mouthOverlay && _mouth.ReplaceTopFrameImage && i == 0)
                    break;

                FrameImage fi = _currentFrame.Layers[i];
                using Bitmap layer = _agentFile.ReadImage(fi.ImageIndex);
                g.DrawImage(layer, fi.OffsetX, fi.OffsetY);
            }

            if(mouthOverlay)
            {
                using Bitmap overlay = _agentFile.ReadImage(_mouth.ImageIndex);
                g.DrawImage(overlay, _mouth.OffsetX, _mouth.OffsetY);
            }
        }

        public Rect GetRect() => new() { Top = Location.Y, Bottom = Location.Y + Height, Left = Location.X, Right = Location.X + Width };
    }
}
