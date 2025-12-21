using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
[RequireComponent(typeof(Collider))]
public class Collectable : MonoBehaviour
{
    [Header("General Settings")]
    public bool collectOnContact = true;
    public int times;
    public float ghostingDuration = 0.5f;
    public GameObject display;
    public AudioClip clip;
    public ParticleSystem particle;

    [Header("Visibility Settings")]
    public bool hidden;
    public float quickShowHeight = 2f;
    public float quickShowDuration = 0.25f;
    public float hideDuration = 0.5f;

    [Header("Life Time")]
    public bool hasLifeTime;
    public float lifeTimeDuration = 5f;

    [Header("Physics Settings")]
    public bool usePhysics;
    public float minForceToStopPhysics = 3f;
    public float collisionRadius = 0.5f;
    public float gravity = 15f;
    public float bounciness = 0.98f;
    public float maxBounceYVelocity = 10f;
    public bool randomizeInitialDiraction = true;
    public Vector3 initialVelocity = new Vector3(0, 12, 0);
    public AudioClip collisionClip;

    [Space(15)]
    public PlayerEvent onCollect;

    protected Collider m_collider;
    protected AudioSource m_Audio;

    protected bool m_vanish;
    protected bool m_ghosting = true;
    protected float m_elapsedLifeTime;
    protected float m_elapsedGhostingTime;
    protected Vector3 m_velocity;

    protected const int k_verticalMinRotation = 0;
    protected const int m_verticalMaxRotation = 30;
    protected const int k_horizontalMinRotation = 0;
    protected const int m_horizontalMaxRotation = 360;

    protected virtual void Awake()
    {
        InitialAudio();
        InitialCollider();
        InitialDisPlay();
        InitialTransform();
        InitialVelocity();
    }

    protected virtual void Update()
    {
        if (!m_vanish)
        {
            HandleGhosting();
            HandleLifeTime();
            if (usePhysics)
            {
                HandleMovement();
                HandleSweep();
            }
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if(collectOnContact && other.CompareTag(GameTags.Player))
        {
            if(other.TryGetComponent<Player>(out var player))
            {
                Collect(player);
            }
        }
    }

    protected virtual void InitialAudio()
    {
        if(!TryGetComponent(out m_Audio))
        {
            m_Audio = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void InitialCollider()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    protected virtual void InitialTransform()
    {
        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }

    protected virtual void InitialDisPlay()
    {
        display.SetActive(!hidden);
    }

    protected virtual void InitialVelocity()
    {
        var direction = initialVelocity.normalized;
        var force = initialVelocity.magnitude;
        if (randomizeInitialDiraction)
        {
            var randomZ = Random.Range(k_verticalMinRotation, m_verticalMaxRotation);
            var randomY = Random.Range(k_horizontalMinRotation, m_horizontalMaxRotation);
            direction = Quaternion.Euler(0, 0, randomZ) * direction; 
            direction = Quaternion.Euler(0, randomY, 0) * direction;
        }
        m_velocity = direction * force;
    }

    protected virtual void HandleGhosting()
    {
        if (m_ghosting)
        {
            m_elapsedGhostingTime += Time.deltaTime;
            if(m_elapsedGhostingTime >= ghostingDuration)
            {
                m_elapsedGhostingTime = 0;
                m_ghosting = false;
            }
        }
    }

    protected virtual void HandleLifeTime()
    {
        if (hasLifeTime)
        {
            m_elapsedLifeTime += Time.deltaTime;
            if (m_elapsedLifeTime >= lifeTimeDuration)
            {
                Vanish();
            }
        }
    }

    protected virtual void HandleMovement()
    {
        m_velocity.y -= gravity * Time.deltaTime;
    }

    protected virtual void HandleSweep()
    {
        var direction = m_velocity.normalized;
        var magnitude = m_velocity.magnitude;
        var distance = magnitude * Time.deltaTime;
        if(Physics.SphereCast(transform.position, collisionRadius, direction, out var hit, 
            distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag(GameTags.Player))
            {
                var bounceDirection = Vector3.Reflect(direction, hit.normal);
                m_velocity = bounceDirection * bounciness * magnitude;
                m_velocity.y = Mathf.Min(m_velocity.y, maxBounceYVelocity);
                m_Audio.Stop();
                m_Audio.PlayOneShot(collisionClip);
            }
            if(m_velocity.y <= minForceToStopPhysics)
                usePhysics = false;
        }
        transform.position += m_velocity * Time.deltaTime;
    }

    public virtual void Collect(Player player)
    {
        if(!m_vanish && !m_ghosting)
        {
            if (!hidden)
            {
                Vanish();
                if(!particle != null)
                {
                    particle.Play();
                }
            }
            else
            {
                StartCoroutine(QuickShowRoutine());
            }
            StartCoroutine(CollectRoutine(player));
        }
    }

    protected virtual IEnumerator QuickShowRoutine()
    {
        var elapsedTime = 0f;
        var initialPosition = transform.position;
        var targetPosition = initialPosition + Vector3.up * quickShowHeight;

        display.SetActive(true);
        m_collider.enabled = false;

        while(elapsedTime < quickShowDuration)
        {
            var t = elapsedTime / quickShowDuration;
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        yield return new WaitForSeconds(hideDuration);
        transform.position = initialPosition;
        Vanish();
    }

    protected virtual IEnumerator CollectRoutine(Player player)
    {
        for(int i = 0; i < times; i++)
        {
            m_Audio.Stop();
            m_Audio.PlayOneShot(clip);
            onCollect.Invoke(player);
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected virtual void Vanish()
    {
        if (!m_vanish)
        {
            m_vanish = true;
            m_elapsedLifeTime = 0;
            display.SetActive(false);
            m_collider.enabled = false;
        }
    }
}