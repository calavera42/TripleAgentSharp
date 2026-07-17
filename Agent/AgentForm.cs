using Agent.Structures;
using System.Media;

namespace Agent
{
    public partial class AgentForm : Form, IAgentDisplay
    {
        private SoundPlayer _player = new();
        private AgentFile _agentFile;

        private System.Windows.Forms.Timer _moveTimer;

        private Point _target;
        private Point _start;
        private double _moveProgress;
        private double _progressIncrement;

        private FrameInfo? _currentFrame;
        private OverlayInfo _mouth;

        private readonly object _lockObject = new();

        private Bitmap? _drewnAgent;

        public int FlightDurationMs = 1500;

        public AgentForm(AgentFile af)
        {
            InitializeComponent();
            _agentFile = af;

            _moveTimer = new();
            _moveTimer.Interval = 20;
            _moveTimer.Tick += MoveTick;

            TopMost = true;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Width = af.Width;
            Height = af.Height;
            TransparencyKey = af.TransparencyColor;

            Invalidated += AgentFormInvalidated;
        }

        private async void AgentFormInvalidated(object? sender, InvalidateEventArgs e)
        {
            _drewnAgent = null;
            if(_currentFrame != null)
                PaintAgent();
        }

        private void MoveTick(object? sender, EventArgs e)
        {
            if (_moveProgress + float.Epsilon >= 1.0f)
            {
                _moveTimer.Stop();
                Location = _target;
                return;
            }
            _moveProgress += _progressIncrement;

            Point delta = new(_target.X - _start.X, _target.Y - _start.Y);

            Location = new(
                (int)Math.Round(_start.X + (_moveProgress * delta.X)), 
                (int)Math.Round(_start.Y + (_moveProgress * delta.Y))
            );
        }

        public void PlayAudio(int index)
        {
            _player.Stream = _agentFile.ReadAudioStream((uint)index);
            _player.Play();
        }

        public new void Move(int x, int y)
        {
            _moveProgress = 0;
            _progressIncrement = (1d / (FlightDurationMs - 1000)) * _moveTimer.Interval;
            _target = new(x, y);
            _start = Location;

            Invoke(_moveTimer.Start);
        }

        public void SetFrame(FrameInfo fi)
        {
            if (fi.Equals(_currentFrame))
                return;
            lock(_lockObject)
            {
                _currentFrame = fi;
            }
            Invoke(Invalidate);
        }

        public void SetMouth(OverlayInfo oi)
        {
            if (oi.Equals(_mouth))
                return;
            lock(_lockObject)
            {
                _mouth = oi;
            }
            Invoke(Invalidate);
        }

        public void PaintAgent()
        {
            if (_currentFrame == null)
                return;

            lock (_lockObject)
            {
                _drewnAgent = new(_agentFile.Width, _agentFile.Height);
                using Graphics g = Graphics.FromImage(_drewnAgent);

                bool mouthOverlay = _mouth.OverlayType != MouthOverlay.Closed;

                g.Clear(_agentFile.TransparencyColor);
                for (int i = _currentFrame.Value.Layers.Length - 1; i >= 0; i--) // é desenhado de trás pra frente
                {
                    if (mouthOverlay && _mouth.ReplaceTopFrameImage && i == 0)
                        break;

                    FrameImage fi = _currentFrame.Value.Layers[i];
                    using Bitmap layer = _agentFile.ReadImage(fi.ImageIndex);
                    g.DrawImage(layer, fi.OffsetX, fi.OffsetY);
                }

                if (mouthOverlay)
                {
                    using Bitmap overlay = _agentFile.ReadImage(_mouth.ImageIndex);
                    g.DrawImage(overlay, _mouth.OffsetX, _mouth.OffsetY);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_drewnAgent == null)
                return;
            Graphics g = e.Graphics;
            g.DrawImage(_drewnAgent, 0, 0);
            return;
        }

        public Rect GetRect() => new() { Top = Location.Y, Bottom = Location.Y + Height, Left = Location.X, Right = Location.X + Width };

        public bool IsReady() => Created;
    }
}
