using System;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(Grid))]
public class GridEditor : Editor
{
	private Grid _grid;
    
    private bool _showGridSection = true;
    private bool _showRulesSection = true;

    private float _tempf;
    private Vector2 _tempv2;

    private PropagationRule _newRule;
    private PropagationRuleType _tempPropType;
    private GameObject[] _selection = new GameObject[1];

    private Platform _platform;


    public override void OnInspectorGUI()
    {
        _showGridSection = EditorGUILayout.Foldout(_showGridSection, "Grid settings");

        // GRID SETTING.
        if (_showGridSection)
        {
            _markedToDestroy = null;

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

            // SUMMARY.
            GUILayout.Space(10); // Summary
            GUILayout.Label("--------------Summary-----------"); // Summary
            GUILayout.Space(10); // Summary

            foreach (var obj in _grid.Objects)
            {
                GUILayout.BeginHorizontal();
                _platform = obj.GetComponent<Platform>();
                GUILayout.Label(String.Format("({0},{1})", _platform.gridXpos, _platform.gridYpos));
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    _markedToDestroy = _platform;
                }
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    _selection[0] = obj;
                    Selection.objects = _selection;
                    SceneView.lastActiveSceneView.FrameSelected();
                }
                GUILayout.EndHorizontal();
            }
            // marked to destroy.
            if(_markedToDestroy != null)
                _grid.DestroyCell(_markedToDestroy.gridYpos, _markedToDestroy.gridXpos);

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

             _newRule.forTileType = (RecipientTileType)DrawEnums("Tile type : ", (byte)_newRule.forTileType, typeof(RecipientTileType)); // New rule stetup.   

            _newRule.forLiveCell = EditorGUILayout.Toggle("For alive cell", _newRule.forLiveCell);

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
                if (GUILayout.Button("x", GUILayout.Width(20)))
                    removeRule = propRule;
                GUILayout.EndHorizontal();
            }
            if (removeRule != null)
                _grid.propagationRules.Remove(removeRule);

            
        }

        SceneView.RepaintAll();
	}

    private int _tempX, _tempY;
    private Platform _markedToDestroy;

    private void DrawRuleGrid(string name, PropagationRuleType type)
    {
        GUILayout.Label(name); // Add new rule.

        // show aggregates.
        if (_newRule.type == PropagationRuleType.Aggregate) {
            _newRule.ruleOp = (AggregateRuleOp)DrawEnums("Alive neighbours :", (int)_newRule.ruleOp, typeof(AggregateRuleOp)); // New rule stetup.   
            GUILayout.BeginHorizontal();
            _newRule.numbers.value1 = Int32.Parse(EditorGUILayout.TextField("", _newRule.numbers.value1.ToString(), GUILayout.Width(20)));
            if(_newRule.ruleOp == AggregateRuleOp.Between) 
                _newRule.numbers.value2 = Int32.Parse(EditorGUILayout.TextField("", _newRule.numbers.value2.ToString(), GUILayout.Width(20)));
            GUILayout.EndHorizontal();
         }
        else {
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
        SceneView.onSceneGUIDelegate = GridUpdate;
	}

    private void GridUpdate(SceneView sceneview)
    {
        var e = Event.current; // get current event.
        var r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
        var mousePos = r.origin;

        DrawButton();

        if (e.isKey)
        {
            GameObject prefab = null;
            var pefabType = TileType.None;

            switch (e.character) {
                case 'a' : // dynamic
                {
                    prefab = AssetDB.Prefabs[0];
                    pefabType = TileType.Dynamic;
                    break;
                }
                case 's': // static
                {
                    prefab = AssetDB.Prefabs[1];
                    pefabType = TileType.Static;
                    break;
                }
                case 'd':
                {
                    prefab = AssetDB.Prefabs[2];
                    pefabType = TileType.Hostile;
                    break;
                }
            }

            if (prefab != null && _grid.IsInGrid(mousePos))
            {
             _grid.AddRemoveItem(pefabType, prefab, mousePos);
            }
        }

    }

    public void DrawButton() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(2, 2, 100,50));
            if(GUILayout.Button("Start simulation")) {
            } 
        GUILayout.EndArea();
     GUILayout.BeginArea(new Rect(102, 2, 60,50));
            if(GUILayout.Button(">>")) {
                _grid.NextSimulationStep();
            } 
        GUILayout.EndArea();
         GUILayout.BeginArea(new Rect(162, 2, 90,50));
            if(GUILayout.Button("Restart")) {
            } 
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}