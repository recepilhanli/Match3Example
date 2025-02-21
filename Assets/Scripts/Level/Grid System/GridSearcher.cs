
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using PrimeTween;
using UnityEngine;

namespace Game.Level
{
    using static Game.Level.ParticleEffectManager;
    using Debug = Utils.Logger.Debug;
    public class GridSearcher
    {

 
        private HashSet<GridCell> _foundCells = null; //searching results

        private Queue<Vector2Int> _searchQueue = new Queue<Vector2Int>(); //for searching 
        private HashSet<Vector2Int> _visitedDirections = new HashSet<Vector2Int>(); //for searching
        private static readonly Vector2Int[] _searchDirections = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) }; //for searching

        private static Grid _grid => GameManager.instance.grid;
        private static GridCell[,] _cells => _grid.cells;

        public GridSearcher()
        {
            int totalCell = _grid.width * _grid.height;
            _foundCells = new HashSet<GridCell>(totalCell);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckCellItem(GridCell cell, BlastableType type)
        {
            return cell != null && cell.item != null && cell.item.type == type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _grid.width && pos.y >= 0 && pos.y < _grid.height;
        }


        #region  Searching Methods

        /// <summary>
        /// Find the cells in the given range from the given position
        /// </summary>
        /// <param name="position"> The position to start the search</param>
        /// <param name="range"> The range of the search</param>
        /// <param name="direction"> The direction of the search</param>
        /// <returns> The found cells</returns>
        public HashSet<GridCell> FindCellsDirectional(in Vector2Int position, int range, SearchDirection direction = SearchDirection.Both)
        {

            _foundCells.Clear();

            switch (direction)
            {

                case SearchDirection.Both:
                    {

                        if (_cells[position.x, position.y] != null) _foundCells.Add(_cells[position.x, position.y]);

                        for (int i = 1; i < range; i++)
                        {
                            foreach (var pos in _searchDirections)
                            {
                                var dir = new Vector2Int(position.x + pos.x * i, position.y + pos.y * i);
                                if (IsValidPosition(dir) && _cells[dir.x, dir.y] != null)
                                {
                                    _foundCells.Add(_cells[dir.x, dir.y]);
                                }
                            }
                        }


                        break;
                    }

                case SearchDirection.Horizontal:
                    {
                        int minX = position.x - range, maxX = position.x + range;
                        if (minX < 0) minX = 0;
                        if (maxX >= _grid.width) maxX = _grid.width - 1;

                        for (int x = minX; x <= maxX; x++)
                        {
                            if (!IsValidPosition(new Vector2Int(x, position.y))) continue;
                            var currentCell = _cells[x, position.y];
                            if (currentCell == null) continue;
                            _foundCells.Add(currentCell);
                        }
                        break;
                    }

                case SearchDirection.Vertical:
                    {
                        int minY = position.y - range, maxY = position.y + range;
                        if (minY < 0) minY = 0;
                        if (maxY >= _grid.height) maxY = _grid.height - 1;

                        for (int y = minY; y <= maxY; y++)
                        {
                            if (!IsValidPosition(new Vector2Int(position.x, y))) continue;
                            var currentCell = _cells[position.x, y];
                            if (currentCell == null) continue;
                            _foundCells.Add(currentCell);
                        }

                        break;
                    }
            }

            return _foundCells;
        }

        /// <summary>
        /// Find the cells in the given range from the given position
        /// </summary>
        /// <param name="position"> The position to start the search</param>
        /// <param name="range"> The range of the search</param>
        /// <returns> The found cells</returns>
        public HashSet<GridCell> FindCellsRangeBased(in Vector2Int position, int range)
        {

            _foundCells.Clear();

            int minX = position.x - range, maxX = position.x + range;
            int minY = position.y - range, maxY = position.y + range;

            if (minX < 0) minX = 0;
            if (maxX >= _grid.width) maxX = _grid.width - 1;
            if (minY < 0) minY = 0;
            if (maxY >= _grid.height) maxY = _grid.height - 1;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!IsValidPosition(new Vector2Int(x, y))) continue;
                    var currentCell = _cells[x, y];
                    if (currentCell == null) continue;
                    _foundCells.Add(currentCell);
                }
            }

            return _foundCells;
        }

        public HashSet<GridCell> FindRelatedCellsDirectional(in Vector2Int position, int range, SearchDirection direction = SearchDirection.Both)
        {

            _foundCells.Clear();
            var baseCell = _cells[position.x, position.y];

            if (baseCell == null)
            {
                Debug.LogError($"attempted to find related cells from empty cell: {position.x}, {position.y}");
                return null;
            }
            else if (baseCell.item == null)
            {
                Debug.LogError($"attempted to find related cells from empty item: {position.x}, {position.y}");
                return null;
            }

            var type = _cells[position.x, position.y].item.type;

            switch (direction)
            {

                case SearchDirection.Both:
                    {

                        if (_cells[position.x, position.y] != null) _foundCells.Add(_cells[position.x, position.y]);

                        for (int i = 1; i < range; i++)
                        {

                            foreach (var dir in _searchDirections)
                            {
                                Vector2Int pos = new Vector2Int(baseCell.gridPosition.x + dir.x * i, baseCell.gridPosition.y + dir.y * i);
                                if (IsValidPosition(pos))
                                {
                                    var cell = _cells[pos.x, pos.y];
                                    if (cell != null && cell.item != null && cell.item.type == type) _foundCells.Add(cell);
                                }
                            }
                        }

                        break;
                    }

                case SearchDirection.Horizontal:
                    {
                        int minX = position.x - range, maxX = position.x + range;
                        if (minX < 0) minX = 0;
                        if (maxX >= _grid.width) maxX = _grid.width - 1;

                        for (int x = minX; x <= maxX; x++)
                        {
                            if (!IsValidPosition(new Vector2Int(x, position.y))) continue;
                            var currentCell = _cells[x, position.y];
                            if (currentCell == null || currentCell.item == null) continue;
                            if (currentCell.item.type == type) _foundCells.Add(currentCell);
                        }
                        break;
                    }

                case SearchDirection.Vertical:
                    {
                        int minY = position.y - range, maxY = position.y + range;
                        if (minY < 0) minY = 0;
                        if (maxY >= _grid.height) maxY = _grid.height - 1;

                        for (int y = minY; y <= maxY; y++)
                        {
                            if (!IsValidPosition(new Vector2Int(position.x, y))) continue;
                            var currentCell = _cells[position.x, y];
                            if (currentCell == null || currentCell.item == null) continue;
                            if (currentCell.item.type == type) _foundCells.Add(currentCell);
                        }

                        break;
                    }
            }

            return _foundCells;
        }

        /// <summary>
        /// Find the related cells to the given position (same type)
        /// </summary>
        /// <param name="position"> The position to start the search</param>
        /// <returns> The connected cells</returns>
        public HashSet<GridCell> FindRelatedCells(Vector2Int position)
        {

            if (!IsValidPosition(position))
            {
                Debug.LogError($"attempted to find related cells from out of bounds cell: {position.x}, {position.y}");
                return null;
            }

            var baseCell = _cells[position.x, position.y];
            if (baseCell == null)
            {
                Debug.LogError($"attempted to find related cells from empty cell: {position.x}, {position.y}");
                return null;
            }

            var item = baseCell.item;
            if (item == null)
            {
                Debug.LogError($"attempted to find related cells from empty item: {position.x}, {position.y}");
                return null;
            }


            var type = baseCell.item.type;

            _foundCells.Clear();
            _visitedDirections.Clear();
            _searchQueue.Clear();

            _searchQueue.Enqueue(position);

            while (_searchQueue.Count > 0)
            {
                var currentPos = _searchQueue.Dequeue();
                var currentCell = _cells[currentPos.x, currentPos.y];
                if (!CheckCellItem(currentCell, type)) continue;

                if (_foundCells.Contains(currentCell)) continue;
                _foundCells.Add(currentCell);

                foreach (var dir in _searchDirections)
                {
                    Vector2Int pos = new Vector2Int(dir.x + currentPos.x, dir.y + currentPos.y);
                    if (IsValidPosition(pos) && !_visitedDirections.Contains(pos))
                    {
                        _searchQueue.Enqueue(pos);
                        _visitedDirections.Add(pos);
                    }
                }

            }
            return _foundCells;
        }



        public bool FindRelatedCells(Vector2Int position, in HashSet<GridCell> foundCells)
        {

            foundCells.Clear();

            if (!IsValidPosition(position))
            {
                Debug.LogError($"attempted to find related cells from out of bounds cell: {position.x}, {position.y}");
                return false;
            }

            var baseCell = _cells[position.x, position.y];
            if (baseCell == null)
            {
                Debug.LogError($"attempted to find related cells from empty cell: {position.x}, {position.y}");
                return false;
            }

            var item = baseCell.item;
            if (item == null)
            {
                Debug.LogError($"attempted to find related cells from empty item: {position.x}, {position.y}");
                return false;
            }

            var type = baseCell.item.type;

            _visitedDirections.Clear();
            _searchQueue.Clear();

            _searchQueue.Enqueue(position);

            while (_searchQueue.Count > 0)
            {
                var currentPos = _searchQueue.Dequeue();
                var currentCell = _cells[currentPos.x, currentPos.y];
                if (!CheckCellItem(currentCell, type)) continue;

                if (foundCells.Contains(currentCell)) continue;
                foundCells.Add(currentCell);

                foreach (var dir in _searchDirections)
                {
                    Vector2Int pos = new Vector2Int(dir.x + currentPos.x, dir.y + currentPos.y);
                    if (IsValidPosition(pos) && !_visitedDirections.Contains(pos))
                    {
                        _searchQueue.Enqueue(pos);
                        _visitedDirections.Add(pos);
                    }
                }

            }
            return foundCells.Count >= 1;
        }

        #endregion




    }
}