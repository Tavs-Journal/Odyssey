using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
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
        enemy.Friction();
    }
}
