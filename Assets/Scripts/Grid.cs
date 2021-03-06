﻿using System;
using System.Collections.Generic;
using System.Linq;
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

    private string _rootObjectTag;
    private string _rootObjectName;

    public List<GameObject> Objects
    {
        get { return _objects.Values.ToList(); }
    }

    // temps.
    private GameObject _rootObj;
    private Platform _tempPlatform;
    private int _tempIndex;
    private int _index;
    private Vector2 _tempVector;
    private Platform _platform;

    static Grid()
    {
        TileTypeAbbrevNames = Enum.GetNames(typeof(TileTypeAbbrev));
    }

    void Awake()
    {
        _rootObjectTag = String.Format("{0}_{1}", ROOT_TAG, gameObject.name);
        _rootObjectName = String.Format("{0}_{1}", ROOT_NAME, gameObject.name);
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
        EnsureRootObject();

        if (propagationRules == null)
            propagationRules = new List<PropagationRule>();

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
        _rootObj = GameObject.FindGameObjectWithTag(_rootObjectTag);
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
        EnsureRootObject();
        var newScale = new Vector2();
        switch (redraw)
        {
            case RedrawMode.Width:
                {
                    foreach (var ob in _objects.Values)
                    {
                        // recalc scale
                       CalcScale(ob, newValue, height);

                       // reposition.
                       _platform = ob.GetComponent<Platform>();
                       _tempVector.x = startPoint.x + _platform.gridXpos * newValue + newValue / 2;
                       _tempVector.y = startPoint.y + _platform.gridYpos * height + height / 2;
                       ob.transform.position = _tempVector;
                   
                    }
                    break;
                }
            case RedrawMode.Height:
                {
                    foreach (var ob in _objects.Values)
                    {
                        CalcScale(ob, width, newValue);

                        // reposition.
                        _platform = ob.GetComponent<Platform>();
                        _tempVector.x = startPoint.x + _platform.gridXpos * width + width / 2;
                        _tempVector.y = startPoint.y + _platform.gridYpos * newValue + newValue / 2;
                        ob.transform.position = _tempVector;

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
                    if (newValue > oldValue)
                    {
                        DisposeGrid(_lastCells);
                        _lastCells = new byte[(int)newValue][];
                    }
                    for (var row = 0; row < newValue; row++)
                    {
                        newCells[row] = new byte[cols];
                        _lastCells[row] = new byte[cols];
                        if(row < oldValue)
                            Array.Copy(_cells[row], newCells[row], cols);
                    }
                    DisposeGrid(_cells);
                    _cells = newCells;

                    if (newValue < oldValue)
                    {                        
                        // destroy objects that are not in the grid anymo'
                        _rootObj = GameObject.FindGameObjectWithTag(_rootObjectTag);                        
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
                        _lastCells[row] = new byte[(int)newValue];
                        Array.Copy(_cells[row], newCells[row], (int)maxCols);
                    }
                    DisposeGrid(_cells);
                    _cells = newCells;

                    if (newValue < oldValue)
                    {
                        // destroy objects that are not in the grid anymo'
                        _rootObj = GameObject.FindGameObjectWithTag(_rootObjectTag);
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
        var obj = GameObject.FindGameObjectWithTag(_rootObjectTag);
        if (obj == null)
        {
            obj = new GameObject { tag = _rootObjectTag, name = _rootObjectName };
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
    public void DestroyCell(int row, int col)
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

        CalcScale(obj);
    }

    /// <summary>
    /// Calculates the correct scale of an object.
    /// </summary>
    private void CalcScale(GameObject obj)
    {
        CalcScale(obj, width, height);
    }

    private void CalcScale(GameObject obj, float newWidth, float newHeight)
    {
        if (newWidth == 0)
            newWidth = 0.0001f;
        if (newHeight == 0)
            newHeight = 0.0001f;
        _tempVector.x = (1.0f / obj.renderer.bounds.size.x) * newWidth * obj.transform.localScale.x;
        _tempVector.y = (1.0f / obj.renderer.bounds.size.y) * newHeight * obj.transform.localScale.y;
        obj.transform.localScale = new Vector2(_tempVector.x, _tempVector.y);
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
        var root = GameObject.FindGameObjectWithTag(_rootObjectTag);
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

    void Simulate()
    {       
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (_cells[row][col] == (byte) TileType.Static) // were not making any rules for static.
                    continue;

                var isAlive = _cells[row][col] != 0;
                var rules = propagationRules.Where(p => p.forLiveCell == isAlive).ToList(); // get rules for alive cells.
                bool willLive = rules.Any() || isAlive; // if there are totaly no rules for me, do nothin.
                foreach (var rule in rules)
                {
                    willLive = willLive && rule.IsAlive(_lastCells, row, col);
                    if (willLive)
                        break;
                }
                if (willLive && !isAlive) // create new one
                {
                    var position = new Vector3(col * width + startPoint.x + width / 2.0f + tileOffsetX, row * height + startPoint.y + height / 2.0f + tileOffsetY, 0);
                    AddCell(position, AssetDB.Prefabs[0], TileType.Dynamic, row, col); // TODO : change this to account for other types.
                    _cells[row][col] = (byte)TileType.Dynamic;
                }
                else if (!willLive && isAlive) // if we need to destroy
                    DestroyCell(row, col);
            }
        }
    }


//    void Simulate() {
//         for(int row = 0; row < rows; row++) { 
//             for(int col = 0; col < cols; col++) {
//                 var neighbours = CountNeighbours(_lastCells, row, col);
//                 var isAlive = _cells[row][col] == 1;
//                 if(isAlive) {
//                     if (neighbours > 3 || neighbours < 2)
//                     {
//                         DestroyCell(row,col);
//                     }
//                 }
//                 else if(neighbours == 3)
//                 {
//                     var position = new Vector3(col * width + startPoint.x + width / 2.0f + tileOffsetX, row * height + startPoint.y + height / 2.0f + tileOffsetY, 0);
//                     AddCell(position, AssetDB.Prefabs[0], TileType.Dynamic, row, col); // TODO : change this to account for other types.
//                     _cells[row][col] = (byte)TileType.Dynamic;
//                 }
//             }
//         }
//    }


    void BackupGrid() {
        for(int i=0; i < rows; i++) {
            Array.Copy(_cells[i], 0, _lastCells[i], 0 , cols); // copy
        }
    }
}


