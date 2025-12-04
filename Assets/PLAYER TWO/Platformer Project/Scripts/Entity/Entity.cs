using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour { 
    public Vector3 unsizedPosition => transform.position;
    public bool isGrounded { get; protected set; } = true;

    public CharacterController controller {  get; protected set; }

    public float originalHeight {  get; protected set; }

    public float lastGroundTime { get; protected set; }

    public virtual bool OnSlopingGround()
    {
        return false;
    }
}
public abstract class Entity<T> :EntityBase where T :Entity<T>
{
    public EntityStateManager<T> states {  get; private set; }

    public Vector3 velocity { get; set; }

    public float accelerationMultiplier { get; set; } = 1f;

    public float gravityMultiplier { get; set; } = 1f;

    public float topSpeedMultiplier {  get; set; } = 1f;

    public float turningDragMultiplier {  get; set; } = 1f;

    public float decelerationMultiplier { get; set; } = 1f;

    public Vector3 lateralvelocity
    {
        get { return new Vector3(velocity.x, 0, velocity.z); }
        set { velocity = new Vector3(value.x, velocity.y, value.z);}
    }

    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }

    protected virtual void InitializeController()
    {
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        controller.skinWidth = 0.005f;
        controller.minMoveDistance = 0;
        originalHeight = controller.height;
    }

    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float TopSpeed)
    {
        var speed = Vector3.Dot(direction, lateralvelocity);
        var velocity =  direction * speed;
        var turningVelocity = lateralvelocity - velocity;
        var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
        var targetTopSpeed = TopSpeed * topSpeedMultiplier;

        if(lateralvelocity.magnitude <  targetTopSpeed || speed < 0)
        {
            speed += acceleration * accelerationMultiplier * Time.deltaTime;
            speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
        }

        velocity = direction * speed;

        turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);

        lateralvelocity = velocity + turningVelocity;
    }

    public virtual void FaceDirection(Vector3 direction, float degreesPersecond)
    {
        if (direction != Vector3.zero)
        {           
            var rotation = transform.rotation;
            var rotationDelta = degreesPersecond * Time.deltaTime;
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta); 
        }
    }

    public virtual void Decelerate(float deceleration)
    {
        var delta = deceleration * decelerationMultiplier * Time.deltaTime;
        lateralvelocity = Vector3.MoveTowards(lateralvelocity, Vector3.zero, delta);
    }

    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(velocity *  Time.deltaTime);
            return;
        }
        transform.position += velocity * Time.deltaTime;
    }
    protected virtual void HandleState() => states.Step();

    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeController();
    }
    
    protected virtual void Update()
    {
        HandleState();
        HandleController();
    }
}
