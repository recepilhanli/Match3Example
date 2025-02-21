using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using static Game.Level.ParticleEffectManager;

namespace Game.Level
{
    public class GridGroups
    {
        private static Grid _grid => GameManager.instance.grid;
        private static GridCell[,] _cells => _grid.cells;

        private Dictionary<GridCell, int> _cellGroups = null; //root cell and group size
        public IReadOnlyDictionary<GridCell, int> cellGroups => _cellGroups;
        private HashSet<GridCell> _wasAssigned = null; //for assigning stages


        public GridGroups()
        {
            int totalCell = _grid.width * _grid.height;
            _cellGroups = new Dictionary<GridCell, int>(totalCell / 2);

            _wasAssigned = new HashSet<GridCell>(totalCell);
        }



        public void MarkRoots()
        {
            Vector3 shakeStrength = new Vector3(.25f, .25f, .25f);
            Sequence markSequence = Sequence.Create();

            foreach (var rootCell in _cellGroups.Keys)
            {
                markSequence.Group(Tween.ShakeScale(rootCell.transform, shakeStrength, .5f, 6, easeBetweenShakes: Ease.InOutSine));
                markSequence.Group(Tween.Alpha(rootCell.image, .75f, 1, .3f, Ease.InOutSine));
                ParticleEffectManager.instance.SpawnEffect(ParticleEffectType.HighlightingStar, rootCell.transform.position);
            }
        }

        public void MarkRoots(int max)
        {
            if (_cellGroups.Count == 0 || max <= 0) return; //there are only explosives..
            else if (max > _cellGroups.Count) max = _cellGroups.Count;

            int totalMarked = 0;

            Vector3 shakeStrength = new Vector3(.4f, .4f, .4f);
            Sequence markSequence = Sequence.Create();

            foreach (var rootCell in _cellGroups.Keys)
            {
                markSequence.Group(Tween.ShakeScale(rootCell.transform, shakeStrength, .3f, 12, easeBetweenShakes: Ease.InOutSine));
                markSequence.Group(Tween.Alpha(rootCell.image, .75f, 1, .3f, Ease.InOutSine));
                ParticleEffectManager.instance.SpawnEffect(ParticleEffectType.HighlightingStar, rootCell.transform.position);
                totalMarked++;
                if (totalMarked >= max) break;
            }
        }


        public int GetRootStage(GridCell root)
        {

            if (root != null && _cellGroups.ContainsKey(root))
            {
                int stage = GameManager.instance.coloredItemStages.FindCurrentStageOfGroup(_cellGroups[root]);
                return stage;
            }

            return StageInfo.STAGE_DEFAULT;
        }


        public void LookForStages(HashSet<GridCell> _searchingGroup)
        {
            _wasAssigned.Clear();

            foreach (var cell in _searchingGroup)
            {
                if (cell == null || _wasAssigned.Contains(cell)) continue;

                if (cell.item == null)
                {
                    Debug.Log($"<color=red>Item is null: {cell.name}</color>");
                    continue;
                }

                var group = _grid.searcher.FindRelatedCells(cell.gridPosition);
                ReasignGroupStage(group, cell.item.type, cell);
                AddGroupToWasSearched(group);
            }

        }

        private void AddGroupToWasSearched(HashSet<GridCell> group) //unionwith created too much garbage....
        {
            foreach (var cell in group)
            {
                _wasAssigned.Add(cell);
            }
        }

        /// <summary>
        /// Looks for the stages of all cells in the grid
        /// </summary>
        public void OrganiseGrid()
        {
            _cellGroups.Clear();
            _wasAssigned.Clear();


            for (int x = 0; x < _grid.width; x++)
            {
                for (int y = 0; y < _grid.height; y++)
                {
                    var cell = _cells[x, y];
                    if (cell == null || _wasAssigned.Contains(cell)) continue;

                    var group = _grid.searcher.FindRelatedCells(cell.gridPosition);
                    if (group.Count < 1) continue;
                    ReasignGroupStage(group, cell.item.type, cell);
                    _wasAssigned.UnionWith(group);
                }
            }
        }

        public void ChangeGroupRootOfCell(GridCell cell, GridCell newRoot)
        {
            if (cell.groupRoot == cell && newRoot != cell) RemoveGroup(cell);
            cell.groupRoot = newRoot;
        }

        public void RemoveGroup(GridCell root)
        {
            if (root != null) _cellGroups.Remove(root);
        }

        public void RemoveGroupPersistent(GridCell root) => _cellGroups.Remove(root);

        private void ReasignGroupStage(HashSet<GridCell> group, BlastableType type, GridCell root)
        {
            if (GameManager.IsNonConnectable(type))
            {
                RemoveGroup(root);
                return;
            }

            if (group.Count > 0)
            {
                var stageInfo = GameManager.instance.coloredItemStages;
                int stage = stageInfo.FindCurrentStageOfGroup(group.Count);
                var sprite = stageInfo.FindColorStage(type, stage);

                if (group.Count > 1)
                {
                    if (root == null) Debug.LogError("Root is null");
                    if (!_cellGroups.ContainsKey(root)) _cellGroups.Add(root, group.Count);
                    else _cellGroups[root] = group.Count;


                    foreach (var cell in group)
                    {
                        cell.image.sprite = sprite;
                        ChangeGroupRootOfCell(cell, root);
                    }
                }
                else
                {
                    var singleCell = GetFirstCellOfGroup(group);
                    singleCell.image.sprite = sprite;
                    root = singleCell.groupRoot;
                    singleCell.groupRoot = null;
                    if (root == singleCell) _cellGroups.Remove(root);
                }

            }

        }

        private GridCell GetFirstCellOfGroup(HashSet<GridCell> group) //zero allocation
        {
            foreach (var cell in group)
            {
                return cell;
            }
            return null;
        }

    }
}