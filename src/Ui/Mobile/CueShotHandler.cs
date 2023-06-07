using Godot;
using System;
using CrazySnooker.Game.Managers;
using CrazySnooker.Utils;

namespace CrazySnooker.Ui.Mobile
{

   public class CueShotHandler : TextureButton
   {
      [Export]
      private NodePath gameManagerPath;
      GameManager gameManager;

      public override void _Ready()
      {
         gameManager = GetNode<GameManager>(gameManagerPath);
         bool isOnMobile = PlatformUtils.IsOnMobile(OS.GetName());

         if (isOnMobile)
         {
            Connect("gui_input", this, nameof(OnGuiInput));
         }
         else
         {
            Visible = false;
         }
      }

      public void OnGuiInput(InputEvent _event)
      {
         if ((gameManager.playerYou.playerID == -1 || gameManager.playerTurnId != gameManager.playerYou.playerID)) return;

         if (_event is InputEventMouseButton)
         {
            InputEventMouseButton clickEvent = (InputEventMouseButton)_event;
            if (clickEvent.IsPressed())
            {
               gameManager.playerYou.Shot();
               gameManager.network.SendShot();
            }
         }
      }
   }
}