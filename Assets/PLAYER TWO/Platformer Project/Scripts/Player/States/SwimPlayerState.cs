using Unity.VisualScripting;
using UnityEngine;
public class SwimPlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        player.velocity *= player.stats.current.waterConversion;
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        //这里之后可以做一下修改：
        //任意时刻都可以跳，全部在水里时人上升，否则缓慢下降
        //当露出水面时按下跳则跳出水面
        //这个需要修改浮力的值，并且也许应该应用重力。
        if (player.onWater)
        {
            var inputDirection = player.input.GetMovementCameraDirection();

            player.WaterAccelerate(inputDirection);
            player.WaterFaceDirection(inputDirection);
            
            

            if(player.position.y < player.water.bounds.max.y)
            {
                if (player.isGrounded)
                {
                    player.verticalVelocity = Vector3.zero;
                }
                player.verticalVelocity += Vector3.up * player.stats.current.waterUpwardsForce * Time.deltaTime;
            }
            else
            {
                player.verticalVelocity = Vector3.zero;
                if (player.input.GetJumpDown())
                {
                    player.Jump(player.stats.current.waterJumpHeight);
                    player.states.Change<FallPlayerState>();
                }
            }
            if (!player.isGrounded && player.input.GetDive())
            {
                player.verticalVelocity += Vector3.down * player.stats.current.swimDiveForce * Time.deltaTime;
            }
            if (inputDirection.sqrMagnitude == 0)
            {
                player.Decelerate(player.stats.current.swimDeceleration);
            }
        }
        else
        {
            player.states.Change<WalkState>();
        }
    }
}