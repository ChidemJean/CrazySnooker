using Godot;
using System;
using System.Collections.Generic;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Game.Controllers;
using CrazySnooker.Game.Network;
using CrazySnooker.Utils;
using CrazySnooker.Game.Network.Messages;
using CrazySnooker.Game.Particles;
using CrazySnooker.Game.Entities.Characters;

namespace CrazySnooker.Game.Managers
{
   public class GameManager : Node
   {
      [Signal]
      private delegate void ResetWhiteBall();

		[Signal]
      private delegate void WinnerEvent();

		[Signal]
      private delegate void LoserEvent();

		[Signal]
      private delegate void InitCategory();

		[Signal]
      private delegate void ChangeTurnEvent();

		[Signal]
      private delegate void BallPocketed();

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

      [Export]
      private Godot.Collections.Dictionary<string, PackedScene> particles = new Godot.Collections.Dictionary<string, PackedScene>();

      public PoolCueController playerYou;
      public PoolCueController playerOpponent;

      public WhiteBall whiteBall;
      public INetwork network;

      public BallCategory yourBallCategory = BallCategory.UNDEFINED;
      public BallCategory opponentBallCategory = BallCategory.UNDEFINED;

      public Dictionary<BallCategory, int> categoriesQtd = new Dictionary<BallCategory, int>();

      public List<BallCategory> pocketedBalls = new List<BallCategory>();

      public override void _Ready()
      {
         network = GetNode<INetwork>("%Network");
         whiteBall = GetNode<WhiteBall>("%WhiteBall");
         playerYou = GetNode<PoolCueController>(playerYouPath);
         playerOpponent = GetNode<PoolCueController>(playerOpponentPath);
         ballsGroup = GetNode<Spatial>(ballsGroupPath);

			categoriesQtd.Add(BallCategory.HEALTHY, 0);
			categoriesQtd.Add(BallCategory.NOT_HEALTHY, 0);
			categoriesQtd.Add(BallCategory.BLACK_BALL, 0);

         if (ballsGroup != null)
         {
            foreach (Node ball in ballsGroup.GetChildren())
            {
               if (ball is GenericBall)
               {
                  GenericBall _ball = (GenericBall)ball;
                  balls.Add(_ball);
                  categoriesQtd[_ball.category]++;
               }
            }
         }
      }

      public void AddPocketedBall(GenericBall ball)
      {
         if (playerTurnId == playerYou.playerID) {
            pocketedBalls.Add(ball.category);
         }
      }

      public bool IsNextTurnMy()
      {
         if (pocketedBalls.Count > 0 && pocketedBalls[0] == yourBallCategory) {
            return true;
         }

         return false;
      }

      public void ClearPocketedBalls()
      {
         pocketedBalls.Clear();
      }

      public void InitFirstTurn()
      {
         if (isHosting)
         {
            SendUpdateTurn();
         }
      }

      public void SendUpdateTurn()
      {
         ClearPocketedBalls();
         //
         int idTurn = playerOpponent.playerID;
         playerTurnId = idTurn;
         network.SendUpdateTurn(idTurn);
         playerYou.ChangeTurn(idTurn);
         playerOpponent.ChangeTurn(idTurn);
			EmitSignal(nameof(ChangeTurnEvent));
      }

      public void UpdateTurn(int idTurn)
      {
         GD.Print($"Turno passou para {idTurn}");
         playerTurnId = idTurn;
         playerYou.ChangeTurn(idTurn);
         playerOpponent.ChangeTurn(idTurn);
			EmitSignal(nameof(ChangeTurnEvent));
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
            GenericBall ball = GetBallById(ballState.scnIdx);
            if (ball != null)
            {
               ball.networkState = ballState;
            }
         }
      }

