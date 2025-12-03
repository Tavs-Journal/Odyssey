using UnityEngine;

public class WalkState : PlayerState
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

        var inputDirection = player.input.GetMovementCameraDirection();

        if(inputDirection.sqrMagnitude > 0 )
        {
            var dot = Vector3.Dot(inputDirection, player.lateralvelocity);

            if(dot >= player.stats.current.brakeThreshold)
            {
                player.Accelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralvelocity);
            }
            else
            {
                player.states.Change<BrakePlayerState>();
            }
        }
        else
        {
            player.Friction();
            if (player.lateralvelocity.sqrMagnitude <= 0)
            {
                player.states.Change<IdleState>();
            }                
        }
    }
}
