using UnityEngine;

public class BrakePlayerState : PlayerState
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
        var inputDirection = player.input.GetMovementCameraDirection();
        if(player.stats.current.canBackflip && player.input.GetJumpDown() && 
            Vector3.Dot(inputDirection, player.transform.forward) < 0)
        {
            player.Backflip(player.stats.current.backflipBackwardTurnForce);
        }
        else
        {
            player.Decelerate();
            player.SnapToGround();
            player.Jump();
            player.Fall();
            if (player.lateralvelocity.sqrMagnitude == 0)
            {
                player.states.Change<IdleState>();
            }
        }
    }
}