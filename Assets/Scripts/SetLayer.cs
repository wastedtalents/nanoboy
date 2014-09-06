using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SetLayer : MonoBehaviour
{
    public string sortingLayerName;
    private Renderer renderer;

    void Start()
    {
        if (String.IsNullOrEmpty(sortingLayerName))
            throw new Exception("Layer cannot be empty");
        renderer = GetComponent<Renderer>();
        renderer.sortingLayerName = sortingLayerName;
    }
}