using UnityEngine;
public class DashPlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero;
        player.lateralvelocity = player.transform.forward * player.stats.current.dashForce;
        player.playerEvents.OnDashStarted?.Invoke();
    }

    protected override void OnExit(Player player)
    {
        player.lateralvelocity = Vector3.ClampMagnitude(player.lateralvelocity, player.stats.current.topSpeed);
        player.playerEvents.OnDashEnded?.Invoke();
    }

    protected override void OnStep(Player player)
    {
        player.Jump();
        if(timeSinceEntered > player.stats.current.dashDuration)
        {
            if (player.isGrounded)
                player.states.Change<IdleState>();
            else 
                player.states.Change<FallPlayerState>();
        }
    }
}