using Godot;
using System;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Extensions;
using CrazySnooker.Game.Managers;

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

      [Export(PropertyHint.Layers3dPhysics)]
      private uint projectionMask;

      [Export]
      private float ballRadius = 0.105f;

      [Export]
      private float sizeNextLineProjection = .45f;

      private GenericBall whiteBall;

      private float rotationFactor = 2f;

      private Area areaDetector;

      private Vector3? initialPos = null;
      private Vector3? shotPos = null;

      private float moveFactor = .1f;

      private bool canShot = false;

      private Spatial projection;
      private Spatial projectionNext;
      private MeshInstance projectionMesh;
      private MeshInstance projectionNextMesh;
      private Spatial projectionArrowNext;
      private MeshInstance projectionBall;

      private MeshInstance debugBallTop;
      private MeshInstance debugBallBottom;
      private MeshInstance debugBallRight;
      private MeshInstance debugBallLeft;
      private MeshInstance debugBallMiddle;
      private GameManager gameManager;

      public override void _Ready()
      {
         gameManager = GetNode<GameManager>("%GameManager");
         whiteBall = GetNode<GenericBall>("%WhiteBall");
         areaDetector = GetNode<Area>(areaDetectorPath);
         areaDetector.Connect("body_entered", this, nameof(OnWhiteBallCollide));

         projection = GetNode<Spatial>("%Projection");
         projectionMesh = projection.GetNode<MeshInstance>("Mesh");
         projectionNext = GetNode<Spatial>("%ProjectionNext");
         projectionNextMesh = projectionNext.GetNode<MeshInstance>("Mesh");
         projectionArrowNext = projectionNext.GetNode<Spatial>("arrow");
         projectionBall = GetNode<MeshInstance>("%WhiteBallProjection");

         debugBallTop = GetNode<MeshInstance>("%MiniBallProjectionDebugTop");
         debugBallBottom = GetNode<MeshInstance>("%MiniBallProjectionDebugBottom");
         debugBallRight = GetNode<MeshInstance>("%MiniBallProjectionDebugRight");
         debugBallLeft = GetNode<MeshInstance>("%MiniBallProjectionDebugLeft");
         debugBallMiddle = GetNode<MeshInstance>("%MiniBallProjectionDebugMiddle");
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
            UpdateProjection();
         }
         else
         {
            areaDetector.Visible = false;
            projectionBall.Visible = false;
            projection.Visible = false;
            projectionNext.Visible = false;
            debugBallTop.Visible = false;
            debugBallBottom.Visible = false;
            debugBallRight.Visible = false;
            debugBallLeft.Visible = false;
            debugBallMiddle.Visible = false;
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

      public void UpdateProjection()
      {
         // if (!canShot) return;

         Vector3 forward = GlobalTransform.basis.z.Normalized();
         Vector3 whiteBallPos = whiteBall.GlobalTransform.origin;
         projection.Visible = true;
         PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;

         Vector3 forwardRay = (forward * 1000);

         Vector3 fromTop = whiteBallPos + new Vector3(-ballRadius / 2, 0, 0);
         Vector3 toTop = forwardRay + new Vector3(0, ballRadius, 0);
         var resultTop = spaceState.IntersectRay(fromTop, toTop, null, projectionMask);
         var posTop = CheckProjectionUnit(resultTop, debugBallTop, fromTop);

         Vector3 fromBottom = whiteBallPos + new Vector3(ballRadius / 2, 0, 0);
         Vector3 toBottom = forwardRay + new Vector3(0, -ballRadius, 0);
         var resultBottom = spaceState.IntersectRay(fromBottom, toBottom, null, projectionMask);
         var posBottom = CheckProjectionUnit(resultBottom, debugBallBottom, fromBottom);

         Vector3 fromRight = whiteBallPos + new Vector3(-ballRadius, 0, 0);
         Vector3 toRight = forwardRay + new Vector3(-ballRadius, 0, 0);
         var resultRight = spaceState.IntersectRay(fromRight, toRight, null, projectionMask);
         var posRight = CheckProjectionUnit(resultRight, debugBallRight, fromRight);

         Vector3 fromLeft = whiteBallPos + new Vector3(ballRadius, 0, 0);
         Vector3 toLeft = forwardRay + new Vector3(ballRadius, 0, 0);
         var resultLeft = spaceState.IntersectRay(fromLeft, toLeft, null, projectionMask);
         var posLeft = CheckProjectionUnit(resultLeft, debugBallLeft, fromLeft);

         Vector3 fromMiddle = whiteBallPos;
         Vector3 toMiddle = forwardRay;
         var resultMiddle = spaceState.IntersectRay(fromMiddle, toMiddle, null, projectionMask);
         var posMiddle = CheckProjectionUnit(resultMiddle, debugBallMiddle, fromMiddle);

         float minDist = Mathf.Inf;
         Vector3 minPosCollided = Vector3.Zero;
         Vector3 minNormal = Vector3.Zero;
         Vector3 objCenter = Vector3.Zero;

         if (posTop != null)
         {
            var posT = (Vector3)posTop[0];
            float distT = posT.DistanceTo(whiteBallPos);
            if (minDist > distT)
            {
               minPosCollided = posT;
               minDist = distT;
               minNormal = posTop[1];
               objCenter = posTop[2];
            }
         }
         if (posBottom != null)
         {
            var posB = (Vector3)posBottom[0];
            float distB = posB.DistanceTo(whiteBallPos);
            if (minDist > distB)
            {
               minPosCollided = posB;
               minDist = distB;
               minNormal = posBottom[1];
               objCenter = posBottom[2];
            }
         }
         if (posRight != null)
         {
            var posR = (Vector3)posRight[0];
            float distR = posR.DistanceTo(whiteBallPos);
            if (minDist > distR)
            {
               minPosCollided = posR;
               minDist = distR;
               minNormal = posRight[1];
               objCenter = posRight[2];
            }
         }
         if (posLeft != null)
         {
            var posL = (Vector3)posLeft[0];
            float distL = posL.DistanceTo(whiteBallPos);
            if (minDist > distL)
            {
               minPosCollided = posL;
               minDist = distL;
               minNormal = posLeft[1];
               objCenter = posLeft[2];
            }
         }
         if (posMiddle != null)
         {
            var posM = (Vector3)posMiddle[0];
            float distM = posM.DistanceTo(whiteBallPos);
            if (minDist > distM)
            {
               minPosCollided = posM;
               minDist = distM;
               minNormal = posMiddle[1];
               objCenter = posMiddle[2];
            }
         }

         Vector3 posCollidedProj = minPosCollided + minNormal * ballRadius;
         Vector3 projectionFinalPos = whiteBallPos + (forward * posCollidedProj.DistanceTo(whiteBallPos));

         // PROJECTION BALL
         projectionBall.GlobalTranslation = projectionFinalPos;
         projectionBall.Visible = true;

         // PROJECTION LINE
         var heightLine = whiteBallPos.DistanceTo(projectionFinalPos);
         (projectionMesh.Mesh as CylinderMesh).Height = heightLine - ballRadius * 2;

         projection.GlobalTranslation = whiteBallPos;
         Transform projectionTrans = projection.GlobalTransform;
         Basis lookAtBasis = projectionTrans.LookAtBasis(projectionFinalPos);
         projectionTrans.basis = lookAtBasis;
         projection.GlobalTransform = projectionTrans;

         Vector3 meshTranslation = projectionMesh.Translation;
         meshTranslation.z = heightLine / 2;
         projectionMesh.Translation = meshTranslation;

         // COLLIDED WITH ANY
         if (minDist != Mathf.Inf)
         {
            projectionNext.Visible = true;
            // PROJECTION NEXT LINE
            if (objCenter != Vector3.Zero)
            {
               // IS A BALL
               projectionNext.GlobalTranslation = objCenter;
               Transform projectionNextTrans = projectionNext.GlobalTransform;
               Vector3 predictionPoint = objCenter + objCenter.DirectionTo(projectionFinalPos) * -1 * 5;
               Basis lookAtBasisNext = projectionNextTrans.LookAtBasis(predictionPoint);
               projectionNextTrans.basis = lookAtBasisNext;
               projectionNext.GlobalTransform = projectionNextTrans;
               gameManager.PredictionVec = projectionNext.GlobalTransform.basis.z;
            }
            else
            {
               // IS TABLE
               projectionNext.GlobalTranslation = projectionFinalPos;
               Transform projectionNextTrans = projectionNext.GlobalTransform;
               Vector3 normal = projectionFinalPos + minNormal * 10;
               float angle = projectionFinalPos.DirectionTo(whiteBallPos).SignedAngleTo(normal, Vector3.Up);
               Vector3 flippedVec = normal.Rotated(Vector3.Up, angle * .5f);
               Basis lookAtBasisNext = projectionNextTrans.LookAtBasis(flippedVec * 10);
               projectionNextTrans.basis = lookAtBasisNext;
               projectionNext.GlobalTransform = projectionNextTrans;
            }

            (projectionNextMesh.Mesh as CylinderMesh).Height = sizeNextLineProjection;

            Vector3 nextMeshTranslation = projectionNextMesh.Translation;
            nextMeshTranslation.z = sizeNextLineProjection / 2;
            projectionNextMesh.Translation = nextMeshTranslation;

            Vector3 arrowNextTranslation = projectionArrowNext.Translation;
            arrowNextTranslation.z = sizeNextLineProjection;
            projectionArrowNext.Translation = arrowNextTranslation;
         }
      }

      public Vector3[] CheckProjectionUnit(Godot.Collections.Dictionary result, Spatial miniBall, Vector3 from)
      {
         bool hasCollision = result != null && result.Contains("collider");
         miniBall.Visible = hasCollision;
         if (hasCollision)
         {
            Spatial collider = (Spatial)result["collider"];
            Vector3 objCenter = collider.GlobalTransform.origin;
            Vector3 position = (Vector3)result["position"];
            Vector3 normal = (Vector3)result["normal"];
            if (collider is GenericBall && !(collider is WhiteBall))
            {
               miniBall.GlobalTranslation = position;
            }
            return new Vector3[] { position, normal, objCenter };
         }
         return null;
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
            whiteBall.AddForce(GlobalTransform.basis.z.Normalized() * (maxForce * distanceAvg) * 60, new Vector3(0,0,0));
            canShot = false;
            shotPos = null;
         }
      }
   }
}