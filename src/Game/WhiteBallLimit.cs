using Godot;
using System;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Game.Managers;

namespace CrazySnooker.Game
{
    public class WhiteBallLimit : Area
    {
        private GameManager gameManager;

        public override void _Ready()
        {
            gameManager = GetNode<GameManager>("%GameManager");
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public void OnBodyEntered(Node node)
        {
            if (node is WhiteBall) {
                gameManager.EmitResetWhiteBall();
            }
        }
    }
}