using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour
{
    public AudioClip clip;
    public UnityEvent OnActivate;

    public Transform respawn;

    protected Collider m_collider;
    protected AudioSource m_audio;
    public bool activated { get; protected set; }

    protected virtual void Active(Player player)
    {
        if (!activated)
        {
            activated = true;
            m_audio.PlayOneShot(clip);
            player.SetRespawn(respawn.position, respawn.rotation);
            OnActivate?.Invoke();
        }
    }

    protected virtual void Awake()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;  
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(!activated && other.TryGetComponent(out Player player))
        {
            Active(player);
        }
    }
}
