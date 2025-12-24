using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Splines;

public abstract class EntityBase : MonoBehaviour { 

    public EntityEvents entityEvents;
    public Vector3 unsizedPosition => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;

    protected Collider[] m_colliders = new Collider[10];

    protected CapsuleCollider m_collider;

    protected Rigidbody m_rigidbody;

    public bool isGrounded { get; protected set; } = true;

    public bool onRails;

    public readonly float m_groundOffSet = 0.1f;

    public CharacterController controller {  get; protected set; }

    public Vector3 velocity { get; set; }

    public Vector3 lateralvelocity
    {
        get { return new Vector3(velocity.x, 0, velocity.z); }
        set { velocity = new Vector3(value.x, velocity.y, value.z); }
    }

    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }

    public float originalHeight {  get; protected set; }

    public float lastGroundTime { get; protected set; }

    public float groundAngel { get; protected set; }

    public float positionDelta { get; set; } 
    
    public Vector3 laterPosition { get; protected set; }

    public RaycastHit groundHit;

    public SplineContainer rails { get; protected set; }

    public Vector3 groundNormal {  get; protected set; }

    public Vector3 localSlopeDirection {  get; protected set; }

    public Vector3 position => transform.position + center;

    public Vector3 center => controller.center;
    public float height => controller.height;
    public float radius => controller.radius;

    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);

    public const int normalsLength = 10;

    public Vector3[] normals = new Vector3[normalsLength];

    public int normalIndex;

    public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;     

    public virtual void ApplyDamage(int damage, Vector3 origin) { }

    public virtual bool OnSlopingGround()
    {
        return false;
    }

    public virtual void FaceDirection(Vector3 direction)
    {
        if(direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = rotation;
        }
    }

    public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return CapsuleCast(direction, distance, out _,  layer, queryTriggerInteraction);
    }

    public virtual bool CapsuleCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers, 
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var origin = position - direction * radius + center;
        var offset = transform.up * (height * 0.5f- radius);
        var top = origin + offset;
        var bottom = origin - offset;
        return Physics.CapsuleCast(top, bottom, radius, direction,
            out hit, distance + radius, layer, queryTriggerInteraction);
    }

    public virtual int OverlapEntity(Collider[] result, float skinOffSet = 0f)
    {
        var contactOffSet = skinOffSet + controller.skinWidth + Physics.defaultContactOffset;
        var overlapRadius = radius + contactOffSet;
        var offset = (height + contactOffSet) * 0.5f - overlapRadius;
        var top = position + Vector3.up * offset;
        var bottom = position + Vector3.down * offset;
        return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapRadius, result);
    }

    public virtual bool SphereCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers, 
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
    }

    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers, 
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
    }

    public virtual void ReSizeCollider(float height)
    {
        var delta = height - this.height;
        controller.height = height;
        controller.center += Vector3.up * delta * 0.5f;
    }
}
public abstract class Entity<T> :EntityBase where T :Entity<T>
{
    public EntityStateManager<T> states {  get; private set; }

    public float accelerationMultiplier { get; set; } = 1f;

    public float gravityMultiplier { get; set; } = 1f;

    public float topSpeedMultiplier {  get; set; } = 1f;

    public float turningDragMultiplier {  get; set; } = 1f;

    public float decelerationMultiplier { get; set; } = 1f;

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

    protected virtual void InitializeRigidbody()
    {
        m_rigidbody = gameObject.AddComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;
    }

