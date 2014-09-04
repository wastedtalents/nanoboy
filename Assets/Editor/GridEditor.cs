using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Grid))]
public class GridEditor : Editor
{
	private Grid _grid;
    private List<GameObject> _prefabs;
    private RedrawMode _redrawMode;

    private float _tempf;
    private Vector2 _tempv2;

    public override void OnInspectorGUI()
    {
        _redrawMode = RedrawMode.None;
       

        // Paint.
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Width ");
        EditorGUI.BeginChangeCheck();
        _tempf = EditorGUILayout.FloatField(_grid.width, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.Width, _grid.width, _tempf);
            _grid.width = _tempf;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Height ");
        EditorGUI.BeginChangeCheck();
        _tempf = EditorGUILayout.FloatField(_grid.height, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.Height, _grid.height, _tempf);
            _grid.height = _tempf;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Rows ");
        EditorGUI.BeginChangeCheck();
        _tempf = EditorGUILayout.IntField(_grid.rows, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.Rows, _grid.rows, _tempf);
            _grid.rows = (int)_tempf;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Cols ");
        EditorGUI.BeginChangeCheck();
        _tempf = EditorGUILayout.IntField(_grid.cols, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.Cols, _grid.cols, _tempf);
            _grid.cols= (int)_tempf;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Tile OffsX ");
        EditorGUI.BeginChangeCheck();
        _tempf = EditorGUILayout.FloatField(_grid.tileOffsetX, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.TileOffsetX, _grid.tileOffsetX, _tempf);
            _grid.tileOffsetX= _tempf;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Tile OffsY ");
        EditorGUI.BeginChangeCheck();
        _tempf = EditorGUILayout.FloatField(_grid.tileOffsetY, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.TileOffsetY, _grid.tileOffsetY, _tempf);
            _grid.tileOffsetY = _tempf;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        _tempv2 = EditorGUILayout.Vector2Field("Start", _grid.startPoint);
        if (EditorGUI.EndChangeCheck())
        {
            _grid.UpdateGrid(RedrawMode.Position, _grid.startPoint, _tempv2);
            _grid.startPoint = _tempv2;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset"))
        {
            if (EditorUtility.DisplayDialog("Reset grid",
                "Are you sure you want to reset this masterfull grid??", "yes", "nope"))
            {
                _grid.Reset();
            } 
        }

        

        SceneView.RepaintAll();
	}


    // as soon as enabled.
	void OnEnable()
	{
		_grid = (Grid)target;
	    LoadPrefabs();
        SceneView.onSceneGUIDelegate = GridUpdate;
	}

    private void LoadPrefabs()
    {
        if (_prefabs == null || _prefabs.Count == 0)
        {
            _prefabs = new List<GameObject>
            {
                AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/DynamicCell.prefab", typeof (GameObject)) as
                    GameObject,
                AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/StaticCell.prefab", typeof (GameObject)) as
                    GameObject,
                AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/HostileCell.prefab", typeof (GameObject)) as
                    GameObject
            };
        }
    }

    private void GridUpdate(SceneView sceneview)
    {
        var e = Event.current; // get current event.
        var r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
        var mousePos = r.origin;

        if (e.isKey)
        {
            GameObject prefab = null;
            var pefabType = TileType.None;

            switch (e.character) {
                case 'a' : // dynamic
                {
                    prefab = _prefabs[0];
                    pefabType = TileType.Dynamic;
                    break;
                }
                case 's': // static
                {
                    prefab = _prefabs[1];
                    pefabType = TileType.Static;
                    break;
                }
                case 'd':
                {
                    prefab = _prefabs[2];
                    pefabType = TileType.Hostile;
                    break;
                }
            }

            if (prefab != null && _grid.IsInGrid(mousePos))
            {
//                //var obj = (GameObject) Instantiate(prefab);
//                var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
//
//                var aligned = new Vector3(Mathf.Floor(mousePos.x / _grid.width) * _grid.width + _grid.width / 2.0f, Mathf.Floor(mousePos.y / _grid.height) * _grid.height + _grid.height / 2.0f, 0.0f);
//                obj.transform.position = aligned;
//                obj.transform.localScale = new Vector3(_grid.width, _grid.height, 1);
//
//                // add this object to dah grid.
//                var indexx = Mathf.Floor((mousePos.x - _grid.startPoint.x) / _grid.width);
//                var indexy = Mathf.Floor((mousePos.y - _grid.startPoint.y) / _grid.height);
                _grid.AddRemoveItem(pefabType, prefab, mousePos);
            }
        }

    }
}