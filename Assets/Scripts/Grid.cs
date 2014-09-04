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
    private const string ROOT_TAG = "RootMapObject";
    private const string PLATFORM_TAG = "Platform";
    private const string ROOT_NAME= "_map";

    public float tileOffsetX = 0f;
    public float tileOffsetY = 0f;
	public float width = 10.0f;
	public float height = 10.0f;
    public int rows = 10;
    public int cols = 10;
    public Vector2 startPoint;

    private byte[][] _cells;
    private Dictionary<int, GameObject> _objects;

    // temps.
    private GameObject _rootObj;
    private Platform _tempPlatform;
    private int _tempIndex;

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
        _objects = new Dictionary<int, GameObject>();
        for (int i = 0; i < rows; i++)
            _cells[i] = new byte[cols];

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
        Debug.Log("items" + _objects.Count);
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
        }
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
	            Gizmos.DrawLine(new Vector3(startPoint.x, startPoint.y + row*height, 0.0f),
	                new Vector3(startPoint.x + (cols*width), startPoint.y + row*height, 0.0f));
	        for (var col = 0; col <= cols; col++)
	            Gizmos.DrawLine(new Vector3(startPoint.x + col*width, startPoint.y, 0.0f),
	                new Vector3(startPoint.x + col*width, startPoint.y + rows*height, 0.0f));

	}

    public bool IsInGrid(Vector3 mousePos)
    {
        if (mousePos.x >= startPoint.x && mousePos.x <= (startPoint.x + cols*width) && mousePos.y >= startPoint.y &&
            mousePos.y <= (startPoint.y + rows*height))
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
            obj = new GameObject {tag = ROOT_TAG, name = ROOT_NAME};
        }
        return obj;
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

        if (_cells[indexy][indexx] != (byte) TileType.None) // theres sth there!
        {
            _cells[indexy][indexx] = (byte) TileType.None;

            var index = indexy*10000 + indexx;

            Debug.Log("ITEM REMOVED AT " + index);

            if (!_objects.ContainsKey(index)) 
                return;
            DestroyImmediate(_objects[index]);
            _objects[index] = null;
            _objects.Remove(index);
        }
        else // we can add
        {
            var root = EnsureRootObject();
            var posX = mousePos.x - startPoint.x;
            var posY = mousePos.y - startPoint.y;

                        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        var aligned = new Vector3(startPoint.x + Mathf.Floor(posX / width) * width + width / 2.0f + tileOffsetX, 
                            startPoint.y + Mathf.Floor(posY / height) * height + height / 2.0f + tileOffsetY, 0.0f);
                        obj.transform.position = aligned;
                        obj.transform.localScale = new Vector3(width, height, 1);
                        obj.tag = PLATFORM_TAG;
            
                        var platform = obj.GetComponent<Platform>();
                        platform.gridXpos = indexx;
                        platform.gridYpos = indexy;
                        platform.type = pefabType;
            
                        _cells[indexy][indexx] = (byte)pefabType;
                        var index = indexy * 10000 + indexx;
                        _objects[index] = obj;
            
                        obj.transform.parent = root.transform;
        }
    }

    public void Reset()
    {
        var root = GameObject.FindGameObjectWithTag(ROOT_TAG);
        DestroyImmediate(root);
        InitGrid();
    }
}


