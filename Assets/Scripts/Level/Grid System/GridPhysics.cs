
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Utils;
using PrimeTween;
using UnityEngine;

namespace Game.Level
{
    using Debug = Utils.Logger.Debug;
    public class GridPhysics
    {
        private const float ALPHA_DURATION = 0.3f;
        private const float MOVEMENT_DURATION = 0.2f;
        private const float DELAY_PER_ROW = 0.005f;


        #region Physical Search Settings
        public const PhysicsSearchSettings DefaultSearch = PhysicsSearchSettings.Gravity | PhysicsSearchSettings.Respawned;
        public const PhysicsSearchSettings AdvancedSearch = PhysicsSearchSettings.Gravity | PhysicsSearchSettings.Bottom | PhysicsSearchSettings.LeftoverBeforeRespawn | PhysicsSearchSettings.Respawned;

        [Flags]
        public enum PhysicsSearchSettings
        {
            Gravity = 2, //look for cells that affected by gravity
            Bottom = 4, //look for bottom cells which is not affected by gravity
            LeftoverBeforeRespawn = 8,//look for leftover cells before respawn
            Respawned = 16, //look for respawned cells
            IgnoreMinMaxAfterRespawn = 32 //ignore min max after respawn (useful for multidirectional fireworks)
        }
        #endregion

        //key is column index, value is the lowest row index
        private Dictionary<int, ColumnUpdateData> _columnUpdate = null;
        private HashSet<GridCell> _searchingGroup = new HashSet<GridCell>();

        private static Grid _grid => GameManager.instance.grid;
        private static GridCell[,] _cells => _grid.cells;

        public GridPhysics()
        {
            _columnUpdate = new Dictionary<int, ColumnUpdateData>(_grid.width);
        }




        /// <summary>
        /// Apply gravity to the blasted group's column
        /// </summary>
        /// <param name="blastedGroup"> The group of cells that will be blasted (Goes To Top of The Grid)</param>
        /// <param name="searchBehaviour"> The behaviour of the searching affected cells</param>
        /// <returns> The group of cells that affected by gravity</returns>

        //would be better if IJobParallelFor to calculate grid...
        public HashSet<GridCell> ApplyGravity(HashSet<GridCell> blastedGroup, PhysicsSearchSettings searchBehaviour = DefaultSearch)
        {

            FindAffectedColumns(blastedGroup, out int minX, out int maxX);
            MoveBottomAffectedCells(minX, maxX, searchBehaviour);
            RespawnBlastedCellsAfterGravity(blastedGroup, minX, maxX, searchBehaviour);

            return _searchingGroup;
        }


