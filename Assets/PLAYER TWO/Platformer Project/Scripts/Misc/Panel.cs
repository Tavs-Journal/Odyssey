using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class Panel : MonoBehaviour, IEntityContact
{
    protected Collider m_collider;
    protected AudioSource m_audio;

    public bool activated;
    public bool autoToggle;
    public bool requirePlayer;
    public bool requirePlayerStomp;

    public AudioClip activateClip;
    public AudioClip deactivateClip;

    public UnityEvent OnActivate;
    public UnityEvent OnDeactivate;

    protected Collider m_entityActivator;
    protected Collider m_otherAcivator;

    protected virtual void Start()
    {
        gameObject.tag = GameTags.Panel;
        m_audio = GetComponent<AudioSource>();
        m_collider = GetComponent<Collider>();
    }

    protected virtual void Update()
    {
        if(m_entityActivator || m_otherAcivator)
        {
            var center = m_collider.bounds.center;
            var contactOffset = Physics.defaultContactOffset + 0.1f;
            var size = m_collider.bounds.size + Vector3.up * contactOffset;
            var bounds = new Bounds(center, size);

            var intersectsEntity = m_entityActivator && bounds.Intersects(m_entityActivator.bounds);
            var intersectsOther = m_otherAcivator && bounds.Intersects(m_otherAcivator.bounds);
            if(intersectsEntity || intersectsOther)
            {
                Activate();
            }
            else
            {
                m_entityActivator = intersectsEntity ? m_entityActivator : null;
                m_otherAcivator = intersectsOther ? m_otherAcivator : null;
                if (autoToggle)
                {
                    Deactivate();
                }
            }
        }
    }

    public void OnEntityContact(EntityBase entity)
    {
        if (entity.velocity.y <= 0 && entity.IsPointUnderStep(m_collider.bounds.max))
        {
            if((!requirePlayer || entity is Player) && 
               (!requirePlayerStomp || (entity as Player).states.IsCurrentOfType(typeof(StompPlayerState))))
            {
                m_entityActivator = entity.controller;
            }
        }
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        if(!(requirePlayer || requirePlayerStomp) && !collision.collider.CompareTag(GameTags.Player))
        {
            m_otherAcivator = collision.collider;
        } 
    }

    protected virtual void Activate()
    {
        if (!activated)
        {
            if (activateClip)
            {
                m_audio.PlayOneShot(activateClip);
            }
            activated = true;
            OnActivate?.Invoke();
        }
    }

    protected virtual void Deactivate()
    {
        if (activated)
        {
            if (deactivateClip)
            {
                m_audio?.PlayOneShot(deactivateClip);
            }
            activated = false;
            OnDeactivate?.Invoke();
        }
    }
}
