using Godot;
using System;
using CrazySnooker.Game.Network.Messages;

namespace CrazySnooker.Game.Network
{

   public interface INetwork
   {
        void SendMoveCue(int move);
        void Join(string ip);
        void SendUpdateTurn(int idTurn);
        void SendRotationCue(Vector3 rotation);
        void SendShot();
        void SendBallsState(UpdatePackage updatePackage);
   }
}