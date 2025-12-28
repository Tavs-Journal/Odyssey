using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class WayPointState : EnemyState
{
    public override void OnContact(Enemy enemy, Collider other)
    {

    }

    protected override void OnEnter(Enemy enemy)
    {

    }

    protected override void OnExit(Enemy enemy)
    {

    }

    protected override void OnStep(Enemy enemy)
    {
        enemy.Gravity();
        enemy.SnapToGround();
        var destination = enemy.wayPoints.current.position;
        destination = new Vector3(destination.x, enemy.position.y, destination.z);
        var head = destination - enemy.position;
        var distance = head.magnitude;
        var direction = head / distance;
        if(distance <= enemy.stats.current.waypointMinDistance)
        {
            enemy.Decelerate();
            enemy.wayPoints.Next();
        }
        else
        {
            enemy.Accelerate(direction, enemy.stats.current.waypointAcceleration, enemy.stats.current.waypointTopSpeed);
            if (enemy.stats.current.faceWaypoint)
            {
                enemy.FaceDirectionSmooth(direction);
            }
        }
    }
}
