using UnityEngine;
public class BackFlipPlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {

    }

    protected override void OnEnter(Player player)
    {
        player.SetJumps(1);
        player.playerevents.OnJump?.Invoke();
        if (player.stats.current.canBackflip)
        {
            player.input.LockMovementDirection();
        }
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        player.Gravity(player.stats.current.backflipGravity);

        player.BackFlipAccelerate();

        if (player.isGrounded)
        {
            player.lateralvelocity = Vector3.zero;
            player.states.Change<IdleState>();
        }   
    }
}