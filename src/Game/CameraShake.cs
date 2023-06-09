using Godot;
using System;
using CrazySnooker.Events;

namespace CrazySnooker.Game
{

   public class CameraShake : Node
   {
      [Export]
      public float maxYaw = 25.0f;
      [Export]
      public float maxPitch = 25.0f;
      [Export]
      public float maxRoll = 25.0f;
      [Export]
      public float shakeReduction = 1.0f;

		private Camera camera;
      private float stress = 0.0f;
      private float shake = 0.0f;

      private Vector3 cameraRotationReset = new Vector3();

      private GlobalEvents globalEvents;

      public override void _Ready()
      {
         globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
         camera = GetParent<Camera>();
         globalEvents.Connect(GameEvent.ExplosionHappened, this, nameof(OnExplosionHappened));
      }

      public void OnExplosionHappened(Vector3 position)
      {
         AddStress(6f / (camera.GlobalTranslation.DistanceTo(position) * 2));
      }

      // TODO: Add in some sort of rotation reset.
      public override void _Process(float delta)
      {
         if (stress == 0.0f)
         {
            cameraRotationReset = camera.RotationDegrees;
         }

         camera.RotationDegrees = ProcessShake(cameraRotationReset, delta);
      }

      private Vector3 ProcessShake(Vector3 angleCenter, float delta)
      {
         shake = stress * stress;

         stress -= shakeReduction / 100.0f;
         stress = Mathf.Clamp(stress, 0.0f, 1.0f);

         var newRotate = Vector3.Zero;
         newRotate.x = maxYaw * shake * GetNoise(new Random().Next(), delta);
         newRotate.y = maxPitch * shake * GetNoise(new Random().Next(), delta + 1.0f);
         newRotate.z = maxRoll * shake * GetNoise(new Random().Next(), delta + 2.0f);

         return angleCenter + newRotate;
      }

      private float GetNoise(float noiseSeed, float time)
      {
         var n = new OpenSimplexNoise();

         n.Seed = (int)noiseSeed;
         n.Octaves = 4;
         n.Period = 20.0f;
         n.Persistence = 0.8f;

         return n.GetNoise1d(time);
      }

      public void AddStress(float amount)
      {
         stress += amount;
         stress = Mathf.Clamp(stress, 0.0f, 1.0f);
      }
   }
}