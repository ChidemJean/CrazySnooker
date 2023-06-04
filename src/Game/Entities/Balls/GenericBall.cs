using Godot;
using System;
using CrazySnooker.Global;
using CrazySnooker.Game.Managers;
using CrazySnooker.Game.Entities.Tables;

namespace CrazySnooker.Game.Entities.Balls
{
   public class GenericBall : RigidBody
   {
      [Export]
      private NodePath meshPath;

      private MeshInstance mesh;

      private Vector3 currentVelocity = Vector3.Zero;
      private Vector3 previousVelocity = Vector3.Zero;
      public Vector3 CurrentVelocity { get { return currentVelocity; } }
      public Vector3 PreviousVelocity { get { return previousVelocity; } }

      protected AudioManager audioManager;

      protected GameManager gameManager;

      public override void _Ready()
      {
         gameManager = GetNode<GameManager>("%GameManager");
         audioManager = GetNode<AudioManager>("/root/MainScene/AudioManager");
         if (meshPath != null) mesh = GetNode<MeshInstance>(meshPath);
         Connect("body_entered", this, nameof(OnBallEntered));
      }

      public void OnBallEntered(Node node) 
      {
         if (node is TableWall) {
            TableWall table = (TableWall) node;
            float hitForce = HitForceStatic(table);
            if (hitForce < 8f) return;

            audioManager.Play("ball_in_ball", null, GlobalTranslation);
            return;
         }
         if (node is GenericBall) {
            GenericBall ball = (GenericBall) node;
            float hitForce = HitForce(ball);
            if (hitForce < 8f) return;

            audioManager.Play("ball_in_ball", null, ball.GlobalTranslation);

            if (gameManager.PredictionVec == Vector3.Zero) return;
            
            Vector3 dir = gameManager.PredictionVec.Normalized();
            ball.AddForce(dir * hitForce * 40, Vector3.Zero);
            AddForce(dir * hitForce * -60, Vector3.Zero);

            gameManager.PredictionVec = Vector3.Zero;
         }
      }

      public void ChangeColor(Color newColor)
      {
         var material = new SpatialMaterial();
         material.AlbedoColor = newColor;
         mesh.MaterialOverride = material;
      }

      public override void _IntegrateForces(PhysicsDirectBodyState state)
      {
        previousVelocity = currentVelocity;
        currentVelocity = state.LinearVelocity;
      }

      public float HitForceStatic(StaticBody body)
      {
         float hitForce = 0;
         hitForce = PreviousVelocity.Length() * Mass;

         return hitForce;
      }

      public float HitForce(GenericBall ball)
      {
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

         return hitForce;
      }
   }
}