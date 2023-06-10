using Godot;
using System;

namespace CrazySnooker.Game.Entities.Balls
{

   public class FoodBall : GenericBall
   {
      [Export]
      public float calories = 0f;

		private Transform initialTrans;

		public override void _Ready()
      {
         base._Ready();
         initialTrans = GlobalTransform;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
         base._IntegrateForces(state);
         if (gameManager.playerYou.playerID == gameManager.playerTurnId)
         {
            if (state.Transform.origin.y < -1f)
            {
               state.LinearVelocity = Vector3.Zero;
               state.AngularVelocity = Vector3.Zero;
               state.Transform = initialTrans;
            }

            return;
         }
      }
   }
}