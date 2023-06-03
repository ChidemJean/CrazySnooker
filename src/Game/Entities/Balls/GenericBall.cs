using Godot;
using System;

namespace CrazySnooker.Game.Entities.Balls
{
   public class GenericBall : RigidBody
   {
      [Export]
      private NodePath meshPath;

      private MeshInstance mesh;

      private Vector3 currentVelocity = Vector3.Zero;
      private Vector3 previousVelocity = Vector3.Zero;
      public Vector3 CurrentVelocity { get { return currentVelocity; } }
      public Vector3 PreviousVelocity { get { return previousVelocity; } }

      public override void _Ready()
      {
         if (meshPath != null) mesh = GetNode<MeshInstance>(meshPath);
      }

      public void ChangeColor(Color newColor)
      {
         var material = new SpatialMaterial();
         material.AlbedoColor = newColor;
         mesh.MaterialOverride = material;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
        previousVelocity = currentVelocity;
        currentVelocity = state.LinearVelocity;
      }
   }
}