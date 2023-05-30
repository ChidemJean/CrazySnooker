using Godot;
using System;
using CrazySnooker.Game.Entities.Balls;

namespace CrazySnooker.Game.Controllers
{
   public class PoolCueController : Spatial
   {

      [Export]
      private NodePath areaDetectorPath;

      [Export]
      private float maxDistance = 1f;

      [Export]
      private float maxForce = 140f;

      private GenericBall whiteBall;

      private float rotationFactor = 2f;

      private Area areaDetector;

      private Vector3? initialPos = null;
      private Vector3? shotPos = null;

      private float moveFactor = .1f;

      private bool canShot = false;

      public override void _Ready()
      {
         whiteBall = GetNode<GenericBall>("%WhiteBall");
         areaDetector = GetNode<Area>(areaDetectorPath);
         areaDetector.Connect("body_entered", this, nameof(OnWhiteBallCollide));
      }

      public void OnWhiteBallCollide(Node bodyEntered)
      {
         if (bodyEntered is WhiteBall)
         {
            ApplyImpulse();
         }
      }

      public override void _PhysicsProcess(float delta)
      {
         var forward = GlobalTransform.basis.z.Normalized();
         GlobalTranslation = whiteBall.GlobalTransform.origin - forward * .1f;
         if (whiteBall.LinearVelocity.Length() == 0)
         {
            areaDetector.Visible = true;
         }
         else
         {
            areaDetector.Visible = false;
         }
      }

      public override void _Input(InputEvent ev)
      {
         if (ev is InputEventMouseMotion)
         {
            Vector2 rel = (ev as InputEventMouseMotion).Relative;
            float move = (rel.x / (rotationFactor)) * -1;
            RotationDegrees = new Vector3(RotationDegrees.x, RotationDegrees.y + move, RotationDegrees.z);
         }
         if (ev is InputEventMouseButton)
         {
            InputEventMouseButton emb = (InputEventMouseButton)ev;
            if (emb.IsPressed())
            {
               if (emb.ButtonIndex == (int)ButtonList.WheelUp)
               {
                  Move(1);
               }
               if (emb.ButtonIndex == (int)ButtonList.WheelDown)
               {
                  Move(-1);
               }
               if (emb.ButtonIndex == (int)ButtonList.Left)
               {
                  Shot();
               }
            }
         }
      }

      public Vector3 DirectionFromInitial(Vector3 moveValue)
      {
         if (initialPos == null) return Vector3.Zero;
         return moveValue.DirectionTo((Vector3)initialPos).Normalized();
      }

      public float DistanceFromInitial(Vector3 moveValue)
      {
         if (initialPos == null) return 0;
         return moveValue.DistanceTo((Vector3)initialPos);
      }

      public void Move(int value)
      {
         if (initialPos == null)
         {
            initialPos = areaDetector.Translation as Vector3?;
         }
         var forward = areaDetector.Transform.basis.x.Normalized() * value;
         var moveValue = areaDetector.Translation + forward * moveFactor;
         var directionFromInitial = DirectionFromInitial(moveValue);
         var distanceFromInitial = DistanceFromInitial(moveValue);
         if (directionFromInitial.x <= 0 && distanceFromInitial <= maxDistance)
         {
            areaDetector.Translation = moveValue;
         }
      }

      public void Shot()
      {
         canShot = true;
         shotPos = areaDetector.Translation;
         SceneTreeTween tween = GetTree().CreateTween();
         tween.TweenProperty(areaDetector, "translation", initialPos, .2f).SetTrans(Tween.TransitionType.Circ);
      }

      public void ApplyImpulse()
      {
         if (canShot)
         {
            var distanceAvg = DistanceFromInitial((Vector3)shotPos);
            whiteBall.ApplyCentralImpulse(GlobalTransform.basis.z.Normalized() * (maxForce * distanceAvg));
            canShot = false;
            shotPos = null;
         }
      }
   }
}