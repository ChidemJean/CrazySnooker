using Godot;
using System;
using MessagePack;
using CrazySnooker.Game.Network;

namespace CrazySnooker.Game.Network.Messages
{
	[MessagePackObject]
   public class UpdatePackage
   {

		[Key(0)]
		public BallState[] ballStates;

      [Key(1)]
      public int type;
   }
}