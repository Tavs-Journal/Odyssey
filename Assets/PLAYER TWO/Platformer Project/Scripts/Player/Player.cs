using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public virtual bool isAlive => !health.isEmpty;

    public Health health { get; protected set; }

    public Pole pole { get; protected set; }

    public Pickable pickable { get; protected set; }

    public Transform skin;

    public Transform pickableSlot;

    public bool holding { get; protected set; }

    public float leanVelocity;

    public bool isSkinLean
    {
        get
        {
            return ((skin.transform.localEulerAngles.z > stats.current.leanOffSet) ||
                (Mathf.Abs(360f - skin.transform.localEulerAngles.z) > stats.current.leanOffSet));
        }
    }

    protected const float k_waterExitOffSet = 0.25f;

    public Vector3 lastWallNormal { get; protected set; }

    protected Vector3 m_skinInitialPosition;
    protected Quaternion m_skinInitialRotation;

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
        entityEvents.OnRailsEnter.AddListener(() =>
        {
            ResetJumps();
            ResetAirDash();
            ResetAirSpin();
            StartGrind();
        });
    }

    protected virtual void InitialTag() => tag = GameTags.Player;

    protected virtual void InitialHealth() => health = GetComponent<Health>();  

    protected virtual void InitializeInput() => input = GetComponent<PlayerInputManager>();

    protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();

    public virtual void Accelerate(Vector3 direction)
    {
        var turningDrag = isGrounded && input.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        var acceleration = isGrounded && input.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        var finalAcceleration = isGrounded ? acceleration : stats.current.acceleration;
        var topSpeed = input.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;

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
    
    public virtual void ResetSkinParent()
    {
        if (skin)
        {
            skin.parent = transform;
            skin.localPosition = m_skinInitialPosition;
            skin.rotation = m_skinInitialRotation;
        }
    }

    public virtual void SetSkinParent(Transform parent)
    {
        if (skin)
        {
            skin.parent = parent;
        }
    }


    public virtual void Jump()
    {    
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
        var holdJump = stats.current.canJumpWhileHolding || !holding;
        if ((canMultiJump || canCoyoteJump || isGrounded || onRails) && holdJump) 
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
         if(!health.isEmpty && !health.recovering)
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
            if (health.isEmpty)
            {
                
            }
        }
    }

    public virtual void PickAndThrow()
    {
        if (stats.current.canPickUp && input.GetPickAndDropDown())
        {
            if (!holding && CapsuleCast(transform.forward, stats.current.pickDistance, out var hit)
                && hit.transform.TryGetComponent(out Pickable pickable))
            {
                PickUp(pickable);
            }
            else
            {
                Throw();
            }
        }
    }

    public virtual void PickUp(Pickable pickable)
    {
        if(!holding && (isGrounded || stats.current.canPickUpOnAir))
        {
            holding = true;
            this.pickable = pickable;
            pickable.PickUp(pickableSlot);
            pickable.onRespawn.AddListener(RemovePickable);
            playerEvents.OnPickUp?.Invoke();
        }
    }

    public virtual void Throw()
    {
        if (holding)
        {
            var force = lateralvelocity.magnitude * stats.current.throwVelocityMultiplier;
            pickable.Release(transform.forward, force);
            pickable = null;
            holding = false;
            playerEvents.OnThrow?.Invoke();
        }
    }

    public virtual void RemovePickable()
    {
        if (holding)
        {
            pickable = null;
            holding = false;
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
            if(CapsuleCast(transform.forward, 0.25f, out var hit, stats.current.wallDragLayers) &&
                !DetectingLedge(0.25f, height, out _))
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

    public virtual void LedgeGrab()
    {
        if(velocity.y < 0 && stats.current.canLedgeHang && !holding && 
            states.ContainsStateOfType(typeof(LedgeHangPlayerState)) &&
            DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
        {
            if(!(hit.collider is SphereCollider) && !(hit.collider is CapsuleCollider))
            {
                var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                var lateralOffset = transform.forward * ledgeDistance;
                var verticalOffset = Vector3.down * height * 0.5f - center;
                velocity = Vector3.zero;
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                transform.position = hit.point - lateralOffset + verticalOffset;

                states.Change<LedgeHangPlayerState>();
                playerEvents.OnLedgeGrabbed?.Invoke();
            }
        }
    }

    public virtual void HandleSkinLean()
    {
        if (stats.current.canLean && !onRails)
        {
            if(states.IsCurrentOfType(typeof(WalkState)) || states.IsCurrentOfType(typeof(GlidePlayerState)))
            {
                bool isWalk = states.IsCurrentOfType(typeof(WalkState));
                var minLean = isWalk ? stats.current.minGroundLeanSpeed : stats.current.minGlideLeanSpeed;
                var maxLeanAngle = isWalk ? stats.current.maxGroundLeanAngle : stats.current.maxGlideLeanAngle;
                var speed = lateralvelocity.magnitude;
                if (speed >= minLean)
                {
                    var targetDirection = input.GetMovementCameraDirection();
                    var moveDirection = lateralvelocity / speed;
                    var angle = Vector3.SignedAngle(targetDirection, moveDirection, Vector3.up);
                    var rot = Mathf.Clamp(angle, -maxLeanAngle, maxLeanAngle);
                    ChangeSkinRotation(rot, stats.current.leanSmoothTime);
                }
                else
                {
                    ChangeSkinRotation(0f, stats.current.leanSmoothTime);
                }
            }
            else if (isSkinLean)
            {
                ChangeSkinRotation(0f, stats.current.leanResetTime);
            }
        }
    }

    public virtual void ChangeSkinRotation(float rot, float leanResetTime)
    {
        var rotation = skin.localEulerAngles;
        rotation.z = Mathf.SmoothDampAngle(rotation.z, rot, ref leanVelocity, leanResetTime);
        skin.transform.localEulerAngles = rotation;
    }

    protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgehit)
    {
        var contactOffset = Physics.defaultContactOffset + positionDelta;
        var ledgeMaxDistance = radius + forwardDistance;
        var ledgeHeightOffset = height * 0.5f + contactOffset;
        var upwardOffset = transform.up * ledgeHeightOffset;
        var forwardOffset = transform.forward * ledgeMaxDistance;

        if(Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance, 
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || 
           Physics.Raycast(position + forwardOffset * 0.01f, transform.up, ledgeHeightOffset, 
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            ledgehit = new RaycastHit();
            return false;
        }

        var origin = position + upwardOffset + forwardOffset;
        var distance = downwardDistance + contactOffset;
        return Physics.Raycast(origin, Vector3.down, out ledgehit, distance, 
            stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
    }

    public virtual bool FitIntoPosition(Vector3 position)
    {
        var radius = controller.radius - controller.skinWidth;
        var offset = height * 0.5f - radius;
        var top = position + Vector3.up * offset;
        var bottom = position - Vector3.up * offset;

        return !Physics.CheckCapsule(top, bottom, radius, 
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
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
        if (!onWater && !health.isEmpty)
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

    public virtual void PushRigidBody(Collider other)
    {
        if(!IsPointUnderStep(other.bounds.max) && other.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            var force = lateralvelocity * stats.current.pushForce;
            rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
        }
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

    public virtual void StartGrind() => states.Change<RailGrindPlayerState>();

    public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);
    
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);
    public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);

    protected override void LateUpdate()
    {
        base.LateUpdate();
        HandleSkinLean();
    }
}
