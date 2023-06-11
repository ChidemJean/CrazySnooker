using Godot;
using System;
using CrazySnooker.Events;

namespace CrazySnooker.Game
{

   public class Viewport3D : Viewport
   {
      [Export] 
		private float scaleFactor = 1.0f;
      
		public float currentScale;

		Vector2 originalSize;

      GlobalEvents globalEvents;

      public async override void _Ready()
      {
         globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
			originalSize = Size;
         currentScale = scaleFactor;
         Connect("size_changed", this, nameof(OnResize));
         await ToSignal(GetTree().CreateTimer(.75f), "timeout");
			UpdateViewport3DSize(scaleFactor);
         globalEvents.Connect(GameEvent.ChangeRenderSize, this, nameof(UpdateViewport3DSize));
      }

      public void OnResize()
      {
			originalSize = this.Size;
         UpdateViewport3DSize(this.currentScale);
      }

      public void UpdateViewport3DSize(float value)
      {

         float scaleFactorClamped = Mathf.Clamp(value, 0.2f, 1f);
         this.Size = OS.WindowSize * scaleFactorClamped;
         GetTree().Root.GetViewport().Size = OS.WindowSize;
         this.currentScale = scaleFactorClamped;

         globalEvents.EmitSignal(GameEvent.RenderSizeChanged, this.Size, scaleFactorClamped);
      }
   }
}