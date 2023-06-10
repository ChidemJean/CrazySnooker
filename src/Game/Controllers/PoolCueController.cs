using Godot;
using System;
using System.Collections.Generic;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Extensions;
using CrazySnooker.Game.Managers;
using CrazySnooker.Global;
using CrazySnooker.Utils;
using CrazySnooker.Game.Network;
using CrazySnooker.Game.Network.Messages;

namespace CrazySnooker.Game.Controllers
{
   public struct ProjectionResultData
   {
      public float minDist;
      public Vector3 minPosCollided;
      public Vector3 minNormal;
      public Vector3 objCenter;
      public Node curTargetProj;
   }


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

      [Export]
      private NodePath cameraPath;

      private Camera camera;

      private Camera outCamera;

      private GenericBall whiteBall;

      [Export]
      private float rotationFactor = 2f;

      [Export]
      private bool manualProjection = false;

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
      public GameManager gameManager;
      private AudioManager audioManager;
      private INetwork network;
      private MainScene mainScene;

      public int playerID = -1;

      [Export]
      public bool isRemote = false;

      [Export]
      public float timeToFinishTurn = 6f;

      [Export]
      public float waitingTimeToFinishTurn = 6f;

      public bool waitingFinishTurn = false;

      public bool canShotInput = true;
      public bool isYourTurn = false;

      private List<RayCast> rays = new List<RayCast>();

      public override void _Ready()
      {
         gameManager = GetNode<GameManager>("%GameManager");
         network = GetNode<INetwork>("%Network");
         audioManager = GetNode<AudioManager>("/root/MainScene/AudioManager");
         mainScene = GetNode<MainScene>("/root/MainScene");
         whiteBall = GetNode<GenericBall>("%WhiteBall");
         areaDetector = GetNode<Area>(areaDetectorPath);
         areaDetector.Connect("body_entered", this, nameof(OnWhiteBallCollide));
         camera = GetNode<Camera>(cameraPath);
         outCamera = GetNode<Camera>("%OutCamera");

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

         foreach (Node node in GetChildren())
         {
            if (node is RayCast)
            {
               rays.Add((RayCast)node);
            }
         }

         canShotInput = !PlatformUtils.IsOnMobile(OS.GetName());
      }

      public void UpdateID(int id)
      {
         playerID = id;
      }

      public void ChangeTurn(int id)
      {
         canShot = false;
         if (initialPos != null) areaDetector.Translation = (Vector3)initialPos;

         if (id == playerID)
         {
            Visible = true;
            if (!isRemote)
            {
               camera.Current = true;
               outCamera.Current = false;
               isYourTurn = true;
            }
         }
         else
         {
            Visible = false;
            if (!isRemote)
            {
               outCamera.Current = true;
               camera.Current = false;
               isYourTurn = false;
            }
         }
      }

      public void OnWhiteBallCollide(Node bodyEntered)
      {
         if (bodyEntered is WhiteBall && gameManager.playerTurnId == playerID && !waitingFinishTurn)
         {
            ApplyImpulse();
         }
      }

      public override void _PhysicsProcess(float delta)
      {
         if (playerID == -1) return;

         var forward = GlobalTransform.basis.z.Normalized();
         GlobalTranslation = whiteBall.GlobalTransform.origin - forward * .1f;

         if (gameManager.playerTurnId == playerID)
         {
            bool anyBallMoving = gameManager.AnyBallIsMoving();

            if (!isRemote)
            {
               if (Engine.GetPhysicsFrames() % 10 == 0)
               {
                  gameManager.SendBallsPackage();
                  if (waitingFinishTurn)
                  {
                     if (gameManager.IsNextTurnMy())
                     {
                        waitingFinishTurn = false;
                        gameManager.ClearPocketedBalls();
                        return;
                     }
                     if (!anyBallMoving)
                     {
                        waitingFinishTurn = false;
                        gameManager.SendUpdateTurn();
                        return;
                     }
                  }
               }
            }

            if (!anyBallMoving)
            {
               areaDetector.Visible = true;
               UpdateProjection();
            }
            else
            {
               areaDetector.Visible = false;
               UpdateProjectionVisible(false);
            }
         }
      }

