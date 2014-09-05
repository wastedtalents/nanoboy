using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using System.Collections;

public enum TileType : byte
{
    None = 0,
    Dynamic = 1,
    Static = 2,
    Hostile = 3
}

public enum RecipientTileType : byte {
    Dynamic = 1,
    Hostile = 3   
}

public enum TileTypeAbbrev : byte
{
    N = 0,
    D = 1,
    S = 2,
    H = 3
}


[Flags]
public enum RedrawMode : byte
{
    None,
    Width, // only resize
    Height,
    Position,
    Rows,
    Cols,
    TileOffsetX,
    TileOffsetY,
    Flush // redraw the whole grid
}


[ExecuteInEditMode]

public class Grid : MonoBehaviour
{
    public static string[] TileTypeAbbrevNames;

    private const string ROOT_TAG = "RootMapObject";
    private const string PLATFORM_TAG = "Platform";
    private const string ROOT_NAME = "_map";

    public float tileOffsetX = 0f;
    public float tileOffsetY = 0f;
    public float width = 10.0f;
    public float height = 10.0f;

    [Range(0, 1000)]
    public int rows = 10;

    [Range(0, 1000)]
    public int cols = 10;
    public Vector2 startPoint;

    public List<PropagationRule> propagationRules; 

    private byte[][] _cells;
    private byte[][] _lastCells;
    private Dictionary<int, GameObject> _objects;

    // temps.
    private GameObject _rootObj;
    private Platform _tempPlatform;
    private int _tempIndex;
    private int _index;

    static Grid()
    {
        TileTypeAbbrevNames = Enum.GetNames(typeof(TileTypeAbbrev));
    }

    void OnEnable()
    {
        if (_cells == null)
        {
            InitGrid();
        }
    }

    void InitGrid()
    {
        _cells = new byte[rows][];
        _lastCells = new byte[rows][];

        _objects = new Dictionary<int, GameObject>();
        for (int i = 0; i < rows; i++)
        {
            _cells[i] = new byte[cols];
            _lastCells[i] = new byte[cols];
        }

        // reload currently visible data.
        var items = 0;
        _rootObj = GameObject.FindGameObjectWithTag(ROOT_TAG);
        if (_rootObj == null)
            return;

        // parse all the items.
        foreach (Transform trans in _rootObj.transform)
        {
            _tempPlatform = trans.gameObject.GetComponent<Platform>();
            if (_tempPlatform != null)
            {
                _cells[_tempPlatform.gridYpos][_tempPlatform.gridXpos] = (byte)_tempPlatform.type;
                _tempIndex = _tempPlatform.gridYpos * 10000 + _tempPlatform.gridXpos;
                _objects[_tempIndex] = trans.gameObject;
                items++;
            }
        }
    }

