using UnityEngine;
public class CrawingPlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
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
        player.Jump();
        player.Fall();

        var inputDirection = player.input.GetMovementCameraDirection();

        if(player.input.GetCrouchAndCraw() || !player.canStandUp)
        {
            if(inputDirection.sqrMagnitude > 0)
            {
                player.CrawlingAccelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralvelocity);
            }
            else
            {
                player.Decelerate(player.stats.current.crawlingFriction);
            }
        }
        else
        {
            player.states.Change<IdleState>(); 
        }
    }
}