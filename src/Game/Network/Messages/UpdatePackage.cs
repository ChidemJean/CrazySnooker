using Godot;
using System;
using MessagePack;

namespace CrazySnooker.Game.Network.Messages
{
	[MessagePackObject]
   public class UpdatePackage
   {

		[Key(0)]
		public BallState[] ballStates;

   }
}