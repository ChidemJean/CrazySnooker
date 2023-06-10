using Godot;
using System;
using CrazySnooker.Game.Entities.Characters;
using CrazySnooker.Game.Managers;

namespace CrazySnooker.Game.Controllers
{
    public class BlockCueView : Area
    {
        private PoolCueController cueController;
        [Export]
        private Material material;

        private GameManager gameManager;

        public override void _Ready()
        {
            cueController = GetParent<PoolCueController>();
            gameManager = GetNode<GameManager>("../../GameManager");
            if (cueController.isRemote) return;
            Connect("body_entered", this, nameof(OnBodyEntered));
            Connect("body_exited", this, nameof(OnBodyExited));
        }

        public void OnBodyEntered(Node node)
        {
            if (node.Name == "CreatureBody" && material != null && gameManager.playerTurnId == cueController.playerID) {
                MeshInstance mesh = node.GetNode<MeshInstance>("../first_creature/Armature/Skeleton/body");
                mesh.MaterialOverride = material;
            }
        }

        public void OnBodyExited(Node node)
        {
            if (node.Name == "CreatureBody" && material != null) {
                MeshInstance mesh = node.GetNode<MeshInstance>("../first_creature/Armature/Skeleton/body");
                mesh.MaterialOverride = null;
            }
        }
    }
}