      public void UpdateProjectionVisible(bool visible)
      {
         projectionBall.Visible = visible;
         projection.Visible = visible;
         // projectionNext.Visible = visible;
         debugBallTop.Visible = visible;
         debugBallBottom.Visible = visible;
         debugBallRight.Visible = visible;
         debugBallLeft.Visible = visible;
         debugBallMiddle.Visible = visible;
      }

      public override void _Input(InputEvent ev)
      {
         if (isRemote || (playerID == -1 || gameManager.playerTurnId != playerID)) return;

         if (ev is InputEventMouseMotion)
         {
            Vector2 rel = (ev as InputEventMouseMotion).Relative;
            float move = (-rel.x * rotationFactor);
            // RotationDegrees = new Vector3(RotationDegrees.x, RotationDegrees.y + move, RotationDegrees.z);
            RotateY(move);
            network.SendRotationCue(RotationDegrees);
         }
         if (ev is InputEventMouseButton)
         {
            InputEventMouseButton emb = (InputEventMouseButton)ev;
            if (emb.IsPressed())
            {
               if (emb.ButtonIndex == (int)ButtonList.WheelUp)
               {
                  Move(1);
                  network.SendMoveCue(1);
               }
               if (emb.ButtonIndex == (int)ButtonList.WheelDown)
               {
                  Move(-1);
                  network.SendMoveCue(-1);
               }
               if (canShotInput && emb.ButtonIndex == (int)ButtonList.Left)
               {
                  Shot();
                  network.SendShot();
               }
            }
         }
      }

