using Godot;
using System;

namespace CrazySnooker.Game.Entities.Balls
{

    public class FoodBall : GenericBall
    {
        [Export]
        public float calories = 0f;
    }
}