using Godot;
using System;

namespace CrazySnooker.Events
{
   public class GlobalEvents : Node
   {
      
		[Signal]
      delegate void RenderSizeChanged(Vector2 newSize, float scale);

      [Signal]
      delegate void ChangeRenderSize(float scale);

      [Signal]
      delegate void ExplosionHappened(Vector3 position);

      [Signal]
      delegate void CueForceChange(float avg);

   }
}