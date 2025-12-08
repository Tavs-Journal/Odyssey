using UnityEngine;
public class CrouchPlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        player.ReSizeCollider(player.stats.current.crouchHeight);
    }

    protected override void OnExit(Player player)
    {
        player.ReSizeCollider(player.originalHeight);
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.Fall();
        player.Decelerate(player.stats.current.crouchFriction);

        var inputDirection = player.input.GetMovementDirection();

        if(player.input.GetCrouchAndCraw() || !player.canStandUp)
        {
            if(inputDirection.sqrMagnitude > 0 && !player.holding)
            {
                if(player.lateralvelocity.sqrMagnitude == 0)
                {
                    player.states.Change<CrawingPlayerState>();
                }
            }
            else if (player.input.GetJumpDown())
            {
               player.Backflip(player.stats.current.backflipBackwardTurnForce);
            }
        }
        else
        {
            player.states.Change<IdleState>();
        }
    }
}
