using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(WayPointManager), typeof(Collider))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Move Settings")]
    public float speed = 3f;
    public WayPointManager waypoints {  get; protected set; }

    protected virtual void Awake()
    {
        tag = GameTags.Platform;
        waypoints = GetComponent<WayPointManager>();
    }

    protected virtual void Update()
    {
        var position = transform.position;
        var target = waypoints.current.position;

        position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
        transform.position = position;

        if(Vector3.Distance(transform.position, target) == 0)
        {
            waypoints.Next();
        }
    }
}