		public GenericBall GetBallById(int id)
		{
			for (int i = 0; i < balls.Count; i++)
         {
            GenericBall ball = balls[i];
				if (ball.id == id) {
					return ball;
				}
			}
			return null;
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
         List<BallState> _ballStates = new List<BallState>();
         _ballStates.Add(whiteBallState);
         for (int i = 0; i < balls.Count; i++)
         {
            GenericBall ball = balls[i];
            
            if (!IsInstanceValid(ball)) continue;
            if (ball.LinearVelocity.Length() == 0) continue;

            _ballStates.Add(new BallState()
            {
               angularVelocity = MathUtils.Vector3ToFloatArray(ball.AngularVelocity),
               linearVelocity = MathUtils.Vector3ToFloatArray(ball.LinearVelocity),
               position = MathUtils.Vector3ToFloatArray(ball.GlobalTransform.origin),
               orientation = MathUtils.Vector3ToFloatArray(ball.GlobalTransform.basis.GetEuler()),
               scnIdx = ball.id,
            });
         }

         UpdatePackage updatePackage = new UpdatePackage()
         {
            ballStates = _ballStates.ToArray()
         };
         network.SendBallsState(updatePackage);
      }

      public BallCategory GetInverseBallCategory(BallCategory category)
      {
         switch (category)
         {
            case BallCategory.HEALTHY:
               return BallCategory.NOT_HEALTHY;
            case BallCategory.NOT_HEALTHY:
               return BallCategory.HEALTHY;
         }
         return BallCategory.UNDEFINED;
      }

      public void CreatureDied(Creature creature)
      {
         if (playerTurnId == playerYou.playerID) {
            Loser();
         }
         if (playerTurnId == playerOpponent.playerID) {
            Winner();
         }
      }

      public void OnTreeExitingBall(Node node)
      {
         GenericBall ball = (GenericBall)node;
         balls.Remove(ball);
         ball.exiting = true;
         BallCategory category = ball.category;

         if (!categoriesQtd.ContainsKey(category)) return;

			if (categoriesQtd[category] > 0) {
         	categoriesQtd[category]--;
			}

         if (category != BallCategory.HEALTHY && category != BallCategory.NOT_HEALTHY) {
            return;
         }

         bool isYourTurn = playerYou.playerID == playerTurnId;

         if (yourBallCategory == BallCategory.UNDEFINED || opponentBallCategory == BallCategory.UNDEFINED)
         {
            if (isYourTurn)
            {
               yourBallCategory = category;
               opponentBallCategory = GetInverseBallCategory(category);
            }
            else
            {
               opponentBallCategory = category;
               yourBallCategory = GetInverseBallCategory(category);
            }

				EmitSignal(nameof(InitCategory));
				EmitSignal(nameof(BallPocketed));
            return;
         }

			EmitSignal(nameof(BallPocketed));

         if (category == yourBallCategory)
         {
				if (categoriesQtd[category] == 0) {
					Winner();
				} else {
					// MAKE POINT
				}
         }
			if (category == opponentBallCategory)
         {
				if (categoriesQtd[category] == 0) {
					Loser();
				} else {
					//
				}
         }
      }

      public void Winner()
      {
			EmitSignal(nameof(WinnerEvent));
      }

      public void Loser()
      {
			EmitSignal(nameof(LoserEvent));
      }

      public void EmitResetWhiteBall()
      {
         EmitSignal("ResetWhiteBall");
      }

      public bool AnyBallIsMoving()
      {
         if (whiteBall.LinearVelocity.Length() >= .02f)
         {
            return true;
         }
         for (int i = 0; i < balls.Count; i++)
         {
            GenericBall ball = balls[i];
            if (IsInstanceValid(ball) && ball.LinearVelocity.Length() >= .02f)
            {
               return true;
            }
         }
         return false;
      }

		public string GetCategoryName(BallCategory ballCategory)
		{
			switch (ballCategory)
         {
            case BallCategory.HEALTHY:
               return "Saudáveis";
            case BallCategory.NOT_HEALTHY:
               return "Não saudáveis";
         }

			return "--";
		}

		public bool IsYourTurn()
		{
			return playerYou.playerID == playerTurnId;
		}

      public void EmitParticleInPosition(string particleKey, Vector3 position)
      {
         PackedScene scn = particles[particleKey];
         MultiParticles particlesNode = scn.Instance<MultiParticles>();
         GetParent().AddChild(particlesNode);
         particlesNode.GlobalTranslation = position;
         particlesNode.Play();
      }
   }
}