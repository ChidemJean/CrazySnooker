using Godot;
using System;
using System.Collections.Generic;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Game.Controllers;
using CrazySnooker.Game.Network;
using CrazySnooker.Utils;
using CrazySnooker.Game.Network.Messages;

namespace CrazySnooker.Game.Managers
{
   public class GameManager : Node
   {
      [Signal]
      private delegate void ResetWhiteBall();

      private Vector3 predictionVec = Vector3.Zero;
      public int predictionBallIdx = -1;
      public Vector3 PredictionVec
      {
         set { this.predictionVec = value; }
         get { return predictionVec; }
      }

      [Export]
      private NodePath ballsGroupPath;

      private Spatial ballsGroup;

      private List<GenericBall> balls = new List<GenericBall>();
      public List<GenericBall> Balls { get { return balls; } }

      public int playerTurnId = -1;

      public bool isHosting = false;

      [Export]
      private NodePath playerYouPath;

      [Export]
      private NodePath playerOpponentPath;

      public PoolCueController playerYou;
      public PoolCueController playerOpponent;

      public WhiteBall whiteBall;
      public P2PNetwork network;

      public override void _Ready()
      {
         network = GetNode<P2PNetwork>("%P2PNetwork");
         whiteBall = GetNode<WhiteBall>("%WhiteBall");
         playerYou = GetNode<PoolCueController>(playerYouPath);
         playerOpponent = GetNode<PoolCueController>(playerOpponentPath);
         ballsGroup = GetNode<Spatial>(ballsGroupPath);
         if (ballsGroup != null)
         {
            foreach (Node ball in ballsGroup.GetChildren())
            {
               if (ball is GenericBall)
               {
                  balls.Add((GenericBall)ball);
               }
            }
         }
      }

      public void InitFirstTurn()
      {
         if (isHosting)
         {
            int idTurn = playerYou.playerID;
            playerTurnId = idTurn;
            network.SendInitTurn(idTurn);
            playerYou.ChangeTurn(idTurn);
            playerOpponent.ChangeTurn(idTurn);
         }
      }

      public void UpdateTurn(int id, int idTurn)
      {
         GD.Print($"{id}: turno passou para {idTurn}");
         playerTurnId = idTurn;
         playerYou.ChangeTurn(idTurn);
         playerOpponent.ChangeTurn(idTurn);
      }

      public void BallsPackageReceive(UpdatePackage updatePackage)
      {
         BallState[] ballStates = updatePackage.ballStates;
         foreach (BallState ballState in ballStates)
         {
            if (ballState.scnIdx == 0)
            {
               whiteBall.networkState = ballState;
               continue;
            }
            GenericBall ball = ballsGroup.GetChildOrNull<GenericBall>(ballState.scnIdx - 1);
            if (ball != null)
            {
               ball.networkState = ballState;
            }
         }
      }

      public void SendBallsPackage()
      {
         var whiteBallState = new BallState()
         {
            angularVelocity = MathUtils.Vector3ToFloatArray(whiteBall.AngularVelocity),
            linearVelocity = MathUtils.Vector3ToFloatArray(whiteBall.LinearVelocity),
            position = MathUtils.Vector3ToFloatArray(whiteBall.GlobalTransform.origin),
            orientation = MathUtils.Vector3ToFloatArray(whiteBall.GlobalTransform.basis.GetEuler()),
				scnIdx = 0,
         };
         BallState[] _ballStates = new BallState[balls.Count + 1];
			_ballStates[0] = whiteBallState;
         for (int i = 0; i < balls.Count; i++)
         {
				GenericBall ball = balls[i];
				_ballStates[i + 1] = new BallState() {
					angularVelocity = MathUtils.Vector3ToFloatArray(ball.AngularVelocity),
					linearVelocity = MathUtils.Vector3ToFloatArray(ball.LinearVelocity),
					position = MathUtils.Vector3ToFloatArray(ball.GlobalTransform.origin),
					orientation = MathUtils.Vector3ToFloatArray(ball.GlobalTransform.basis.GetEuler()),
					scnIdx = ball.GetIndex() + 1,
				};
         }
         UpdatePackage updatePackage = new UpdatePackage() {
				ballStates = _ballStates
			};
			network.SendBallsState(updatePackage);
      }

		public void OnTreeExitingBall(Node node)
		{
			balls.Remove((GenericBall) node);
			((GenericBall) node).exiting = true;
		}

      public void EmitResetWhiteBall()
      {
         EmitSignal("ResetWhiteBall");
      }
   }
}