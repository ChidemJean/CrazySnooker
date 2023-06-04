using Godot;
using System;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Game.Managers;
using CrazySnooker.Global;

namespace CrazySnooker.Game
{
    public class WhiteBallLimit : Area
    {
        private GameManager gameManager;
        private AudioManager audioManager;

        public override void _Ready()
        {
            gameManager = GetNode<GameManager>("%GameManager");
            audioManager = GetNode<AudioManager>("/root/MainScene/AudioManager");
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public void OnBodyEntered(Node node)
        {
            if (node is WhiteBall) {
                gameManager.EmitResetWhiteBall();
                return;
            }
            GenericBall ball = (GenericBall) node;
            node.QueueFree();
            audioManager.Play("crunch", null, GlobalTranslation);
        }
    }
}