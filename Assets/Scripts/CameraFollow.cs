using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public Transform player;
    public float speed = 1;

    private float _z;
    private Vector3 _position;


    // Use this for initialization
    void Start()
    {
        _position = new Vector3(0, 0, transform.position.z);
        _z = transform.position.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        _position = Vector3.Lerp(transform.position, player.position, Time.deltaTime * speed);
        _position.z = _z;
        transform.position = _position;
    }
}