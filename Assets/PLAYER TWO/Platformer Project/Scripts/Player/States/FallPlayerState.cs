using UnityEngine;

public class FallPlayerState : PlayerState
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
        player.Gravity();
        player.FaceDirectionSmooth(player.lateralvelocity);
        player.Jump();
        if (player.isGrounded)
        {
            player.states.Change<IdleState>();
        }
    }
}