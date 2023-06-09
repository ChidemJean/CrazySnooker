using Godot;
using System;

namespace CrazySnooker.Game.Ui
{

   public class CaloriesBar : MeshInstance
   {
      [Export]
      private NodePath holderPath;

      private Control holder;

      [Export]
      private NodePath progressPath;

      private ProgressBar progressBar;

      [Export]
      public float maxYaw = 25.0f;
      [Export]
      public float maxPitch = 25.0f;
      [Export]
      public float maxRoll = 25.0f;
      [Export]
      public float shakeReduction = 1.0f;
      [Export]
      private float minValueToShake = .4f;
      private float stress = 0.0f;
      private float shake = 0.0f;
      private Vector3 rotationReset = new Vector3();
		private SpatialMaterial material;
		[Export]
      public NodePath viewportPath;
      public Viewport viewport;

      public double ProgressValue
      {
         set
         {
            progressBar.Value = value;

            if (value > 0 && !Visible)
            {
               Visible = true;
               AddStress(4f);
            }
            if (value >= minValueToShake)
            {
               AddStress(10f * ((float)value));
            }
         }
         get
         {
            return progressBar.Value;
         }
      }

      public override void _Ready()
      {
         progressBar = GetNode<ProgressBar>(progressPath);
         holder = GetNode<Control>(holderPath);
			viewport = GetNode<Viewport>(viewportPath);
			material = MaterialOverride as SpatialMaterial;
      }

      public override void _Process(float delta)
      {
			material.AlbedoTexture = viewport.GetTexture();
         if (stress == 0.0f)
         {
            rotationReset = RotationDegrees;
         }

         RotationDegrees = ProcessShake(rotationReset, delta);
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