
using System.Collections.Generic;
using Game.Level;
using Game.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    using Debug = Utils.Logger.Debug;

    //GameManager.Items.cs
    public partial class GameManager
    {

        [Header("Item Properties")]
        public StageInfo coloredItemStages = new StageInfo();
        public List<ColoredItemContainer> coloredContainers = new List<ColoredItemContainer>();
        public List<FireworkItemContainer> fireworkContainers = new List<FireworkItemContainer>();
        public List<BombItemContainer> bombContainers = new List<BombItemContainer>();


        public ABlastableContainer GetRandomContainer() => coloredContainers[Random.Range(0, coloredContainers.Count)];
        public void SetRandomColoredItem(GridCell cell) => cell.SetItem(GetRandomContainer());

        /// <summary>
        /// Assigns a new item for the group based on the stage
        /// </summary>
        /// <param name="stage">Current stage of the cell</param>
        /// <param name="cell"> The cell to assign the item to</param>
        /// <returns> True if an item was assigned, false otherwise</returns>
        public bool AssignNewItemForStage(int stage, GridCell cell)
        {
            if (stage == StageInfo.STAGE_DEFAULT) return false;

            if (stage >= StageInfo.STAGE_B && bombContainers.Count > 0)
            {
                cell.SetItem(bombContainers[Random.Range(0, bombContainers.Count)]);
                grid.totalExplosives++;
                return true;
            }
            else if (stage >= StageInfo.STAGE_A && fireworkContainers.Count > 0)
            {
                cell.SetItem(fireworkContainers[Random.Range(0, fireworkContainers.Count)]);
                grid.totalExplosives++;
                return true;
            }

            return false;
        }


        #region Item Comparisons

        public static bool IsNonConnectable(BlastableType type)
        {
            return type switch
            {
                BlastableType.Bomb => true,
                BlastableType.Firework => true,
                _ => false,
            };
        }

        public static bool IsExplosive(BlastableType type)
        {
            return type switch
            {
                BlastableType.Bomb => true,
                BlastableType.Firework => true,
                _ => false,
            };
        }

        #endregion

    }

}