    public void UpdateGrid(RedrawMode redraw, float oldValue, float newValue)
    {
        var newScale = new Vector2();
        switch (redraw)
        {
            case RedrawMode.Width:
                {
                    foreach (var ob in _objects.Values)
                    {
                        newScale.x = newValue;
                        newScale.y = ob.transform.localScale.y;
                        ob.transform.localScale = newScale;
                    }
                    break;
                }
            case RedrawMode.Height:
                {
                    foreach (var ob in _objects.Values)
                    {
                        newScale.x = ob.transform.localScale.x;
                        newScale.y = newValue;
                        ob.transform.localScale = newScale;
                    }
                    break;
                }
            case RedrawMode.TileOffsetX:
                {
                    foreach (var ob in _objects.Values)
                    {
                        newScale.x = ob.transform.position.x + (newValue - oldValue);
                        newScale.y = ob.transform.position.y;
                        ob.transform.position = newScale;
                    }
                    break;
                }
            case RedrawMode.TileOffsetY:
                {
                    foreach (var ob in _objects.Values)
                    {
                        newScale.x = ob.transform.position.x;
                        newScale.y = ob.transform.position.y + (newValue - oldValue);
                        ob.transform.position = newScale;
                    }
                    break;
                }
            case RedrawMode.Rows:
                {
                    var newCells = new byte[(int)newValue][];
                    for (var row = 0; row < newValue; row++)
                    {
                        newCells[row] = new byte[cols];
                        if(row < oldValue)
                            Array.Copy(_cells[row], newCells[row], cols);
                    }
                    DisposeGrid(_cells);
                    _cells = newCells;

                    if (newValue < oldValue)
                    {
                        // destroy objects that are not in the grid anymo'
                        _rootObj = GameObject.FindGameObjectWithTag(ROOT_TAG);
                        foreach (Transform trans in _rootObj.transform)
                        {
                            _tempPlatform = trans.GetComponent<Platform>();
                            if (_tempPlatform.gridYpos >= newValue)
                            {                                
                                _index = GetIndex(_tempPlatform.gridYpos, _tempPlatform.gridXpos);
                                _objects.Remove(_index);
                                DestroyImmediate(_tempPlatform.gameObject);
                            }
                        }
                    }

                    break;
                }
            case RedrawMode.Cols:
                {
                    var newCells = new byte[rows][];
                    var maxCols = Math.Min(newValue, oldValue);
                    for (var row = 0; row < rows; row++)
                    {
                        newCells[row] = new byte[(int)newValue];
                        Array.Copy(_cells[row], newCells[row], (int)maxCols);
                    }
                    DisposeGrid(_cells);
                    _cells = newCells;

                    if (newValue < oldValue)
                    {
                        // destroy objects that are not in the grid anymo'
                        _rootObj = GameObject.FindGameObjectWithTag(ROOT_TAG);
                        foreach (Transform trans in _rootObj.transform)
                        {
                            _tempPlatform = trans.GetComponent<Platform>();
                            if (_tempPlatform.gridXpos >= newValue)
                            {
                                _index = GetIndex(_tempPlatform.gridYpos, _tempPlatform.gridXpos);
                                _objects.Remove(_index);
                                DestroyImmediate(_tempPlatform.gameObject);
                            }
                        }
                    }

                    break;
                }
        }
    }

    private void DisposeGrid(byte[][] cells)
    {
        for (int i = 0; i < cells.Length; i++)
            cells[i] = null;
    }

    public void UpdateGrid(RedrawMode redraw, Vector2 oldValue, Vector2 newValue)
    {
        var diff = newValue - oldValue;
        foreach (var obj in _objects.Values)
            obj.transform.position = (Vector2)obj.transform.position + diff;
    }

    void OnDrawGizmos()
    {
        Vector3 pos = Camera.current.transform.position;
        for (var row = 0; row <= rows; row++)
            Gizmos.DrawLine(new Vector3(startPoint.x, startPoint.y + row * height, 0.0f),
                new Vector3(startPoint.x + (cols * width), startPoint.y + row * height, 0.0f));
        for (var col = 0; col <= cols; col++)
            Gizmos.DrawLine(new Vector3(startPoint.x + col * width, startPoint.y, 0.0f),
                new Vector3(startPoint.x + col * width, startPoint.y + rows * height, 0.0f));

    }

    public bool IsInGrid(Vector3 mousePos)
    {
        if (mousePos.x >= startPoint.x && mousePos.x <= (startPoint.x + cols * width) && mousePos.y >= startPoint.y &&
            mousePos.y <= (startPoint.y + rows * height))
        {
            return true;
        }
        return false;
    }

    GameObject EnsureRootObject()
    {
        var obj = GameObject.FindGameObjectWithTag(ROOT_TAG);
        if (obj == null)
        {
            obj = new GameObject { tag = ROOT_TAG, name = ROOT_NAME };
        }
        return obj;
    }

    private int GetIndex(int row, int col)
    {
        return row * 10000 + col;
    }

