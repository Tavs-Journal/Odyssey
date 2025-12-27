using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider), typeof(AudioSource))]
public class Portal : MonoBehaviour
{
    public Portal exit;
    public bool useFlash;
    public float exitOffSet = 1f;
    public AudioClip clip;

    protected Collider m_collider;
    protected AudioSource m_source;
    protected PlayerCamera m_camera;
    
    protected Vector3 position => transform.position;
    protected Vector3 forward => transform.forward;

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_source = GetComponent<AudioSource>();
        m_camera = FindObjectOfType<PlayerCamera>();
        m_collider.isTrigger = true;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(exit && other.TryGetComponent(out Player player))
        {
            var offset = player.unsizedPosition.y - transform.position.y;
            player.transform.position = exit.transform.position + Vector3.up * offset;
            player.FaceDirection(exit.forward);
            m_camera.Reset();
            var inputDirection = player.input.GetMovementCameraDirection();
            if(Vector3.Dot(inputDirection, exit.forward) < 0)
            {
                player.FaceDirection(-exit.forward);
            }
            player.transform.position += player.transform.forward * exitOffSet;
            player.lateralvelocity = player.transform.forward * player.lateralvelocity.magnitude;
            if(useFlash)
            {
                Flash.instance?.Trigger();
            }
            m_source.PlayOneShot(clip);
        }
    }
}