      public void UpdateRotationCue(Vector3 rotation)
      {
         RotationDegrees = rotation;
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
         if (isRemote || gameManager.playerTurnId != playerID || waitingFinishTurn)
         {
            UpdateProjectionVisible(false);
            return;
         }

         UpdateProjectionVisible(true);

         Vector3 forward = GlobalTransform.basis.z.Normalized();
         Vector3 whiteBallPos = whiteBall.GlobalTransform.origin;
         PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;

         ProjectionResultData resultData = manualProjection ? GetManualProjetionResult() : GetProjetionResult();

         Vector3 posCollidedProj = resultData.minPosCollided + resultData.minNormal * ballRadius;
         Vector3 projectionFinalPos = whiteBallPos + (forward * posCollidedProj.DistanceTo(whiteBallPos));

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

         // PROJECTION BALL
         projectionBall.GlobalTranslation = projectionFinalPos;
         projectionBall.Visible = true;

         // PROJECTION FEEDBACK COLOR
         SpatialMaterial ballMaterial = projectionBall.GetSurfaceMaterial(0) as SpatialMaterial;
         SpatialMaterial lineMaterial = projectionMesh.GetSurfaceMaterial(0) as SpatialMaterial;
         ballMaterial.AlbedoColor = new Color(1, 1, 1, .8f);
         // lineMaterial.AlbedoColor = new Color(1, 1, 1, .8f);

         if (resultData.curTargetProj != null)
         {
            if (resultData.curTargetProj is GenericBall && gameManager.yourBallCategory != BallCategory.UNDEFINED)
            {
               if (((GenericBall)resultData.curTargetProj).category == gameManager.yourBallCategory)
               {
                  ballMaterial.AlbedoColor = new Color(0, 0, 1, .8f);
                  // lineMaterial.AlbedoColor = new Color(0, 0, 1, .8f);
               }
               else
               {
                  ballMaterial.AlbedoColor = new Color(1, 0, 0, .8f);
                  // lineMaterial.AlbedoColor = new Color(1, 0, 0, .8f);
               }
            }
         }

         projectionBall.SetSurfaceMaterial(0, ballMaterial);

         // COLLIDED WITH ANY
         if (resultData.minDist != Mathf.Inf)
         {
            // PROJECTION NEXT LINE
            if (resultData.objCenter != Vector3.Zero && resultData.curTargetProj is FoodBall)
            {
               // IS A BALL
               FoodBall foodBall = (FoodBall)resultData.curTargetProj;
               if (gameManager.yourBallCategory != BallCategory.UNDEFINED && foodBall.category != gameManager.yourBallCategory)
               {
                  ChangeNextProjectionVisible(false);
                  return;
               }
               ChangeNextProjectionVisible(true);

               projectionNext.GlobalTranslation = resultData.objCenter;
               Transform projectionNextTrans = projectionNext.GlobalTransform;
               Vector3 predictionPoint = resultData.objCenter + resultData.objCenter.DirectionTo(projectionFinalPos) * -1 * 5;
               Basis lookAtBasisNext = projectionNextTrans.LookAtBasis(predictionPoint);
               projectionNextTrans.basis = lookAtBasisNext;
               projectionNext.GlobalTransform = projectionNextTrans;
               gameManager.PredictionVec = projectionNext.GlobalTransform.basis.z;
            }
            else
            {
               ChangeNextProjectionVisible(false);
               // IS TABLE
               projectionNext.GlobalTranslation = projectionFinalPos;
               Transform projectionNextTrans = projectionNext.GlobalTransform;
               Vector3 normal = projectionFinalPos + resultData.minNormal * 10;
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
         else
         {
            ChangeNextProjectionVisible(false);
         }
      }

      public void ChangeNextProjectionVisible(bool visible)
      {
         projectionNext.Visible = visible;
         projectionArrowNext.Visible = visible;
      }

      public ProjectionResultData GetProjetionResult()
      {
         ProjectionResultData resultData = new ProjectionResultData();
         resultData.minDist = Mathf.Inf;
         resultData.minPosCollided = Vector3.Zero;
         resultData.minNormal = Vector3.Zero;
         resultData.objCenter = Vector3.Zero;
         resultData.curTargetProj = null;

         foreach (RayCast ray in rays)
         {
            if (ray.IsColliding())
            {
               Vector3 position = ray.GetCollisionPoint();
               Vector3 normal = ray.GetCollisionNormal();
               Spatial node = (Spatial)ray.GetCollider();
               float distance = ray.GlobalTranslation.DistanceTo(position);

               if (resultData.minDist > distance)
               {
                  resultData.minPosCollided = position;
                  resultData.minDist = distance;
                  resultData.minNormal = normal;
                  resultData.objCenter = ((Spatial)node).GlobalTranslation;
                  resultData.curTargetProj = node;
               }
            }
         }

         return resultData;
      }

      public ProjectionResultData GetManualProjetionResult()
      {
         Vector3 whiteBallPos = whiteBall.GlobalTransform.origin;
         PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;
         Vector3 forward = GlobalTransform.basis.z.Normalized();
         Vector3 forwardRay = (forward * 1000);

         Vector3 fromTop = whiteBallPos + new Vector3(-ballRadius / 2, 0, 0);
         Vector3 toTop = forwardRay + new Vector3(0, ballRadius, 0);
         var resultTop = spaceState.IntersectRay(fromTop, toTop, null, projectionMask);
         var posTop = ManualProjectionRay(resultTop, debugBallTop, fromTop);

         Vector3 fromBottom = whiteBallPos + new Vector3(ballRadius / 2, 0, 0);
         Vector3 toBottom = forwardRay + new Vector3(0, -ballRadius, 0);
         var resultBottom = spaceState.IntersectRay(fromBottom, toBottom, null, projectionMask);
         var posBottom = ManualProjectionRay(resultBottom, debugBallBottom, fromBottom);

         Vector3 fromRight = whiteBallPos + new Vector3(-ballRadius, 0, 0);
         Vector3 toRight = forwardRay + new Vector3(-ballRadius, 0, 0);
         var resultRight = spaceState.IntersectRay(fromRight, toRight, null, projectionMask);
         var posRight = ManualProjectionRay(resultRight, debugBallRight, fromRight);

         Vector3 fromLeft = whiteBallPos + new Vector3(ballRadius, 0, 0);
         Vector3 toLeft = forwardRay + new Vector3(ballRadius, 0, 0);
         var resultLeft = spaceState.IntersectRay(fromLeft, toLeft, null, projectionMask);
         var posLeft = ManualProjectionRay(resultLeft, debugBallLeft, fromLeft);

         Vector3 fromMiddle = whiteBallPos;
         Vector3 toMiddle = forwardRay;
         var resultMiddle = spaceState.IntersectRay(fromMiddle, toMiddle, null, projectionMask);
         var posMiddle = ManualProjectionRay(resultMiddle, debugBallMiddle, fromMiddle);

         ProjectionResultData resultData = new ProjectionResultData();
         resultData.minDist = Mathf.Inf;
         resultData.minPosCollided = Vector3.Zero;
         resultData.minNormal = Vector3.Zero;
         resultData.objCenter = Vector3.Zero;
         resultData.curTargetProj = null;

         if (posTop != null)
         {
            var posT = (Vector3)posTop[0];
            float distT = posT.DistanceTo(whiteBallPos);
            if (resultData.minDist > distT)
            {
               resultData.minPosCollided = posT;
               resultData.minDist = distT;
               resultData.minNormal = (Vector3)posTop[1];
               resultData.objCenter = (Vector3)posTop[2];
               resultData.curTargetProj = (Node)posTop[3];
            }
         }
         if (posBottom != null)
         {
            var posB = (Vector3)posBottom[0];
            float distB = posB.DistanceTo(whiteBallPos);
            if (resultData.minDist > distB)
            {
               resultData.minPosCollided = posB;
               resultData.minDist = distB;
               resultData.minNormal = (Vector3)posBottom[1];
               resultData.objCenter = (Vector3)posBottom[2];
               resultData.curTargetProj = (Node)posBottom[3];
            }
         }
         if (posRight != null)
         {
            var posR = (Vector3)posRight[0];
            float distR = posR.DistanceTo(whiteBallPos);
            if (resultData.minDist > distR)
            {
               resultData.minPosCollided = posR;
               resultData.minDist = distR;
               resultData.minNormal = (Vector3)posRight[1];
               resultData.objCenter = (Vector3)posRight[2];
               resultData.curTargetProj = (Node)posRight[3];
            }
         }
         if (posLeft != null)
         {
            var posL = (Vector3)posLeft[0];
            float distL = posL.DistanceTo(whiteBallPos);
            if (resultData.minDist > distL)
            {
               resultData.minPosCollided = posL;
               resultData.minDist = distL;
               resultData.minNormal = (Vector3)posLeft[1];
               resultData.objCenter = (Vector3)posLeft[2];
               resultData.curTargetProj = (Node)posLeft[3];
            }
         }
         if (posMiddle != null)
         {
            var posM = (Vector3)posMiddle[0];
            float distM = posM.DistanceTo(whiteBallPos);
            if (resultData.minDist > distM)
            {
               resultData.minPosCollided = posM;
               resultData.minDist = distM;
               resultData.minNormal = (Vector3)posMiddle[1];
               resultData.objCenter = (Vector3)posMiddle[2];
               resultData.curTargetProj = (Node)posMiddle[3];
            }
         }

         return resultData;
      }

      public object[] ManualProjectionRay(Godot.Collections.Dictionary result, Spatial miniBall, Vector3 from)
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
            return new object[] { position, normal, objCenter, collider };
         }
         return null;
      }

      public async void Shot()
      {
         if (initialPos == null || waitingFinishTurn) return;
         canShot = true;
         shotPos = areaDetector.Translation;
         SceneTreeTween tween = GetTree().CreateTween();
         PropertyTweener tweener = tween.TweenProperty(areaDetector, "translation", initialPos, .2f).SetTrans(Tween.TransitionType.Circ);
      }

      public void ApplyImpulse()
      {
         if (canShot)
         {
            var distanceAvg = DistanceFromInitial((Vector3)shotPos);
            whiteBall.AddForce(GlobalTransform.basis.z.Normalized() * (maxForce * distanceAvg) * 60, new Vector3(0, .05f, 0));
            audioManager.Play("cue_in_whiteball", null, whiteBall.GlobalTranslation);
            canShot = false;
            shotPos = null;
            WaitingFinishTurn();
         }
      }

      public async void WaitingFinishTurn()
      {
         isYourTurn = false; // TODO: definido pelo timer de turn apenas
         await ToSignal(GetTree().CreateTimer(waitingTimeToFinishTurn), "timeout");
         waitingFinishTurn = true;
      }
   }
}