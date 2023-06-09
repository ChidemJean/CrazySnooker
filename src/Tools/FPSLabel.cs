using Godot;
using System;

namespace CrazySnooker.Tools
{
   public class FPSLabel : Label
   {
      public override void _Process(float delta)
      {
         this.Text = Engine.GetFramesPerSecond().ToString();
      }
   }
}