using Godot;
using System;

namespace CrazySnooker.Game.Entities.Balls
{
   public class WhiteBall : GenericBall
   {
      private Transform initialTrans;
      private bool shouldReset = false;

      public override void _Ready()
      {
         base._Ready();
         initialTrans = GlobalTransform;
         gameManager.Connect("ResetWhiteBall", this, nameof(Reset));
      }

      public void Reset()
      {
         shouldReset = true;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
         base._IntegrateForces(state);
         if (gameManager.playerYou.playerID == gameManager.playerTurnId)
         {
            if (shouldReset)
            {
               state.LinearVelocity = Vector3.Zero;
               state.AngularVelocity = Vector3.Zero;
               state.Transform = initialTrans;
               shouldReset = false;
            }

            return;
         }
      }
   }
}