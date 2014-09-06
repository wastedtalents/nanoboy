using UnityEngine;
using UnityEditor;

public static class EditorListVLS 
{
    private static GUIContent
        toggle = new GUIContent("Active", "Enable"),
        upButton = new GUIContent("\u25B2", "Move Up"),
        downButton = new GUIContent("\u25BC", "Move Down"),
        dupButton = new GUIContent("+", "Duplicate"),
        delButton = new GUIContent("\u2717", "Delete"),
        addButton = new GUIContent("New Pass", "Add new pass.");



    public static void Show(SerializedProperty list)
    {
        EditorGUILayout.PropertyField(list);
        EditorGUI.indentLevel += 1;
        if (list.isExpanded)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("activeLayer").boolValue = GUILayout.Toggle(list.GetArrayElementAtIndex(i).FindPropertyRelative("activeLayer").boolValue, toggle);
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("Layer " + (i+1), ""), true);

                EditorGUILayout.BeginHorizontal();
                ShowButtons(list, i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(addButton))
            {
                list.arraySize += 1;
            }
        }
        EditorGUI.indentLevel -= 1;
    }

    private static void ShowButtons(SerializedProperty list, int index)
    {        

        if(GUILayout.Button(upButton))
        {
            list.MoveArrayElement(index, index - 1);
        }

        if (GUILayout.Button(downButton))
        {
            list.MoveArrayElement(index, index + 1);
        }

        if (GUILayout.Button(dupButton))
        {
            list.InsertArrayElementAtIndex(index);
        }
        
        if(GUILayout.Button(delButton))
        {
            int oldSize = list.arraySize;
            list.DeleteArrayElementAtIndex(index);
            if (list.arraySize == oldSize)
                list.DeleteArrayElementAtIndex(index);
        }
    }
}
