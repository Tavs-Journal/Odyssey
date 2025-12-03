using UnityEngine;

public class BrakePlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
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
        player.Decelerate();

        if(player.lateralvelocity.sqrMagnitude == 0)
        {
            player.states.Change<IdleState>();
        }
    }
}