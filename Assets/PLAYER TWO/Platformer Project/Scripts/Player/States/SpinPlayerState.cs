using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
public class SpinPlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        if (!player.isGrounded)
        {
            player.verticalVelocity = Vector3.up * player.stats.current.airSpinUpwardForce;
        }
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.StomAttack();
        player.AirDive();
        player.AccelerateToInputDirection();

        if(timeSinceEntered >= player.stats.current.spinDuration)
        {
            if (player.isGrounded)
            {
                player.states.Change<IdleState>();
            }
            else
            {
                player.states.Change<FallPlayerState>();
            }
        }
    }
}
