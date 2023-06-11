using Godot;
using System;
using CrazySnooker.Events;
using CrazySnooker.Game.Managers;

namespace CrazySnooker.Ui
{

    public class ForceMeasure : Control
    {
        [Export]
        private NodePath progressPath;

        [Export]
        private NodePath gameManagerPath;

        private TextureProgress textureProgress;

        GlobalEvents globalEvents;
        GameManager gameManager;

        SceneTreeTween tween;

        public override void _Ready()
        {
            gameManager = GetNode<GameManager>(gameManagerPath);
            globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
            textureProgress = GetNode<TextureProgress>(progressPath);
            globalEvents.Connect(GameEvent.CueForceChange, this, nameof(OnCueForceChange));

            gameManager.Connect("ChangeTurnEvent", this, nameof(OnChangeTurn));
        }

        public void OnChangeTurn()
        {
            if (tween != null) {
                tween.Stop();
            }
            textureProgress.Value = 0;
        }

        public override void _PhysicsProcess(float delta)
        {
            Visible = gameManager.playerYou.playerID != -1 && gameManager.playerYou.playerID == gameManager.playerTurnId && !gameManager.playerYou.waitingFinishTurn;
        }

        public void OnCueForceChange(float force)
        {
            if (tween != null) {
                tween.Stop();
            }
            tween = GetTree().CreateTween();
            tween.TweenProperty(textureProgress, "value", Mathf.Clamp(100 * force, 0, 100), .35f);
        }

    }
}