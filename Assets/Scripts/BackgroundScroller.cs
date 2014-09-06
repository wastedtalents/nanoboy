using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class BackgroundScroller : MonoBehaviour
{
    public Transform[] backgrounds;
    public float parallaxScale;
    public float parallaxReductionFactor;
    public float smoothing;

    private Vector2 _lastPosition;

    void Start()
    {
        _lastPosition = transform.position;
    }

    void Update()
    {
        var parallax = (_lastPosition.x - transform.position.x)*parallaxScale;
        for(var i = 0; i < backgrounds.Length; i++)
        {
            var backgroundTargetPosition = backgrounds[i].position.x + parallax * (i * parallaxReductionFactor + 1);
            backgrounds[i].position = Vector2.Lerp(
                backgrounds[i].position,
                new Vector2(backgroundTargetPosition, backgrounds[i].position.y),
                smoothing*Time.deltaTime);
        }
        _lastPosition = transform.position;
    }

}
