using Godot;
using System;

namespace CrazySnooker.Game.Managers
{
    public class GameManager : Node
    {
        [Signal]
        private delegate void ResetWhiteBall();

        private Vector3 predictionVec = Vector3.Zero;
        public int predictionBallIdx = -1;
        public Vector3 PredictionVec { 
            set { this.predictionVec = value; } 
            get { return predictionVec; }
        }

        public override void _Ready()
        {
            
        }

        public void EmitResetWhiteBall()
        {
            EmitSignal("ResetWhiteBall");
        }
    }
}