        #region Gravity Related Methods
        private void FindAffectedColumns(HashSet<GridCell> blastedGroup, out int minX, out int maxX)
        {
            _columnUpdate.Clear();
            _searchingGroup.Clear();

            minX = _grid.width;
            maxX = 0;

            foreach (var blastedCell in blastedGroup)
            {
                var x = blastedCell.gridPosition.x;
                var y = blastedCell.gridPosition.y;

                if (!_columnUpdate.ContainsKey(x))
                {
                    _columnUpdate[x] = new ColumnUpdateData { lowestRow = y, totalEmptyCells = 1 };
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
                else
                {
                    var data = _columnUpdate[x];
                    if (data.lowestRow < y) data.lowestRow = y;
                    data.totalEmptyCells++;
                    _columnUpdate[x] = data;
                }

                blastedCell.image.color = CachedColors.whiteNoAlpha;
                _grid.cells[x, y] = null; //remove the cell from the grid it cached in blastedGroup
            }
        }

        private void RespawnBlastedCellsAfterGravity(HashSet<GridCell> blastedGroup, int minX, int maxX, PhysicsSearchSettings searchBehaviour = DefaultSearch)
        {

            foreach (var column in _columnUpdate)
            {
                var x = column.Key;
                var columnData = column.Value;
                int totalEmptyCells = columnData.totalEmptyCells;
                int totalRespawned = 0;

                foreach (var blastedCell in blastedGroup)
                {
                    if (blastedCell.gridPosition.x == x)
                    {
                        if ((searchBehaviour & PhysicsSearchSettings.LeftoverBeforeRespawn) != 0)
                        {
                            Vector2Int oldPosition = blastedCell.gridPosition;
                            if (oldPosition.x == minX || (searchBehaviour & PhysicsSearchSettings.IgnoreMinMaxAfterRespawn) != 0) FindLeftNeighbor(oldPosition);
                            if (oldPosition.x == maxX || (searchBehaviour & PhysicsSearchSettings.IgnoreMinMaxAfterRespawn) != 0) FindRightNeighbor(oldPosition);
                        }

                        RespawnCell(blastedCell, totalEmptyCells - totalRespawned - 1);
                        totalRespawned++;

                        if ((searchBehaviour & PhysicsSearchSettings.Respawned) != 0)
                        {
                            AddCellToSearchingGroup(blastedCell);
                        }
                    }

                    if (totalRespawned >= totalEmptyCells) break;
                }

            }
        }

        private void MoveBottomAffectedCells(int minX, float maxX, PhysicsSearchSettings searchBehaviour = DefaultSearch)
        {
            //Move to the bottom the cells that are not blasted 
            foreach (var column in _columnUpdate)
            {
                var x = column.Key;
                var columnData = column.Value;
                GridCell lastMovedCell = null;

                if ((searchBehaviour & PhysicsSearchSettings.Bottom) != 0 && columnData.lowestRow + 1 < _grid.height) //bottom left over cells
                {
                    AddCellToSearchingGroup(_cells[x, columnData.lowestRow + 1]);
                }

                for (int i = columnData.lowestRow - 1; i >= 0; i--)
                {

                    var cell = _cells[x, i];
                    if (cell == null) continue;

                    bool _wasMoved = false;
                    bool _wasFinished = false;
                    Vector2Int oldPosition = cell.gridPosition;

                    while (i + 1 < _grid.height)
                    {
                        var bottomCell = _cells[x, i + 1];
                        if (bottomCell == null)
                        {
                            if ((i + 2 < _grid.height && _cells[x, i + 2] != null) || i + 2 >= _grid.height) _wasFinished = true;
                            _wasMoved = true;
                            MoveCell(cell, i + 1);
                            AddCellToSearchingGroup(cell, lastMovedCell);
                            lastMovedCell = cell;
                            i++;
                        }
                        else break;
                    }

                    if (_wasFinished) AnimateCellMoving(cell);

                    if (_wasMoved && (searchBehaviour & PhysicsSearchSettings.Gravity) != 0)
                    {
                        if (oldPosition.x == minX) FindLeftNeighbor(oldPosition);
                        if (oldPosition.x == maxX) FindRightNeighbor(oldPosition);
                    }
                }
            }
        }

        #endregion

        public void CheckCellAbove(GridCell cell)
        {
            var position = cell.gridPosition;

            if (position.y > 0 && position.y < _grid.height && !_columnUpdate.ContainsKey(cell.gridPosition.x))
            {
                for (int y = position.y; y >= 0; y--)
                {
                    var UpperCell = _cells[position.x, y];
                    if (UpperCell != null)
                    {
                        AddCellToSearchingGroup(UpperCell);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCellToSearchingGroup(GridCell cell, GridCell otherCell)
        {
            if ((otherCell != null && otherCell.item != null && cell.item != null && otherCell.item.type != cell.item.type) || otherCell == null) _searchingGroup.Add(cell);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCellToSearchingGroup(GridCell cell)
        {
            if (cell != null && cell.item != null)
            {
                _searchingGroup.Add(cell);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindLeftNeighbor(Vector2Int position)
        {
            position.x--;
            if (_grid.searcher.IsValidPosition(position))
            {
                AddCellToSearchingGroup(_cells[position.x, position.y]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindRightNeighbor(Vector2Int position)
        {
            position.x++;
            if (_grid.searcher.IsValidPosition(position))
            {
                AddCellToSearchingGroup(_cells[position.x, position.y]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveCell(GridCell cell, int targetY) //move cell to targetY
        {
            _grid.cells[cell.gridPosition.x, cell.gridPosition.y] = null;
            cell.gridPosition = new Vector2Int(cell.gridPosition.x, targetY);
            _grid.cells[cell.gridPosition.x, targetY] = cell;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AnimateCellMoving(GridCell cell) => Tween.UIAnchoredPositionY(cell.rectTransform, -cell.gridPosition.y * _grid.spacing, MOVEMENT_DURATION);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RespawnCell(GridCell cell, int targetY) //respawn cell to targetY
        {
            cell.gridPosition = new Vector2Int(cell.gridPosition.x, targetY);
            _grid.cells[cell.gridPosition.x, targetY] = cell;
            cell.groupRoot = null;

            Sequence spawnSequence = Sequence.Create();
            spawnSequence.Group(Tween.Alpha(cell.image, 1, ALPHA_DURATION));
            spawnSequence.Group(Tween.UIAnchoredPositionY(cell.rectTransform, 60, -targetY * _grid.spacing, MOVEMENT_DURATION, startDelay: (_grid.height - targetY) * DELAY_PER_ROW));
        }

        struct ColumnUpdateData
        {
            public int lowestRow;
            public int totalEmptyCells;
        }

    }

}