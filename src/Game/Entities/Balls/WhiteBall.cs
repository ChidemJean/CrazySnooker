using Godot;
using System;
using CrazySnooker.Game.Managers;

namespace CrazySnooker.Game.Entities.Balls
{
   public class WhiteBall : GenericBall
   {
      [Export]
      private NodePath predictAreaPath;
      private Area predictArea;
      private GameManager gameManager;
      private Transform initialTrans;
      private bool shouldReset = false;

      public override void _Ready()
      {
         initialTrans = GlobalTransform;
         gameManager = GetNode<GameManager>("%GameManager");
         predictArea = GetNode<Area>(predictAreaPath);
         gameManager.Connect("ResetWhiteBall", this, nameof(Reset));
         predictArea.Connect("body_entered", this, nameof(OnBallEntered));
      }

      public void Reset()
      {
         shouldReset = true;
      }

      public void OnBallEntered(Node node) 
      {
         if (gameManager.PredictionVec == Vector3.Zero) return;

         GenericBall ball = (GenericBall) node;

         float hitForce = 0;
         // bool hitter = false;

         if (ball.PreviousVelocity == Vector3.Zero) {
            // hitter = true;
            hitForce = PreviousVelocity.Length() * ball.Mass;
         } else if (PreviousVelocity == Vector3.Zero) {
            // hitter = false;
            hitForce = ball.PreviousVelocity.Length() * ball.Mass;
         } else {
            float hitVelocity = (ball.PreviousVelocity - PreviousVelocity.Project(ball.PreviousVelocity)).Length();
            // float enemyHitPriority;
            // float ourHitPriority;
            // float collisionAngle = Mathf.Abs(Mathf.Sin(PreviousVelocity.AngleTo(ball.PreviousVelocity) * 0.5f));
            // if (collisionAngle == 0) {
            //    enemyHitPriority = ball.PreviousVelocity.Length();
            //    ourHitPriority = PreviousVelocity.Length();
            // } else {
            //    enemyHitPriority = ball.PreviousVelocity.Length() * (ball.Mass * collisionAngle);
            //    ourHitPriority = PreviousVelocity.Length() * (Mass * collisionAngle);
            // }
            // if (enemyHitPriority > ourHitPriority) {
            //    hitter = false;
            // } else {
            //    hitter = true;
            // }
            hitForce = hitVelocity * ball.Mass;
         }
         
         Vector3 dir = gameManager.PredictionVec.Normalized();
         GD.Print(hitForce, dir);
         ball.AddForce(dir * hitForce * 40, Vector3.Zero);
         AddForce(dir * hitForce * -60, Vector3.Zero);

         gameManager.PredictionVec = Vector3.Zero;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
         base._IntegrateForces(state);
         if (shouldReset) {
            state.LinearVelocity = Vector3.Zero;
            state.AngularVelocity = Vector3.Zero;
            state.Transform = initialTrans;
            shouldReset = false;
         }
      }
   }
}