using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour { }
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
            Debug.Log("change");
            var rotation = transform.rotation;
            var rotationDelta = degreesPersecond * Time.deltaTime;
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta); 
        }
    }

    protected virtual void HandleController()
    {
        transform.position += velocity * Time.deltaTime;
    }
    protected virtual void HandleState() => states.Step();

    protected virtual void Awake()
    {
        InitializeStateManager();
    }
    
    protected virtual void Update()
    {
        HandleState();
        HandleController();
    }
}
