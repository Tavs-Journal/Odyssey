using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity<Enemy>
{
    public Player player {  get; protected set; }
    public Health health { get; protected set; }
    public EnemyEvent enemyevents;
    public EnemyStatsManager stats { get; protected set; }
    public WayPointManager wayPoints { get; protected set; }
    protected Collider[] m_sightOverlaps = new Collider[200];
    protected Collider[] m_contactAttackOverlaps = new Collider[200];

    protected override void Awake()
    {
        base.Awake();
        InitializeTag();
        InitializeHealth();
        InitializeWayPointManager();
        InitializeEnemyStatsManager();
    }

    protected virtual void InitializeTag() => tag = GameTags.Enemy;
    protected virtual void InitializeHealth() => health = GetComponent<Health>();
    protected virtual void InitializeEnemyStatsManager() => stats = GetComponent<EnemyStatsManager>();    
    protected virtual void InitializeWayPointManager() => wayPoints = GetComponent<WayPointManager>();

    protected override void OnUpdate()
    {
        HandleSight();
        ContactAttack();
    }

    public override void ApplyDamage(int damage, Vector3 origin)
    {
        if(!health.isEmpty && !health.recovering)
        {
            health.Damage(damage);
            enemyevents.OnDamage?.Invoke();
            if (health.isEmpty)
            {
                controller.enabled = false;
                enemyevents.OnDie?.Invoke();
            }
        }
    }

    public virtual void HandleSight()
    {
        if (!player)
        {
            var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);
            for(int i = 0; i < overlaps; i++)
            {
                if (m_sightOverlaps[i].TryGetComponent(out Player player))
                {
                    this.player = player;
                    enemyevents.OnPlayerSpotted?.Invoke();
                    return;
                }
            }
        }
        else
        {
            var distance = Vector3.Distance(position, player.position);
            if((player.health.current == 0) || distance > stats.current.viewRange)
            {
                player = null;
                enemyevents.OnPlayerScaped?.Invoke();
            }
        }
    }

    public virtual void ContactAttack()
    {
        if (stats.current.canAttackOnContact)
        {
            var overlaps = OverlapEntity(m_contactAttackOverlaps, stats.current.contactOffset);
            for(int i = 0; i < overlaps; i++)
            {
                if(m_contactAttackOverlaps[i].TryGetComponent(out Player player))
                {
                    var step = controller.bounds.max - Vector3.down * stats.current.contactSteppingTolerance;
                    if (!player.IsPointUnderStep(step))
                    {
                        if (stats.current.contactPushback)
                        {
                            lateralvelocity = -transform.forward * stats.current.contactPushBackForce;
                        }
                        player.ApplyDamage(stats.current.contactDamage, transform.position);
                        enemyevents.OnPlayerContact?.Invoke();
                    }
                }
            }
        }
    }

    public virtual void Gravity() => Gravity(stats.current.gravity);
    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
    public virtual void Friction() => Decelerate(stats.current.friction);
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed); 
    public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) => 
        Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);
}
