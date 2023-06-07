using Godot;
using System;
using CrazySnooker.Game.Managers;
using CrazySnooker.Utils;

namespace CrazySnooker.Ui.Mobile
{

    public class CueForceHandler : PanelContainer
    {
        [Export]
        private NodePath gameManagerPath;
        GameManager gameManager;
        
        public override void _Ready()
        {
            gameManager = GetNode<GameManager>(gameManagerPath);
            bool isOnMobile = PlatformUtils.IsOnMobile(OS.GetName());

            if (isOnMobile) {
                Connect("gui_input", this, nameof(OnGuiInput));
            } else {
                Visible = false;
            }
        }

        public void OnGuiInput(InputEvent _event)
        {
            if ((gameManager.playerYou.playerID == -1 || gameManager.playerTurnId != gameManager.playerYou.playerID)) return;

            if (_event is InputEventMouseMotion) {
                InputEventMouseMotion motionEvent = (InputEventMouseMotion) _event;
                Vector2 motion = motionEvent.Relative.Normalized();
                int move = Mathf.CeilToInt(motion.y) * -1;
                gameManager.playerYou.Move(move);
                gameManager.network.SendMoveCue(move);
            }
        }

    }
}