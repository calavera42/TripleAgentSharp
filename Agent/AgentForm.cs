using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Media;
using System.Text;
using System.Timers;

namespace Agent
{
    public partial class AgentForm : Form
    {
        private Animation? _animation;

        public AgentForm()
        {
            InitializeComponent();
            TopMost = true;
        }
    }
}
