using UnityEngine;
public class GlidePlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
        player.WallDrag(other);
        player.GrabPole(other);
    }

    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero;
        player.playerEvents.OnGlidingStart.Invoke();
    }

    protected override void OnExit(Player player)
    {
        player.playerEvents.OnGlidingStop.Invoke();
    }

    protected override void OnStep(Player player)
    {
        var inputdirection = player.input.GetMovementCameraDirection();

        HandleGlidingGravity(player);

        player.FaceDirection(player.lateralvelocity);

        player.Accelerate(inputdirection, player.stats.current.glidingTurningDrag,
               player.stats.current.airAcceleration, player.stats.current.topSpeed);

        player.LedgeGrab();

        if (player.isGrounded)
        {
            player.states.Change<IdleState>();
        }
        else if (!player.input.GetGlide())
        {
            player.states.Change<FallPlayerState>();
        }
    }
     protected virtual void HandleGlidingGravity(Player player)
    {
        var yVelocity = player.verticalVelocity.y;
        yVelocity -= player.stats.current.glidingGravity * Time.deltaTime;
        yVelocity = Mathf.Max(yVelocity, -player.stats.current.glidingMaxFallSpeed);
        player.verticalVelocity = new Vector3(0, yVelocity, 0);
    }
}
