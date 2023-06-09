using Godot;
using System;
using CrazySnooker.Events;

namespace CrazySnooker.Tools
{
    public class RenderSize : Label
    {
        GlobalEvents globalEvents;

        public override void _Ready()
        {
            globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
            globalEvents.Connect(GameEvent.RenderSizeChanged, this, nameof(OnRenderSizeChanged));
        }

        public void OnRenderSizeChanged(Vector2 newSize, float scale)
        {
            Text = $"{newSize.x}x{newSize.y} ({100*scale}%)";
        }

    }
}