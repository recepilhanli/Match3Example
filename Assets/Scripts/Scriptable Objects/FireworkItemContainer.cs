using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Level;
using Game.UI;
using Game.Utils;
using PrimeTween;
using UnityEngine;


namespace Game.ScriptableObjects
{
    using static Game.Level.GridPhysics;
    using Debug = Utils.Logger.Debug;

    [CreateAssetMenu(fileName = "NewFireWorkItem", menuName = "Game/Create a New Firework Item Container")]
    public class FireworkItemContainer : ABlastableContainer
    {

        #region Physical Search Settings For Each Firework Type
        private const PhysicsSearchSettings FireWorkVerticalSearch = PhysicsSearchSettings.Gravity | PhysicsSearchSettings.Respawned | PhysicsSearchSettings.LeftoverBeforeRespawn;
        private const PhysicsSearchSettings FireWorkHorizontalSearch = PhysicsSearchSettings.Bottom | PhysicsSearchSettings.Respawned;
        private const PhysicsSearchSettings FireworkBothDirectionsSearch = FireWorkVerticalSearch | FireWorkHorizontalSearch | PhysicsSearchSettings.IgnoreMinMaxAfterRespawn;
        #endregion

        [Header("Firework Settings")]
        [SerializeField] private SearchDirection _fireworkType = SearchDirection.Both;
        [SerializeField, Tooltip("Range of the firework (0 is all grid sizes)")] private int range = 0;

        public static List<GridCell> combinedFireworks = new List<GridCell>(5);

        public override void Blast(GridCell cell)
        {
            int searchRange = range;
            SearchDirection direction = _fireworkType;
            if (range <= 0 && _fireworkType == SearchDirection.Both) searchRange = _grid.height * _grid.width;
            else if (range <= 0 && _fireworkType == SearchDirection.Horizontal) searchRange = _grid.width;
            else if (range <= 0 && _fireworkType == SearchDirection.Vertical) searchRange = _grid.height;

            var foundGroup = _grid.searcher.FindCellsDirectional(cell.gridPosition, 2, SearchDirection.Both); //look for other explosives

            if (foundGroup.Count > 1)
            {
                BombItemContainer foundBomb = null;
                foreach (var foundCell in foundGroup)
                {
                    if (foundCell == cell) continue;
                    if (foundCell.item.type == BlastableType.Bomb)
                    {
                        foundBomb = foundCell.item as BombItemContainer;
                        break;
                    }
                    else if (foundCell.item.type == BlastableType.Firework)
                    {
                        combinedFireworks.Add(foundCell);
                        direction = SearchDirection.Both;
                        if (range != 0) range++;
                    }
                }

                if (foundBomb != null)
                {
                    foundBomb.Blast(cell);
                    return;
                }
            }


            foundGroup = _grid.searcher.FindCellsDirectional(cell.gridPosition, searchRange, direction);
            foundGroup.Add(cell);
            ExplodeCells(foundGroup, cell, direction).Forget();
        }

        private static async UniTaskVoid SetOldPosXDelayed(RectTransform rectTransform, float delay)
        {
            float x = rectTransform.anchoredPosition.x;
            await UniTask.WaitForSeconds(delay);
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        }

        private static async UniTaskVoid ExplodeCells(HashSet<GridCell> blastedGroup, GridCell fireworkRoot, SearchDirection direction)
        {
            _grid.DisableCells();

            ParticleEffectManager.instance.SpawnEffect(fireworkRoot.item.type, fireworkRoot.transform.position);
            _ = GameUI.instance.BackgroundColorHueShift();


            if (combinedFireworks.Count > 0)
            {
                Sequence combineSequence = Sequence.Create();

                foreach (var firework in combinedFireworks)
                {
                    SetOldPosXDelayed(firework.rectTransform, .12f).Forget();
                    Tween combineTween = Tween.UIAnchoredPosition(firework.rectTransform, fireworkRoot.rectTransform.anchoredPosition, .085f, ease: Ease.InOutSine);
                    _ = combineSequence.Group(combineTween);
                }

                combinedFireworks.Clear();

                await combineSequence;

                SoundManager.instance.PlaySound(SoundManager.sounds.mergeFirework, .65f);
                _ = GameManager.instance.ShakeLocationGrid(20);
            }
            else
            {
                SoundManager.instance.PlaySound(SoundManager.sounds.singleFirework, .65f);
                _ = GameManager.instance.ShakeRotationGrid(1.5f);
            }

            SoundManager.instance.PlaySound(SoundManager.sounds.blastFirework, .5f);

            var sequence = Sequence.Create();
            float startDelay = 0f;
            foreach (var blastedCell in blastedGroup)
            {

                if (blastedCell.groupRoot == blastedCell)
                {
                    _grid.groups.RemoveGroupPersistent(blastedCell);
                }

                blastedCell.SetItem(GameManager.instance.GetRandomContainer(), assignSprite: false);
                Tween punchTween = Tween.ShakeScale(blastedCell.rectTransform, new Vector3(.3f, .3f, .3f), .15f, startDelay: startDelay, easeBetweenShakes: Ease.InOutElastic);
                _ = sequence.Group(punchTween);
                blastedCell.UpdateCellSpriteDelayed(.15f).Forget();
                startDelay += 0.008f;
            }

            await sequence;

            PhysicsSearchSettings searchSettings = FireWorkVerticalSearch;
            if (direction == SearchDirection.Horizontal) searchSettings = FireWorkHorizontalSearch;
            else if (direction == SearchDirection.Both) searchSettings = FireworkBothDirectionsSearch;

            var affectedGroup = _grid.physics.ApplyGravity(blastedGroup, searchSettings);
            _grid.groups.LookForStages(affectedGroup);

            GameManager.instance.CheckDeadEnd();
            _grid.EnableCells();
        }
    }
}

