using Godot;
using System;

namespace CrazySnooker.Game.Entities.Traps
{
    public class SpiningTrap : RigidBody
    {
        [Export]
        public float speed = 3f;

        public override void _PhysicsProcess(float delta)
        {
            Vector3 rot = GlobalRotation;
            rot.y += delta * speed;
            GlobalRotation = rot;
        }

    }
}