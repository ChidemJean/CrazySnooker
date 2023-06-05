using Godot;
using System;
using CrazySnooker.Utils;
using CrazySnooker.Game.Network;
using CrazySnooker.Game.Network.Messages;

namespace CrazySnooker.Game.Entities.Balls
{
   public class WhiteBall : GenericBall
   {
      private Transform initialTrans;
      private bool shouldReset = false;
      public BallState networkState = null;
      P2PNetwork network;

      public override void _Ready()
      {
         base._Ready();
         initialTrans = GlobalTransform;
         network = GetNode<P2PNetwork>("%P2PNetwork");
         gameManager.Connect("ResetWhiteBall", this, nameof(Reset));
      }

      public void Reset()
      {
         shouldReset = true;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
         base._IntegrateForces(state);
         if (network.isHosting)
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
         if (networkState != null)
         {
            state.LinearVelocity = MathUtils.FloatArrayToVector3(networkState.linearVelocity);
            state.AngularVelocity = MathUtils.FloatArrayToVector3(networkState.angularVelocity);
            Transform curTrans = state.Transform;
            curTrans.origin = MathUtils.FloatArrayToVector3(networkState.position);
            curTrans.basis = new Basis(MathUtils.FloatArrayToVector3(networkState.orientation));
            state.Transform = curTrans;
         }
      }
   }
}