using Godot;
using System;
using MessagePack;

namespace CrazySnooker.Game.Network.Messages
{
	[MessagePackObject]
	public class BallState
	{		
		[Key(0)]
		public float[] position;

		[Key(1)]
		public float[] orientation;

		[Key(2)]
		public float[] linearVelocity;

		[Key(3)]
		public float[] angularVelocity;

		[Key(4)]
		public int scnIdx;
	}
}