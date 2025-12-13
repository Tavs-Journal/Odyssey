using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtPlayerState : PlayerState
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
        if(player.isGrounded && player.verticalVelocity.y <= 0)
        {
            if (player.health.current > 0)
            {
                player.states.Change<IdleState>();
            }
            else
            {

            }
        }
    }
}