    /// <summary>
    /// Destroys cell.
    /// </summary>
    private void DestroyCell(int row, int col)
    {
        _cells[row][col] = (byte)TileType.None;

        _index = GetIndex(row, col);
        if (!_objects.ContainsKey(_index))
            return;
        DestroyImmediate(_objects[_index]);
        _objects[_index] = null;
        _objects.Remove(_index);
    }

    private void AddCell(Vector3 aligned, GameObject prefab, TileType prefabType, int row, int col)
    {
        var root = EnsureRootObject();
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        obj.transform.position = aligned;
        obj.transform.localScale = new Vector3(width, height, 1);
        obj.tag = PLATFORM_TAG;

        var platform = obj.GetComponent<Platform>();
        platform.gridXpos = col;
        platform.gridYpos = row;
        platform.type = prefabType;

        _cells[row][col] = (byte)prefabType;
        _index = GetIndex(row, col);
        _objects[_index] = obj;

        obj.transform.parent = root.transform;
    }

    /// <summary>
    /// Adds an item of a given type to the grid.
    /// </summary>
    /// <param name="pefabType"></param>
    /// <param name="o"></param>
    /// <param name="indexx"></param>
    /// <param name="indexy"></param>
    public void AddRemoveItem(TileType pefabType, GameObject prefab, Vector2 mousePos)
    {
        var indexx = (int)Mathf.Floor((mousePos.x - startPoint.x) / width);
        var indexy = (int)Mathf.Floor((mousePos.y - startPoint.y) / height);

        if (_cells[indexy][indexx] != (byte)TileType.None) // theres sth there!
        {
          DestroyCell(indexy, indexx);
        }
        else // we can add
        {
            var posX = mousePos.x - startPoint.x;
            var posY = mousePos.y - startPoint.y;

            var aligned = new Vector3(startPoint.x + Mathf.Floor(posX / width) * width + width / 2.0f + tileOffsetX,
                startPoint.y + Mathf.Floor(posY / height) * height + height / 2.0f + tileOffsetY, 0.0f);

            AddCell(aligned, prefab, pefabType, indexy, indexx);
        }
    }

    public void Reset()
    {
        var root = GameObject.FindGameObjectWithTag(ROOT_TAG);
        DestroyImmediate(root);
        InitGrid();
    }

    public void ResetRules()
    {
        foreach (var rule in propagationRules)
            rule.Dispose();
        propagationRules.Clear();
    }

    // Next step
    public void NextSimulationStep() {
        BackupGrid();
        Simulate();
    }

    void Simulate() {
         for(int row = 0; row < rows; row++) { 
             for(int col = 0; col < cols; col++) {
                 var neighbours = AliveNeighbours(_lastCells, row, col);
                 var isAlive = _cells[row][col] == 1;
                 if(isAlive) {
                     if (neighbours > 3 || neighbours < 2)
                     {
                         DestroyCell(row,col);
                     }
                 }
                 else if(neighbours == 3)
                 {
                     var position = new Vector3(col * width + startPoint.x + width / 2.0f + tileOffsetX, row * height + startPoint.y + height / 2.0f + tileOffsetY, 0);
                     AddCell(position, AssetDB.Prefabs[0], TileType.Dynamic, row, col); // TODO : change this to account for other types.
                     _cells[row][col] = (byte)TileType.Dynamic;
                 }
             }
         }
    }

    /// <summary>
    /// Count alive neighbours of a cell.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    int AliveNeighbours(byte[][] board, int row, int col)
    {
        var sum = 0;
        for (int i = (row == 0 ? 0 : row - 1); i <= ((row >= rows - 1) ? row : row + 1); i++)
        {
            for (int j = (col == 0 ? 0 : col - 1); j <= ((col >= cols - 1) ? col : col + 1); j++)
            {
                if (i != row || j != col)
                    sum += board[i][j]; // that is to be changed to account for any board element.
            }
        }
        return sum;
    }

    void BackupGrid() {
        for(int i=0; i < rows; i++) {
            Array.Copy(_cells[i], 0, _lastCells[i], 0 , cols); // copy
        }
    }
}


