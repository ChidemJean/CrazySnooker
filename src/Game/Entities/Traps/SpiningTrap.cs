using Godot;
using System;

namespace CrazySnooker.Game.Entities.Traps
{
    public class SpiningTrap : RigidBody
    {
        [Export]
        public float speed = 3f;

        [Export]
        private NodePath spinningNodePath;

        private Spatial spinningNode;

        public override void _Ready()
        {
            spinningNode = GetNode<Spatial>(spinningNodePath);
        }

        public override void _PhysicsProcess(float delta)
        {
            Vector3 rot = spinningNode.Rotation;
            rot.y += delta * speed;
            spinningNode.Rotation = rot;
        }

    }
}