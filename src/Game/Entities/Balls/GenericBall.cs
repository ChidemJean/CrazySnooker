using Godot;
using System;

namespace CrazySnooker.Game.Entities.Balls
{
    public class GenericBall : RigidBody
    {
        [Export]
        private NodePath meshPath;

        private MeshInstance mesh;

        public override void _Ready()
        {
            mesh = GetNode<MeshInstance>(meshPath);
        }

        public void ChangeColor(Color newColor)
        {
            var material = new SpatialMaterial();
            material.AlbedoColor = newColor;
            mesh.MaterialOverride = material;
        }
    }
}