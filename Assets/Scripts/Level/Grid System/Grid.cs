using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrimeTween;
using UnityEngine;

namespace Game.Level
{
    using Debug = Utils.Logger.Debug;
    public class Grid
    {

        public GridCell[,] cells;
        public int width => cells.GetLength(0);
        public int height => cells.GetLength(1);


        public float spacing { get; private set; } = 0;
        public bool isBusy { get; private set; } = false;

        public int totalExplosives
        {
            get => _totalExplosives;
            set
            {
                _totalExplosives = value;
                if (_totalExplosives < 0)
                {
                    _totalExplosives = 0;
                    Debug.LogWarning("Total Explosives can't be less than 0");
                }
            }
        }

        public int totalBlastableGroups => _groups.cellGroups.Count + totalExplosives;
        private int _totalExplosives = 0;


        #region Components
        private GridSearcher _searcher;
        private GridPhysics _physicsHelper;
        private GridGroups _groups;
        
        public GridGroups groups => _groups;
        public GridPhysics physics => _physicsHelper;
        public GridSearcher searcher => _searcher;
        #endregion

        public void Generate(int Width, int Height, float spacing)
        {
            cells = new GridCell[Width, Height];
            _searcher = new GridSearcher();
            _physicsHelper = new GridPhysics();
            _groups = new GridGroups();
            this.spacing = spacing;
        }

        public void DisableCells() => isBusy = true;
        public void EnableCells() => isBusy = false;


        public void AddCell(GridCell cell, int x, int y)
        {

            if (cells == null)
            {
                Debug.LogError("attempted to add cell to null grid");
                return;
            }
            else if (x < 0 || x >= width || y < 0 || y >= height)
            {
                Debug.LogError($"attempted to add cell to out of bounds grid: {x}, {y}");
                return;
            }
            else if (cells[x, y] != null)
            {
                Debug.LogError($"attempted to add cell to non-empty grid cell: {x}, {y}");
                return;
            }

            cells[x, y] = cell;
        }

    }
}