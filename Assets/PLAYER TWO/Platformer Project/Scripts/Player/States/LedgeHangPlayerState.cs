using System.Collections;
using UnityEngine;
public class LedgeHangPlayerState : PlayerState
{
    protected bool m_keepParent;

    protected Coroutine m_clearParentRoutine;

    protected const float k_clearParentDelay = 0.25f;

    public override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        if(m_clearParentRoutine != null)
        {
            player.StopCoroutine(m_clearParentRoutine);
        }
        m_keepParent = false;
        player.skin.position += player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
        player.ResetAirDash();
        player.ResetJumps();
        player.ResetAirSpin();
    }

    protected override void OnExit(Player player)
    {
        m_clearParentRoutine = player.StartCoroutine(ClearParentRoutine(player));
        player.skin.position -= player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
    }

    protected override void OnStep(Player player)
    {
        var ledgeTopMaxDistance = player.radius + player.stats.current.ledgeMaxForwardDistance;
        var ledgeTopHeightOffset = player.height * 0.5f + player.stats.current.ledgeMaxDownwardDistance;
        var topOrigin = player.position + Vector3.up * ledgeTopHeightOffset +
                player.transform.forward * ledgeTopMaxDistance;

        var sideOrigin = player.position + Vector3.up * player.height * 0.5f + 
                Vector3.down * player.stats.current.ledgeSideHeightOffset;
        var rayDistance = player.radius + player.stats.current.ledgeSideMaxDistance;
        var rayRadius = player.stats.current.ledgeSideCollisionRadius;

        if(Physics.SphereCast(sideOrigin, rayRadius,player.transform.forward, out var sideHit, rayDistance,
            player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore) && 
           Physics.Raycast(topOrigin, Vector3.down, out var topHit, player.height, 
            player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
        {
            var inputDirection = player.input.GetMovementDirection();
            var ledgeSideOrigin = sideOrigin + player.transform.right * Mathf.Sign(inputDirection.x) * player.radius;
            var sideForward = -new Vector3(sideHit.normal.x, 0, sideHit.normal.z).normalized;
            var ledgeHeight = topHit.point.y - player.height * 0.5f;
            player.FaceDirection(sideForward);
            if(Physics.Raycast(ledgeSideOrigin, sideForward, rayDistance, 
                player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
            {
                player.lateralvelocity = player.transform.right * inputDirection.x * player.stats.current.ledgeMovementSpeed;
            }
            else
            {
                player.lateralvelocity = Vector3.zero;
            }
            player.transform.position = new Vector3(sideHit.point.x, ledgeHeight, sideHit.point.z) -
                player.center - sideForward * player.radius;

            if (player.input.GetReleaseLedgeDown())
            {
                player.FaceDirection(-sideForward);
                player.states.Change<FallPlayerState>();
            }
            else if (player.input.GetJumpDown())
            {
                player.Jump(player.stats.current.maxJumpHeight);
                player.states.Change<FallPlayerState>();
            }
            
        }
        else
        {
            player.states.Change<FallPlayerState>();
        }
    }

    protected virtual IEnumerator ClearParentRoutine(Player player)
    {
        if(m_keepParent) yield break;
        yield return new WaitForSeconds(k_clearParentDelay);
        player.transform.parent = null;
    }
}