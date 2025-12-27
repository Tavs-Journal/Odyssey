using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]

public class GravtyField : MonoBehaviour
{
    public float force = 75f;
    protected Collider m_collider;
    protected virtual void OnTriggerStay(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            if(player.isGrounded)
            {
                player.verticalVelocity = Vector3.zero;
            }
            else
            {
                player.velocity += transform.up * force * Time.deltaTime;
            }
        }
    }
    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }
}
