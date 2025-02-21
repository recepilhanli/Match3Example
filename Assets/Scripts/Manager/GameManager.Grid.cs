
using System.Collections.Generic;
using Game.Level;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace Game
{
    using Grid = Level.Grid;
    using Debug = Utils.Logger.Debug;
    //GameManager.Grid.cs
    public partial class GameManager
    {
        [Header("Grid Settings")]

        public GridCell gridCellPrefab;
        public Grid grid = new Grid();
        public RectTransform gridTable;
        public RectTransform gridTopLeft;

        private void CreateGrid(int Width, int Height, float spacing)
        {
            PrimeTweenConfig.SetTweensCapacity(Width * Height * 3 + 30);
            grid.Generate(Width, Height, spacing);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridCell cell = Instantiate(gridCellPrefab, gridTopLeft);
                    Vector2Int virtualPosition = new Vector2Int(x, y);
                    Vector2 anchoredPosition = new Vector2(x * spacing, -y * spacing);
                    cell.Init(grid, in anchoredPosition, in virtualPosition);
                    cell.SetItem(GetRandomContainer(), true);
                }
            }

            grid.groups.OrganiseGrid();
            CheckDeadEnd();
        }


        public void RemixGrid()
        {
            foreach (var cell in grid.cells)
            {
                cell.SetItem(GetRandomContainer(), true);
                cell.image.raycastTarget = true;
            }

            grid.totalExplosives = 0;

            grid.groups.OrganiseGrid();

            if (grid.totalBlastableGroups == 0) PlantRandomGroups();

            onGridRemixed?.Invoke();
            
            CheckDeadEnd();
        }

        /// <summary>
        /// Plants random groups in the grid
        /// </summary>
        public void PlantRandomGroups()
        {

            if (2 >= grid.width)
            {
                Debug.LogError("Grid width is too small for a group size");
                return;
            }

            else if (2 >= grid.height)
            {
                Debug.LogError("Grid height is too small for a group size");
                return;
            }

            Debug.Log("Planting Random Groups..");

            int totalRandomHorizontalGroups = Random.Range(1, Mathf.Max(2, grid.width / 2));
            int totalRandomVerticalGroups = Random.Range(1, Mathf.Max(2, grid.height / 2));

            int horizontalMaxGroupSize = Random.Range(2, grid.width);
            int verticalMaxGroupSize = Random.Range(2, grid.height);

            for (int i = 0; i < totalRandomHorizontalGroups; i++)
            {
                int groupSize = Random.Range(2, horizontalMaxGroupSize);
                int randomY = Random.Range(0, grid.height);
                int randomX = Random.Range(0, grid.width - 1);

                var item = grid.cells[randomX, randomY].item;
                if (item == null) item = GetRandomContainer();

                for (int j = 1; j < groupSize; j++)
                {
                    int x = randomX + j;
                    if (x >= grid.width) break;
                    grid.cells[x, randomY].SetItem(item, true);
                }

            }

            for (int i = 0; i < totalRandomVerticalGroups; i++)
            {
                int groupSize = Random.Range(2, verticalMaxGroupSize);
                int randomY = Random.Range(0, grid.height - 1);
                int randomX = Random.Range(0, grid.width);

                var item = grid.cells[randomX, randomY].item;
                if (item == null) item = GetRandomContainer();

                for (int j = 1; j < groupSize; j++)
                {
                    int y = randomY + j;
                    if (y >= grid.height) break;
                    grid.cells[randomX, y].SetItem(item, true);
                }
            }

            grid.groups.OrganiseGrid();
        }


        public void SetAllSameItem()
        {
            var randomItem = GetRandomContainer();
            foreach (var cell in grid.cells)
            {
                cell.SetItem(randomItem, false);
            }

            grid.groups.OrganiseGrid();
        }


        public void MarkBlastableGroups() => grid.groups.MarkRoots();
        public void MarkBlastableGroups(int max) => grid.groups.MarkRoots(max);


        public void CheckDeadEnd()
        {
            if (grid.totalBlastableGroups <= 0)
            {
                Debug.Log("No more moves left");
                RemixGrid();
                ShakeLocationGrid();
            }
        }


        public Tween ShakeRotationGrid(float strength = 3f)
        {
            Vector3 shakeStrength = new Vector3(0, 0, strength);
            return Tween.ShakeLocalRotation(gridTable, shakeStrength, .75f);
        }

        public Tween ShakeLocationGrid(float strength = 20f)
        {
            Vector3 shakeStrength = new Vector3(strength, strength, strength);
            return Tween.ShakeLocalPosition(gridTable, shakeStrength, 1f);
        }

    }
}
