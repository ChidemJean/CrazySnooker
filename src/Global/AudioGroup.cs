using Godot;
using System;
using System.Collections.Generic;

namespace CrazySnooker.Global
{
    public class AudioGroup : Resource
    {
        public enum PlayMode { UI = 1, D3 = 2, D2 = 3 }

        [Export]
        public List<AudioStream> audios = new List<AudioStream>();

        [Export]
        public PlayMode playMode = PlayMode.UI;
    }
}