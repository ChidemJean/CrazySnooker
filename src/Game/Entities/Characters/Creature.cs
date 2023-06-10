using Godot;
using System;
using CrazySnooker.Game.Managers;
using CrazySnooker.Global;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Game.Particles;
using CrazySnooker.Game.Ui;
using CrazySnooker.Events;

namespace CrazySnooker.Game.Entities.Characters
{
   public class Creature : Spatial
   {
      [Export]
      private NodePath detectorPath;
      private Area detector;

      [Export]
      private NodePath animationTreePath;
      private AnimationTree animationTree;

      [Export]
      private NodePath animationPlayerPath;
      private AnimationPlayer animationPlayer;

      [Export]
      private NodePath viewPath;
      private Area view;

      [Export]
      private float waitPrepareEat = 10f;

      [Export]
      private float particlesTime = 1.6f;

      [Export]
      private float maxCalories = 100f;

      private float calories = 0;

      private GameManager gameManager;
      private GlobalEvents globalEvents;
      private AudioManager audioManager;

      private bool died = false;

		[Export]
		private NodePath caloriesBarPath;
		private CaloriesBar caloriesBar;

      public override void _Ready()
      {
			caloriesBar = GetNode<CaloriesBar>(caloriesBarPath);
         view = GetNode<Area>(viewPath);
         detector = GetNode<Area>(detectorPath);
         animationTree = GetNode<AnimationTree>(animationTreePath);
         animationPlayer = GetNode<AnimationPlayer>(animationPlayerPath);
         gameManager = GetNode<GameManager>("%GameManager");
         globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
         audioManager = GetNode<AudioManager>("/root/MainScene/AudioManager");

         detector.Connect("body_entered", this, nameof(OnDetectorEntered));
         view.Connect("body_entered", this, nameof(OnViewEntered));
      }

      public void OnDetectorEntered(Node node)
      {
         if (died) return;
         if (node is WhiteBall)
         {
            gameManager.EmitResetWhiteBall();
            Hit();
            return;
         }
         if (node is FoodBall)
         {
            FoodBall foodBall = (FoodBall) node;
            Eat(foodBall);
            gameManager.AddPocketedBall(foodBall);
         }
      }

      public void OnViewEntered(Node node)
      {
         if (died) return;
         PrepareEat((GenericBall)node);
      }

      public void PrepareEat(GenericBall ball, bool makeAnim = true)
      {
         if (makeAnim)
         {
            GetTree().CreateTween().TweenProperty(animationTree, "parameters/PrepareEat/add_amount", 1, .4f);
         }
         CloseMouth();
      }

      public void Hit()
      {
         var tween = GetTree().CreateTween();
         tween.TweenProperty(animationTree, "parameters/Hit/blend_amount", 1, .12f);
         tween.TweenProperty(animationTree, "parameters/Hit/blend_amount", 0, .26f);
      }

      public void Eat(FoodBall ball)
      {
         ball.QueueFree();
         audioManager.Play("crunch", null, GlobalTranslation);
         CloseMouth();
         animationTree.Set("parameters/Eat/active", true);
         UpdateCalories(ball.calories);
      }

      public void UpdateCalories(float calories)
      {
         this.calories = Mathf.Clamp(this.calories + calories, 0f, maxCalories);
			float perc = this.calories / maxCalories;
         var tween = GetTree().CreateTween();
         tween.TweenProperty(animationTree, "parameters/Fatten/add_amount", perc, .1f);
         if (this.calories >= maxCalories)
         {
            Die();
				return;
         }
			caloriesBar.ProgressValue = perc;
      }

      public async void Die()
      {
         if (died) return;
         died = true;
         var tween = GetTree().CreateTween();
         tween.TweenProperty(animationTree, "parameters/Hit/blend_amount", 1, .08f);
         MakeExplosion();
         await ToSignal(tween, "finished");
         QueueFree();
      }

      public void MakeExplosion()
      {
         gameManager.EmitParticleInPosition("explosion", GlobalTranslation);
         globalEvents.EmitSignal(GameEvent.ExplosionHappened, GlobalTranslation);
      }

      public async void CloseMouth()
      {
         await ToSignal(GetTree().CreateTimer(waitPrepareEat), "timeout");
         if (view.GetOverlappingBodies().Count > 0)
         {
            CloseMouth();
            return;
         }
         GetTree().CreateTween().TweenProperty(animationTree, "parameters/PrepareEat/add_amount", 0, .55f);
      }
   }
}