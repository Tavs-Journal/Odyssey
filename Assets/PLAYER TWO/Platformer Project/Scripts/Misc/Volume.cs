using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
public class Volume : MonoBehaviour
{
    public UnityEvent onEnter;
    public UnityEvent onExit;

    public AudioClip enterclip;
    public AudioClip exitclip;

    protected Collider m_Collider;
    protected AudioSource m_audio;

    protected virtual void Start()
    {
        InitialCollider();
        InitializeAudioSource();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(!m_Collider.bounds.Contains(other.bounds.max) ||
            !m_Collider.bounds.Contains(other.bounds.min))
        {
            m_audio.PlayOneShot(enterclip);
            onEnter?.Invoke();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!m_Collider.bounds.Contains(other.transform.position))
        {
            m_audio.PlayOneShot(exitclip);
            onExit?.Invoke();
        }
    }

    protected virtual void InitialCollider()
    {
        m_Collider = GetComponent<Collider>();
        m_Collider.isTrigger = true;
    }

    protected virtual void InitializeAudioSource()
    {
        if(!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        m_audio.spatialBlend = 0.5f;
    }
}