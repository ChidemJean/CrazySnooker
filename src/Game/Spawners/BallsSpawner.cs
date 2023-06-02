using Godot;
using System;
using System.Collections.Generic;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Utils;

namespace CrazySnooker.Game.Spawners
{
    public class BallsSpawner : Spatial
    {
        [Export]
        private NodePath[] spawnPoints;

        [Export]
        private Color[] colors;

        [Export]
        private int qtdBalls;

        [Export]
        private PackedScene ballScene;

        private List<Position3D> positions = new List<Position3D>();

        public override void _Ready()
        {
            foreach (var pointPath in spawnPoints) 
            {
                positions.Add(GetNode<Position3D>(pointPath));
            }
            Spawn();
        }

        public void Spawn()
        {
            if (ballScene == null) return;

            for (int i = 0; i < qtdBalls; i++) 
            {
                var ballNode = ballScene.Instance<GenericBall>();
                AddChild(ballNode);
                var position = positions[MathUtils.RandiRange(0, positions.Count - 1)];
                ballNode.GlobalTranslation = position.GlobalTransform.origin;
                // int rndIndexColor = MathUtils.RandiRange(0, colors.Length - 1);
                // ballNode.ChangeColor(colors[rndIndexColor]);
            }
        }

    }
}
