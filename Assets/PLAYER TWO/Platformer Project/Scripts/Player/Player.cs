using UnityEngine;

public class Player : Entity<Player>
{
    public PlayerEvents playerEvents;

    public PlayerInputManager input {  get; protected set; }

    public PlayerStatsManager stats { get; protected set; }

    public int jumpCounter { get; protected set; }

    public int airDashCounter { get; protected set; }

    public int airSpinCounter { get; protected set; }

    public float lastDashTime { get; protected set; }

    public bool onWater {  get; protected set; }

    public Health health { get; protected set; }

    public Pole pole { get; protected set; }

    public Transform skin;

    public bool holding { get; protected set; }

    protected const float k_waterExitOffSet = 0.25f;

    public Vector3 lastWallNormal {  get; protected set; }

    public Collider water {  get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeInput();
        InitializeStats();
        InitialHealth();
        InitialTag();
        entityEvents.OnGroundEnter.AddListener(() => 
        { 
            ResetJumps(); 
            ResetAirDash();
            ResetAirSpin();
        });
    }

    protected virtual void InitialTag() => tag = GameTags.Player;

    protected virtual void InitialHealth() => health = GetComponent<Health>();  

    protected virtual void InitializeInput() => input = GetComponent<PlayerInputManager>();

    protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();

    public virtual void Accelerate(Vector3 direction)
    {
        //var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        //var acceleration = isGrounded && input.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        //var finalAcceleration = isGrounded ? acceleration : stats.current.acceleration;
        //var topSpeed = input.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;

        var turningDrag = stats.current.turningDrag;
        var acceleration = stats.current.acceleration;
        var finalAcceleration = acceleration;
        var topSpeed = stats.current.topSpeed;

        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);
    }

    public virtual void BackFlipAccelerate()
    {
        var direction = input.GetMovementCameraDirection();
        Accelerate(direction, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
    }

    public virtual void CrawlingAccelerate(Vector3 direction) =>
        Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);

    public virtual void WaterAccelerate(Vector3 direction) =>
        Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);

    public virtual void Decelerate() => Decelerate(stats.current.deceleration);

    public virtual void Friction()
    {
        if (OnSlopingGround())
            Decelerate(stats.current.slopeFriction);
        else 
            Decelerate(stats.current.friction);
    }

    public virtual void Gravity()
    {
        if(!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;
            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }

    public virtual void AccelerateToInputDirection()
    {
        var inputDirection = input.GetMovementCameraDirection();
        Accelerate(inputDirection);
    }

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

    public virtual void SetJumps(int amount) => jumpCounter = amount;

    public virtual void ResetJumps() => jumpCounter = 0;

    public virtual void ResetAirDash() => airDashCounter = 0;

    public virtual void ResetAirSpin() => airSpinCounter = 0;   

    public virtual void Jump()
    {    
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        var canCoyoteJump = (jumpCounter == 0) && (Time.deltaTime < lastGroundTime + stats.current.coyoteJumpThreshold);
        if (canMultiJump || canCoyoteJump || isGrounded) 
        {
            if (input.GetJumpDown())
            {
                Jump(stats.current.maxJumpHeight);
            }
        }
        if (input.GetJumpUp() && jumpCounter > 0 && verticalVelocity.y > stats.current.maxJumpHeight)
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }

    public virtual void Fall()
    {
        if (!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }

    public virtual void Jump(float height)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        states.Change<FallPlayerState>();
        playerEvents.OnJump?.Invoke();
    }

    public override void ApplyDamage(int damage, Vector3 origin)
    {
         if(!health.IsEmpty && !health.recovering)
        {
            health.Damage(damage);
            var damageDir = origin - transform.position;
            damageDir.y = 0;
            damageDir.Normalize();
            FaceDirection(damageDir);
            lateralvelocity = -transform.forward * stats.current.hurtBackwardsForce;
            if (!onWater)
            {
                verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                states.Change<HurtPlayerState>();
            }
            playerEvents.OnHurt?.Invoke();
            if (health.IsEmpty)
            {
                
            }
        }
    }

    public virtual void Dash()
    {
        var canAirdash = stats.current.canAirDash && !isGrounded &&
                         airDashCounter < stats.current.allowedAirDashes;
        var canGroundDash = stats.current.canGroundDash && isGrounded && 
                            Time.time - lastDashTime > stats.current.groundDashCoolDown;
        if(input.GetDashDown() && (canAirdash || canGroundDash))
        {
            if (!isGrounded) airDashCounter++;
            lastDashTime = Time.time;
            states.Change<DashPlayerState>();
        }                     
    }

    public virtual void DirectionalJump(Vector3 direction, float distance, float height)
    {
        jumpCounter++;
        lateralvelocity = direction * distance;
        verticalVelocity = Vector3.up * height;
        playerEvents.OnJump?.Invoke();
    }

    public virtual void WallDrag(Collider other) 
    {
        if(stats.current.canWallDrag && !holding && velocity.y <= 0 && !other.TryGetComponent<Rigidbody>(out _))
        {
            if(CapsuleCast(transform.forward, 0.25f, out var hit, stats.current.wallDragLayers))
            {
                if (hit.collider.CompareTag(GameTags.Platform))
                    transform.parent = hit.transform;
                lastWallNormal = hit.normal;
                states.Change<WallDragPlayerState>();
            }
        }
    }

    public virtual void GrabPole(Collider other)
    {
        if(stats.current.canPoleClimb && velocity.y <= 0 && !holding && other.TryGetComponent(out Pole pole))
        {
            this.pole = pole;
            states.Change<PoleClimbingPlayerState>();
        }
    }

    public virtual void Glide()
    {
        if(!isGrounded && input.GetGlide() && stats.current.canGlide && verticalVelocity.y <= 0)
        {
            states.Change<GlidePlayerState>();
        }
    }

    public virtual void AirDive()
    {
        if(stats.current.canAirDash && !isGrounded && !holding && input.GetAirDiveDown())
        {
            states.Change<AirDivePlayerState>();
            playerEvents.OnAirDive?.Invoke();
        }
    }

    public virtual void SpinAttack()
    {
        var canSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;
        if(stats.current.canAirSpin && canSpin && !holding && input.GetSpinDown())
        {
            if (!isGrounded)
            {
                airSpinCounter++;
            }
            states.Change<SpinPlayerState>();
            playerEvents.OnSpin?.Invoke();
        }
    }

    public virtual void StomAttack()
    {
        if(!isGrounded && stats.current.canStompAttack && input.GetStompDown())
        {
            states.Change<StompPlayerState>();
        }
    }

    public virtual void Backflip(float force)
    {
        if(stats.current.canBackflip && !holding)
        {
            verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
            lateralvelocity = -transform.forward * force;
            states.Change<BackFlipPlayerState>();
            playerEvents.OnBackflip?.Invoke();
        }
    }

    protected virtual void EnterWater(Collider water)
    {
        if (!onWater && !health.IsEmpty)
        {
            onWater = true;
            this.water = water;
            states.Change<SwimPlayerState>();
        }
    }

    protected virtual void ExitWater()
    {
        onWater = false;
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.VolumeWater))
        {
            if (!onWater && other.bounds.Contains(unsizedPosition))
            {
                EnterWater(other);
            }
            else if (onWater)
            {
                var exitPoint = position + Vector3.down * k_waterExitOffSet;
                if (!other.bounds.Contains(exitPoint))
                {
                    ExitWater();
                }
            }
        }
    }

    public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);
    
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);
    public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);
}
