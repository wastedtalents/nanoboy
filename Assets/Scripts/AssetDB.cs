using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class AssetDB
{
    private static List<GameObject> _prefabs;

    public static List<GameObject> Prefabs { get { return _prefabs;  } }

    static AssetDB ()
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
