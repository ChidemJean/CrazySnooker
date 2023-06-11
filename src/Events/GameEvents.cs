using Godot;
using System;

namespace CrazySnooker.Events 
{
    public class GameEvent { 
        public const string RenderSizeChanged = "RenderSizeChanged";
        public const string ChangeRenderSize = "ChangeRenderSize";
        public const string ExplosionHappened = "ExplosionHappened";
        public const string CueForceChange = "CueForceChange";
    }
}