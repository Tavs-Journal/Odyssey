using UnityEngine;
public class AirDivePlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
  
    }

    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero;
        player.lateralvelocity = player.transform.forward * player.stats.current.airDiveForwardForce;
    }


    protected override void OnExit(Player player)
    {

    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.Jump();

        player.FaceDirection(player.lateralvelocity);
        if (player.isGrounded)
        {
            var inputDirection = player.input.GetMovementCameraDirection();
            var localInputDirection = player.transform.InverseTransformDirection(inputDirection);
            var rotation = localInputDirection.x * player.stats.current.airDiveRotationSpeed * Time.deltaTime;
            player.lateralvelocity = Quaternion.Euler(0, rotation, 0) * player.lateralvelocity;
        }
        
        player.Decelerate(player.stats.current.airDiveFriction);
        if(player.lateralvelocity.sqrMagnitude == 0)
        {
            player.verticalVelocity = Vector3.up * player.stats.current.airDiveGroundLeapHeight;
            player.states.Change<FallPlayerState>();    
        }
    }
}
