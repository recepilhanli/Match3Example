using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Level;
using Game.UI;
using Game.Utils;
using PrimeTween;
using UnityEngine;



namespace Game.ScriptableObjects
{

    using Debug = Utils.Logger.Debug;

    [CreateAssetMenu(fileName = "NewBombIterm", menuName = "Game/Create a New Bomb Item Container")]
    public class BombItemContainer : ABlastableContainer
    {
        [Header("Bomb Settings")]
        [SerializeField, Tooltip("Range of the explosion")] private int range = 1;

        public override void Blast(GridCell cell)
        {
            int explosionRange = range;
            var foundGroup = _grid.searcher.FindRelatedCellsDirectional(cell.gridPosition, 2, SearchDirection.Both); //look for other explosives

            foreach (var foundCell in foundGroup)
            {
                if (foundCell == cell) continue;
                explosionRange += 2;
            }

            GameManager.instance.ShakeLocationGrid();
            GameManager.instance.ShakeRotationGrid();

            foundGroup = _grid.searcher.FindCellsRangeBased(cell.gridPosition, explosionRange);
            foundGroup.Add(cell);
            ParticleEffectManager.instance.SpawnEffect(cell.item.type, cell.transform.position);
            SoundManager.instance.PlaySound(SoundManager.sounds.blastBomb, .4f);
            ExplodeCells(foundGroup).Forget();
        }


        private static async UniTaskVoid ExplodeCells(HashSet<GridCell> blastedGroup)
        {
            _grid.DisableCells();

            _= GameUI.instance.BackgroundColorFade(Color.black, .6f);

            var sequence = Sequence.Create();

            foreach (var blastedCell in blastedGroup)
            {
                if (blastedCell.groupRoot == blastedCell)
                {
                    _grid.groups.RemoveGroupPersistent(blastedCell);
                }

                blastedCell.SetItem(GameManager.instance.GetRandomContainer(), assignSprite: false);
                Tween punchTween = Tween.ShakeScale(blastedCell.rectTransform, new Vector3(.3f, .3f, .3f), .15f, easeBetweenShakes: Ease.InOutElastic);
                _ = sequence.Group(punchTween);
                blastedCell.UpdateCellSpriteDelayed(.15f).Forget();
            }
   
            await sequence;

            var affectedGroup = _grid.physics.ApplyGravity(blastedGroup, GridPhysics.AdvancedSearch);
            _grid.groups.LookForStages(affectedGroup);

            GameManager.instance.CheckDeadEnd();
            _grid.EnableCells();

        }



    }
}

