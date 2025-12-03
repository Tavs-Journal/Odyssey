using UnityEngine;

public class Player : Entity<Player>
{
    public PlayerInputManager input {  get; protected set; }

    public PlayerStatsManager stats { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeInput();
        InitailizeStats();
    }

    protected virtual void InitializeInput() => input = GetComponent<PlayerInputManager>();

    protected virtual void InitailizeStats() => stats = GetComponent<PlayerStatsManager>();

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
        isGrounded = false;
        if(!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;
            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);
}
