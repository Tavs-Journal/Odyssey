using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.Friction();
        player.SnapToGround();
    }
}
