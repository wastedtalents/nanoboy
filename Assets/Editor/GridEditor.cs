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
    
    private bool _showGridSection = true;
    private bool _showRulesSection = true;

    private float _tempf;
    private Vector2 _tempv2;

    private PropagationRule _newRule;
    private PropagationRuleType _tempPropType;

    private Texture _cellIcon;

    public override void OnInspectorGUI()
    {
        _showGridSection = EditorGUILayout.Foldout(_showGridSection, "Grid settings");

        // GRID SETTING.
        if (_showGridSection)
        {
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
                _grid.rows = (int) _tempf;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Grid Cols ");
            EditorGUI.BeginChangeCheck();
            _tempf = EditorGUILayout.IntField(_grid.cols, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                _grid.UpdateGrid(RedrawMode.Cols, _grid.cols, _tempf);
                _grid.cols = (int) _tempf;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Tile OffsX ");
            EditorGUI.BeginChangeCheck();
            _tempf = EditorGUILayout.FloatField(_grid.tileOffsetX, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                _grid.UpdateGrid(RedrawMode.TileOffsetX, _grid.tileOffsetX, _tempf);
                _grid.tileOffsetX = _tempf;
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

        }

        _showRulesSection = EditorGUILayout.Foldout(_showRulesSection, "Rules");
        if (_showRulesSection)
        {
            if (_newRule == null)
            {
                _newRule = new PropagationRule();
            }

            // NEW RULE>
            GUILayout.Label("--------------Add new rule-----------"); // Add new rule.
            _newRule.name = EditorGUILayout.TextField("Rule name : ", _newRule.name); // New rule stetup.

            EditorGUI.BeginChangeCheck();
            _newRule.type = (PropagationRuleType)DrawEnums("Rule type : ", (int)_newRule.type, typeof(PropagationRuleType)); // New rule stetup.   
            if (EditorGUI.EndChangeCheck())
                _newRule.UpdateType();

            DrawRuleGrid("Alive condition : ", _tempPropType);
           // DrawRuleGrid("Rule consequence : ", _newRule.type); // TO BE USED MAYBE L*R.

            if (GUILayout.Button("Save"))
            {
                _grid.propagationRules.Add(_newRule);
                _newRule = new PropagationRule();
            }

            // SUMMARY.
            GUILayout.Space(10); // Summary
            GUILayout.Label("--------------Summary-----------"); // Summary
            GUILayout.Space(10); // Summary
            GUILayout.Label("Total rules count : " + _grid.propagationRules.Count);

            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Clear rules",
                    "Are you sure you want to clear those amazing rules??", "yeppa", "noppa"))
                {
                    _grid.ResetRules();
                }
            }

            // Draw rules.
            PropagationRule removeRule = null;
            foreach (var propRule in _grid.propagationRules)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(propRule.ToString());
                if (GUILayout.Button("delete", GUILayout.Width(50)))
                    removeRule = propRule;
                GUILayout.EndHorizontal();
            }
            if (removeRule != null)
                _grid.propagationRules.Remove(removeRule);

            
        }

        SceneView.RepaintAll();
	}

    private int _tempX, _tempY;

    private void DrawRuleGrid(string name, PropagationRuleType type)
    {
        Debug.Log(_newRule);

        GUILayout.Label(name); // Add new rule.
        
       // midx, midy;
        _tempX = _tempY = (int)Mathf.Floor(_newRule.preCondition.Length/2);
                  
        // Draw grid.
        for (int row = 0; row < _newRule.preCondition.Length; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < _newRule.preCondition[row].Length; col++)
            {
                if (row == _tempY && col == _tempX)
                    GUILayout.Label("    X", GUILayout.Width(50));
                else
                    _newRule.preCondition[row][col] = (byte) DrawEnums("", _newRule.preCondition[row][col],
                        typeof (TileTypeAbbrev), GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();            
        }

        
    }

    private static int DrawEnums(string label, int index, System.Type aType, GUILayoutOption layout = null)
    {
        var itemNames = System.Enum.GetNames(aType);
        if(layout != null)
            return EditorGUILayout.Popup(label, index, itemNames, layout);
        else
        {
            return EditorGUILayout.Popup(label, index, itemNames);
        }
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
        _cellIcon = AssetDatabase.LoadAssetAtPath("Assets/Resources/Icons/bullseye.png", typeof (Texture)) as Texture;
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