using UnityEngine;
using UnityEditor;
using Game;
using PrimeTween;
using Game.Level;
using Game.Utils;

[CustomEditor(typeof(GameManager))]
class GameManagerEditor : Editor
{

    private bool _visualizeGrid = false;
    private int _girdHeight = 10;
    private int _gridWidth = 8;

    private Vector2 _cellSize = new Vector2(1, 1);
    private Vector2 _paadding = new Vector2(0, 0);

    private float _cellSpacing = 1;

    private bool _findCellsFoldout = false;
    private Vector2Int _findCellsPoint = new Vector2Int(0, 0);
    private int _findCellsRange = 1;
    private bool _findStraightOnly = false;
    private bool _groupsFoldout = false;

    private GameManager manager => GameManager.instance;


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
        {
            VisualizeGridInspector();
            EditorGUILayout.LabelField("Other tools are only available in play mode.");
        }
        else
        {
            FindCellsInRangeInspector();
            GroupsInspector();
            TestButtonsInspector();
        }
    }



    private void GroupsInspector()
    {
        EditorGUILayout.Space(3);

        EditorGUILayout.BeginVertical("box");

        _groupsFoldout = EditorGUILayout.Foldout(_groupsFoldout, "Groups", true);
        if (_groupsFoldout)
        {
            var groupsDictioanry = manager.grid.groups.cellGroups;
            GUILayout.Label("Group Count: " + groupsDictioanry.Count);
            GUILayout.Label("Total Blastable (Groups + Explosives): " + manager.grid.totalBlastableGroups);

            foreach (var pair in groupsDictioanry)
            {
                int memberCount = pair.Value;
                GridCell root = pair.Key;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(root, typeof(GridCell), false);
                GUILayout.Label($"Members: {memberCount}");

                if (GUILayout.Button("Mark All Members"))
                {
                    var grid = manager.grid;
                    for (int x = 0; x < grid.width; x++)
                    {
                        for (int y = 0; y < grid.height; y++)
                        {
                            var cell = grid.cells[x, y];
                            if (cell.groupRoot == root)
                            {
                                cell.image.sprite = null;
                                cell.image.color = Color.gray;
                            }
                        }
                    }

                }
                EditorGUILayout.EndHorizontal();
            }

        }

        EditorGUILayout.EndVertical();
    }


    private void TestButtonsInspector()
    {

        EditorGUILayout.Space(2);

        GUI.color = CachedColors.softCyan;

        if (GUILayout.Button("Plant Random Groups"))
        {
            manager.PlantRandomGroups();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Mark All Blastable Roots"))
        {
            manager.MarkBlastableGroups();
        }

        GUI.color = CachedColors.softYellow;

        EditorGUILayout.Space();
        if (GUILayout.Button("Organise Grid"))
        {
            manager.grid.groups.OrganiseGrid();
        }

        GUI.color = CachedColors.softRed;

        EditorGUILayout.Space();

        if (GUILayout.Button("Find Buggy Cells"))
        {
            var grid = manager.grid;
            bool hasBug = false;

            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.cells[x, y];
                    if (cell == null)
                    {
                        Debug.LogError($"Cell is null at {x}, {y}");
                        hasBug = true;
                        continue;
                    }
                    else if (cell.item == null)
                    {
                        cell.image.sprite = null;
                        cell.image.color = Color.red;
                        Debug.LogError($"[Bug Check] {x}, {y} item is null");
                        hasBug = true;
                    }
                    else if (cell.groupRoot != null && !grid.groups.cellGroups.ContainsKey(cell.groupRoot))
                    {
                        cell.image.sprite = null;
                        cell.image.color = Color.yellow;
                        Debug.LogError($"[Bug Check] {x}, {y} group root is not in the group dictionary");
                        hasBug = true;
                    }
                    else if (cell.groupRoot != null)
                    {
                        var relatedCells = grid.searcher.FindRelatedCells(cell.groupRoot.gridPosition);
                        if (relatedCells.Count != grid.groups.cellGroups[cell.groupRoot])
                        {
                            cell.image.sprite = null;
                            cell.image.color = Color.magenta;
                            Debug.LogError($"[Bug Check] {x}, {y} group member count is incorrect");
                            hasBug = true;
                        }
                        else if (!relatedCells.Contains(cell))
                        {
                            cell.image.sprite = null;
                            cell.image.color = Color.cyan;
                            Debug.LogError($"[Bug Check] {x}, {y} is not in connected!!");
                            hasBug = true;
                        }

                        else if (relatedCells.Count == 1)
                        {
                            cell.image.sprite = null;
                            cell.image.color = Color.blue;
                            Debug.LogError($"[Bug Check] {x}, {y} has only single group member!!");
                            hasBug = true;
                        }

                        else
                        {
                            foreach (var relatedCell in relatedCells)
                            {
                                if (relatedCell == cell) continue;

                                if (relatedCell.groupRoot != cell.groupRoot)
                                {
                                    cell.image.sprite = null;
                                    cell.image.color = CachedColors.softPink;
                                    Debug.LogError($"[Bug Check] {x}, {y} {relatedCell.groupRoot} / {cell.groupRoot} has different group root but they are connected!!");
                                    hasBug = true;
                                    break;
                                }
                            }
                        }

                    }
                }
            }

            if (!hasBug) Debug.Log("No any bugs found");
        }



        GUI.color = Color.white;

    }


    private void FindCellsInRangeInspector()
    {

        EditorGUILayout.Space(3);

        EditorGUILayout.BeginVertical("box");

        _findCellsFoldout = EditorGUILayout.Foldout(_findCellsFoldout, "Find Cells In Range", true);
        if (_findCellsFoldout)
        {
            GUILayout.Label("Find Cells In Range");
            _findCellsPoint = EditorGUILayout.Vector2IntField("Point", _findCellsPoint);
            _findCellsRange = EditorGUILayout.IntField("Range", _findCellsRange);
            _findStraightOnly = EditorGUILayout.Toggle("Straight Only", _findStraightOnly);
            if (GUILayout.Button("Find Cells"))
            {
                var foundCells = (_findStraightOnly) ? manager.grid.searcher.FindCellsDirectional(_findCellsPoint, _findCellsRange) : manager.grid.searcher.FindCellsDirectional(_findCellsPoint, _findCellsRange);
                if (foundCells != null)
                {
                    Vector3 punchStrength = new Vector3(0.2f, 0.2f, 0.2f);
                    foreach (var cell in foundCells)
                    {
                        Tween.PunchScale(cell.rectTransform, punchStrength, 0.3f);
                    }
                }
            }
        }

        EditorGUILayout.EndVertical();
    }





    private void VisualizeGridInspector()
    {
        EditorGUILayout.Space(5);

        if (_visualizeGrid)
        {
            GUI.color = Color.green;
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Grid Visualization");
            _cellSpacing = EditorGUILayout.FloatField("Cell Spacing", _cellSpacing);
            _gridWidth = EditorGUILayout.IntField("Grid Width", _gridWidth);
            _girdHeight = EditorGUILayout.IntField("Grid Height", _girdHeight);
            _paadding = EditorGUILayout.Vector2Field("Padding", _paadding);
            _cellSize = EditorGUILayout.Vector2Field("Cell Size", _cellSize);
            EditorGUILayout.EndVertical();
        }

        else GUI.color = Color.gray;

        if (GUILayout.Button("Visualize Grid"))
        {
            if (!_visualizeGrid && manager.gridTopLeft == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the grid left top position", "OK");
                return;
            }

            _visualizeGrid = !_visualizeGrid;
        }

        GUI.color = Color.white;
    }


    private void GridSceneGUI()
    {
        if (!_visualizeGrid) return;

        Handles.color = Color.green;

        Vector3 topLeft = manager.gridTopLeft.position;

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _girdHeight; y++)
            {
                Vector3 cellPosition = topLeft + new Vector3(x * _cellSpacing, -y * _cellSpacing);
                Vector3 cellSize = new Vector3(_cellSize.x, _cellSize.y, 0);
                Vector3 padding = new Vector3(_paadding.x, _paadding.y, 0);

                Handles.DrawWireCube(cellPosition + padding, cellSize);

            }
        }
    }

    private void OnSceneGUI()
    {
        if (!Application.isPlaying) GridSceneGUI();
    }
}
