using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class Pickable : MonoBehaviour, IEntityContact
{
    [Header("General Setting")]
    public Vector3 localPositionOffSet;
    public Quaternion localRotationOffSet;
    public float releaseOffSet;

    [Header("Respawn Settings")]
    public bool autoRespawn;
    public bool respawnOnHitHazard;
    public float respawnHeightLitmit = -100;

    [Header("Attack Settings")]
    public bool attackEnemies = true;
    public int damage = 1;
    public float minDamageSpeed = 5f;

    [Space(15)]

    public UnityEvent onPicked;
    public UnityEvent onReleased;
    public UnityEvent onRespawn;

    public bool beHold { get; protected set; }

    protected Collider m_collider;
    protected Rigidbody m_rigidBody;

    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected Transform initialParent;

    protected RigidbodyInterpolation m_interpolation;
    protected virtual void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialParent = transform.parent;
    }

    protected virtual void Update()
    {
        if(autoRespawn && transform.position.y < respawnHeightLitmit)
        {
            Respawn();
        }
    }

    public  virtual void PickUp(Transform slot)
    {
        if (!beHold)
        {
            beHold = true;
            transform.parent = slot;
            transform.localPosition = Vector3.zero + localPositionOffSet;
            transform.localRotation = Quaternion.identity * localRotationOffSet;
            m_rigidBody.isKinematic = true;
            m_collider.isTrigger = true;
            m_interpolation = m_rigidBody.interpolation;
            m_rigidBody.interpolation = RigidbodyInterpolation.None;
            onPicked.Invoke();
        }
    }

    public virtual void Release(Vector3 direction, float force)
    {
        if (beHold)
        {
            transform.parent = null;
            transform.position += direction * releaseOffSet;
            m_collider.isTrigger = m_rigidBody.isKinematic = beHold = false;
            m_rigidBody.interpolation = m_interpolation;
            m_rigidBody.velocity = direction * force;
            onReleased.Invoke();
        }
    }

    public virtual void Respawn()
    {
        m_rigidBody.velocity = Vector3.zero;
        transform.parent = initialParent;
        transform.SetLocalPositionAndRotation(initialPosition, initialRotation);
        m_rigidBody.isKinematic = beHold = m_collider.isTrigger = false;
        onRespawn?.Invoke();
    }

    protected virtual void EvaluateHazardRespawn(Collider other)
    {
        if(autoRespawn && respawnOnHitHazard && other.CompareTag(GameTags.Hazard))
        {
            Respawn();
        }
    }

    protected virtual void OnTriggerEnter(Collider other) => EvaluateHazardRespawn(other);
    protected virtual void OnCollisionEnter(Collision collision) => EvaluateHazardRespawn(collision.collider);

    public void OnEntityContact(EntityBase entity)
    {
        
    }
}
