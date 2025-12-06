using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour { 

    public EntityEvents entityEvents;
    public Vector3 unsizedPosition => transform.position;
    public bool isGrounded { get; protected set; } = true;
    public readonly float m_groundOffSet = 0.1f;

    public CharacterController controller {  get; protected set; }

    public float originalHeight {  get; protected set; }

    public float lastGroundTime { get; protected set; }

    public float groundAngel { get; protected set; }

    public RaycastHit groundHit;

    public Vector3 groundNormal {  get; protected set; }

    public Vector3 localSlopeDirection {  get; protected set; }

    public Vector3 position => transform.position + center;

    public Vector3 center => controller.center;
    public float height => controller.height;
    public float radius => controller.radius;

    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);

    public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;     

    public virtual bool OnSlopingGround()
    {
        return false;
    }

    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers, 
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
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

    protected virtual bool EvaluateLanding(RaycastHit hit)
    {
        return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
    }

    protected virtual void EnterGround(RaycastHit hit)
    {
        if (!isGrounded)
        {
            groundHit = hit;
            isGrounded = true;
            entityEvents.OnGroundEnter?.Invoke();
        }
    }

    protected virtual void ExitGround(RaycastHit hit)
    {
        if (isGrounded)
        {
            isGrounded = false;
            transform.parent = null;
            lastGroundTime = Time.time;
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    public virtual void SnapToGround(float force)
    {
        if(isGrounded && (verticalVelocity.y) <= 0)
        {
            verticalVelocity = Vector3.down * force;
        }
    }

    protected virtual void UpdateGround(RaycastHit hit)
    {
        if (isGrounded)
        {
            groundHit = hit;
            groundNormal = groundHit.normal;
            groundAngel = Vector3.Angle(Vector3.up, groundNormal);
            localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z);
            transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
        }
    }

    protected virtual void HandleGround()
    {
        var distance = (height * 0.5f) + m_groundOffSet;
        if(SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            if (!isGrounded)
            {
                if (EvaluateLanding(hit))
                {
                    EnterGround(hit);
                }
            }
            else if (IsPointUnderStep(hit.point)){
                UpdateGround(hit);
            }
        }
        else
        {
            ExitGround(hit);
        }      
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
        if (controller.enabled)
        {
            HandleState();
            HandleController();
            HandleGround();
        }
    }
}
