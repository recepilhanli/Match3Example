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

    [CreateAssetMenu(fileName = "NewColoredItem", menuName = "Game/Create a New Item Colored Container")]
    public class ColoredItemContainer : ABlastableContainer
    {

        private const int MAX_BLASTED_TWEEN_COUNT = 24;

        public override void Blast(GridCell cell)
        {

            var blastedGroup = _grid.searcher.FindRelatedCells(cell.gridPosition);
            if (blastedGroup.Count > 1)
            {
                ParticleEffectManager.instance.SpawnEffect(cell.item.type, cell.transform.position);
                SoundManager.instance.PlaySound(SoundManager.sounds.blastColored);

                var groupRoot = cell.groupRoot;
                _grid.groups.RemoveGroup(groupRoot);

                BlastGroup(blastedGroup, cell).Forget();
            }
            else
            {
                Tween.ShakeScale(cell.rectTransform, new Vector3(.15f, .15f, .15f), .2f);
                Tween.Color(cell.image, CachedColors.whiteHalfAlpha, Color.white, .1f);
                SoundManager.instance.PlaySound(SoundManager.sounds.cantBlast, .2f);
            }
        }

        private static async UniTaskVoid BlastGroup(HashSet<GridCell> blastedGroup, GridCell blastRoot)
        {
            _grid.DisableCells();

            _ = GameUI.instance.SerializeTypeForBackgroundFade(blastRoot.item.type);
            _ = GameManager.instance.ShakeLocationGrid(blastedGroup.Count * .6f);

            int stage = GameManager.instance.coloredItemStages.FindCurrentStageOfGroup(blastedGroup.Count);
            bool newItemAssigned = GameManager.instance.AssignNewItemForStage(stage, blastRoot);

            if (newItemAssigned)
            {
                blastRoot.groupRoot = null;
                blastedGroup.Remove(blastRoot);
            }

            var sequence = Sequence.Create();

            //if (blastedGroup.Count >= GameManager.instance.coloredItemStages.stageThresholds[0]) Vibration

            int createdTweens = 0;

            foreach (var blastedCell in blastedGroup)
            {
                blastedCell.SetItem(GameManager.instance.GetRandomContainer(), assignSprite: false);

                if (createdTweens < MAX_BLASTED_TWEEN_COUNT)
                {
                    Tween punchTween = Tween.PunchScale(blastedCell.rectTransform, new Vector3(.25f, .25f, .25f), .15f, easeBetweenShakes: Ease.InOutBounce);
                    _ = sequence.Group(punchTween);
                    createdTweens++;
                }
                
                blastedCell.UpdateCellSpriteDelayed(.15f).Forget();
            }

            await sequence;

            HashSet<GridCell> affectedGroup = _grid.physics.ApplyGravity(blastedGroup);
            _grid.physics.CheckCellAbove(blastRoot);

            _grid.groups.LookForStages(affectedGroup);

            GameManager.instance.CheckDeadEnd();
            _grid.EnableCells();
        }
    }
}

