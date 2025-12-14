using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
public class PoleClimbingPlayerState : PlayerState
{

    protected float m_collisionRadius;

    protected const float k_poleOffSet = 0.01f;

    public override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        player.ResetAirDash();
        player.ResetJumps();
        player.ResetAirSpin();
        player.velocity = Vector3.zero;

        player.pole.GetDirectionToPole(player.transform, out m_collisionRadius);

        player.skin.position += player.transform.rotation * player.stats.current.poleClimbSkinOffset;
    }

    protected override void OnExit(Player player)
    {
        player.skin.position -= player.transform.rotation * player.stats.current.poleClimbSkinOffset;
    }

    protected override void OnStep(Player player)
    {
        var toPoleDirection = player.pole.GetDirectionToPole(player.transform);

        var inputdirection = player.input.GetMovementDirection();

        player.FaceDirection(toPoleDirection);  

        player.lateralvelocity = player.transform.right * inputdirection.x * player.stats.current.climbRotationSpeed;

        if(inputdirection.z != 0)
        {
            var speed = inputdirection.z > 0 ?
                player.stats.current.climbUpSpeed : -player.stats.current.climbDownSpeed;

            player.verticalVelocity = Vector3.up * speed;
        }
        else
        {
            player.verticalVelocity = Vector3.zero;
        }

        if (player.input.GetJumpDown())
        {
            player.FaceDirection(-toPoleDirection);
            player.DirectionalJump(-toPoleDirection, 
                player.stats.current.poleJumpDistance, player.stats.current.poleJumpHeight);
            player.states.Change<FallPlayerState>();
        }
        if (player.isGrounded)
        {
            player.states.Change<IdleState>();
        }

        var offset = player.height * 0.5f + player.center.y;
        var center = new Vector3(player.pole.center.x, player.transform.position.y, player.pole.center.z);
        var position = center - toPoleDirection * m_collisionRadius;
        player.transform.position = player.pole.ClampPointToPoleHeight(position, offset);
    }
}