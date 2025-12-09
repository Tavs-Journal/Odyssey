using UnityEngine;
public class StompPlayerState : PlayerState
{
    protected float m_airTimer;
    protected float m_groundTimer;

    protected bool m_falling;
    protected bool m_landing;

    protected override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        m_falling = m_landing = false;
        m_airTimer = m_groundTimer = 0;
        player.velocity = Vector3.zero;
        player.playerevents.OnStompStarted?.Invoke();
    }

    protected override void OnExit(Player player)
    {
        player.playerevents.OnStompEnding?.Invoke();
    }

    protected override void OnStep(Player player)
    {
        if (!m_falling)
        {
            m_airTimer += Time.deltaTime;
            if(m_airTimer > player.stats.current.stompAirTime)
            {
                m_falling = true;
                player.playerevents.OnStompFalling?.Invoke();
            }
        }
        else
        {
            player.verticalVelocity += Vector3.down * player.stats.current.stompDownwardForce;
        }
        if (player.isGrounded)
        {
            if (m_landing)
            {
                m_landing = true;
                player.playerevents.OnStompLanding?.Invoke();
            }
            if(m_groundTimer > player.stats.current.stompGroundTime)
            {
                player.verticalVelocity = Vector3.up * player.stats.current.stompGroundLeapHeight;
                player.states.Change<FallPlayerState>();
            }
            else
            {
                m_groundTimer += Time.deltaTime;
            }
        }
    }
}