using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent (typeof(Collider))]
public class Breakable : MonoBehaviour 
{
    public GameObject display;

    public AudioClip clip;

    public UnityEvent OnBreak;

    protected Collider m_collider;

    protected AudioSource m_audio;

    protected Rigidbody m_rb;

    public bool broken { get; protected set; }

    protected virtual void Start()
    {
        m_audio = GetComponent<AudioSource>();
        m_collider = GetComponent<Collider>();
        TryGetComponent(out m_rb);
    }

    public virtual void Break()
    {
        if (!broken)
        {
            if (m_rb)
            {
                m_rb.isKinematic = true;
            }
            broken = true;
            display.SetActive (false);
            m_collider.enabled = false;
            m_audio.PlayOneShot(clip);
            OnBreak?.Invoke();
        }
    }
}