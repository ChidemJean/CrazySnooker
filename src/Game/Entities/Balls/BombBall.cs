using Godot;
using System;

namespace CrazySnooker.Game.Entities.Balls
{
   public class BombBall : GenericBall
   {
      private Transform initialTrans;

      private AnimationPlayer animationPlayer;

      private bool isActive = false;

      public override void _Ready()
      {
         base._Ready();
         animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
         initialTrans = GlobalTransform;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
         if (mesh != null)
         {
            if (!isActive && state.LinearVelocity.Length() > 0.02f)
            {
               animationPlayer.Play("inflating");
               isActive = true;
            }
            if (isActive && state.LinearVelocity.Length() < 0.02f)
            {
               animationPlayer.Stop(true);
               isActive = false;
               ((SpatialMaterial)mesh.Mesh.SurfaceGetMaterial(0)).AlbedoColor = new Color(1, 1, 1, 1);
               ((SpatialMaterial)mesh.Mesh.SurfaceGetMaterial(1)).AlbedoColor = new Color(1, 1, 1, 1);
					GlobalScale(Vector3.One);
            }
         }

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