    protected virtual void InitializeCollider()
    {
        m_collider = gameObject.AddComponent<CapsuleCollider>();
        m_collider.height = controller.height;
        m_collider.radius = controller.radius;
        m_collider.center = controller.center;
        m_collider.isTrigger = controller.isTrigger;
        m_collider.enabled = false;
    }

    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float TopSpeed)
    {
        if(direction.sqrMagnitude > 0)
        {
            var speed = Vector3.Dot(direction, lateralvelocity);
            var velocity = direction * speed;
            var turningVelocity = lateralvelocity - velocity;
            var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            var targetTopSpeed = TopSpeed * topSpeedMultiplier;

            if (lateralvelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += acceleration * accelerationMultiplier * Time.deltaTime;
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }

            velocity = direction * speed;

            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);

            lateralvelocity = velocity + turningVelocity;
        }
    }

    public virtual void Gravity(float gravity)
    {
        if (!isGrounded)
        {
            verticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
        }
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

    public virtual void UseCustomCollision(bool value)
    {
        controller.enabled = !value;
        if (value)
        {
            InitializeCollider();
            InitializeRigidbody();
        }
        else
        {
            Destroy(m_collider);
            Destroy(m_rigidbody);
        }
    }

    protected virtual void EnterRails(SplineContainer rails)
    {
        if (!onRails)
        {
            onRails = true;
            this.rails = rails;
            entityEvents.OnRailsEnter?.Invoke();
        }
    }

    public virtual void ExitRails()
    {
        if (onRails)
        {
            onRails = false;
            entityEvents.OnRailsExit?.Invoke();
        }
    }

    protected virtual void OnContact(Collider other)
    {
        if (other)
        {
            states.OnContact(other);
        }
    }

    protected virtual void HandleGround()
    {
        if (onRails) return;
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
            else if (IsPointUnderStep(hit.point))
            {
                UpdateGround(hit);
            }
        }
        else
        {
            ExitGround(hit);
        }      
    }

    protected virtual void HandleNormals()
    {
        if (!isGrounded)
        {
            var baseVelocity = velocity;
            for (int i = 0; i < normalIndex; i++)
            {
                var normal = normals[i];
                var normalWithUp = Vector3.Angle(Vector3.up, normal);
                if (normalWithUp > 110f) continue;
                var delta = Vector3.Dot(baseVelocity, normal);
                if(delta < 0)
                {
                    var target = baseVelocity - normal * delta;
                    baseVelocity = Vector3.MoveTowards(baseVelocity, target, 5f * Time.deltaTime);
                }
            }
            velocity = baseVelocity;
        }
        normalIndex = 0;
    }

    protected virtual void HandleContacts()
    {
        var overlaps = OverlapEntity(m_colliders, 0.01f);

        for(int i = 0; i < overlaps; i++)
        {
            if (!m_colliders[i].isTrigger && m_colliders[i].transform != transform)
            {
                OnContact(m_colliders[i]);

                var listeners = m_colliders[i].GetComponents<IEntityContact>();
                foreach(var contact in listeners)
                {
                    contact.OnEntityContact((T)this);
                }
                //这里可以添加一个角色y方向速度大于零
                //也就是速度上升的时候且撞到时才会触发
                if (m_colliders[i].bounds.min.y > controller.bounds.max.y)
                {
                    verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
                }
            }
        }
    }

    protected virtual void HandleSpline()
    {
        var distance = (height * 0.5f) + height * 0.5f;
        if(SphereCast(-transform.up, distance, out var hit) && hit.collider.CompareTag(GameTags.InteractiveRail))
        {
            if(!onRails && verticalVelocity.y <= 0)
            {
                EnterRails(hit.collider.GetComponent<SplineContainer>());
            }
        }
        else
        {
            ExitRails();
        }
    }

    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(velocity * Time.deltaTime);
            return;
        }
        transform.position += velocity * Time.deltaTime;
    }

    protected virtual void HandlePosition()
    {
        positionDelta = (position - laterPosition).magnitude;
        laterPosition = position;
    }

    protected virtual void HandleState() => states.Step();

    protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(normalIndex < normalsLength - 1 && verticalVelocity.y < 0)
        {
            normals[normalIndex++] = hit.normal;
        }
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeController();
    }
    
    protected virtual void Update()
    {
        if (controller.enabled || m_collider != null)
        {
            HandleState();
            HandleController();
            HandleGround();
            HandleSpline();
            HandleContacts();
            HandleNormals();
        }
    }

    protected virtual void LateUpdate()
    {
        if (controller.enabled)
        {
            HandlePosition();
        }
    }
}
