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
        var inputDirection = player.input.GetMovementCameraDirection();

        if(inputDirection.sqrMagnitude > 0 )
        {
            var dot = Vector3.Dot(inputDirection, player.lateralvelocity);

            if(dot >= player.stats.current.brakeThreshold)
            {
                player.Accelerate(inputDirection);
                Debug.Log("move");
            }
        }
    }
}
