using TMPro;
using UnityEngine;
[RequireComponent (typeof(Player))]
public class PlayerParticle : MonoBehaviour
{
    [Header("速度设置")]
    public float walkDustMinSpeed = 3.5f;
    public float landingPaiticleMinSpeed = 5f;

    [Header("粒子特效引用")]
    public ParticleSystem walkDust;
    public ParticleSystem landDust;
    public ParticleSystem hurtDust;
    public ParticleSystem dashDust;
    public ParticleSystem speedTrails;
    public ParticleSystem grindTrails;

    protected Player player;

    protected virtual void Start()
    {
        player = GetComponent<Player>();
        player.entityEvents.OnGroundEnter.AddListener(HandleLandParticle);
        player.playerEvents.OnHurt.AddListener(HandleHurtParticle);
        player.playerEvents.OnDashStarted.AddListener(OnDashStarted);
        player.playerEvents.OnDashEnded.AddListener(() => Stop(speedTrails, true));
    }

    protected virtual void Update()
    {
        HandleWalkParticle();
        HandleRailParticle();
    }

    protected virtual void HandleWalkParticle()
    {
        if(player.isGrounded && !player.onWater && !player.onRails)
        {
            if (player.lateralvelocity.x > walkDustMinSpeed)
            {
                Play(walkDust);
            }
            else
            {
                Stop(walkDust);
            }
        }
        else
        {
            Stop(walkDust);
        }
    }

    protected virtual void HandleRailParticle()
    {
        if (player.onRails)
        {
            Play(grindTrails);
        }
        else
        {
            Stop(grindTrails, true);
        }
    }

    protected virtual void HandleLandParticle()
    {
        if(!player.onWater && Mathf.Abs(player.velocity.y) >= landingPaiticleMinSpeed)
        {
            Play(landDust);
        }
    }

    protected virtual void HandleHurtParticle() => Play(hurtDust);

    protected virtual void OnDashStarted()
    {
        Play(dashDust);
        Play(speedTrails);
    }

    protected virtual void Stop(ParticleSystem partical, bool clear = false)
    {
        if (partical.isPlaying)
        {
            var mode = clear ? ParticleSystemStopBehavior.StopEmittingAndClear :
                ParticleSystemStopBehavior.StopEmitting;
            partical.Stop(true, mode);
        }
    }

    protected virtual void Play(ParticleSystem particle)
    {
        if (!particle.isPlaying)
        {
            particle.Play();
        }
    }
}