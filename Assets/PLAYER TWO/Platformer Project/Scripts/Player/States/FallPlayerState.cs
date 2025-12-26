using UnityEngine;

public class FallPlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
        player.WallDrag(other);
        player.GrabPole(other);
        player.PushRigidBody(other);
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
        player.Dash();
        player.Glide();
        player.AirDive();
        player.LedgeGrab();
        player.StomAttack();
        player.SpinAttack(); 
        player.PickAndThrow();
        player.AccelerateToInputDirection();
        if (player.isGrounded)
        {
            player.states.Change<IdleState>();
        }
    }
}