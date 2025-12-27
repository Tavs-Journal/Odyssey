using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]

public class FallingPlatForm : MonoBehaviour, IEntityContact
{
    protected Collider m_collider;
    protected Vector3 initialPosition;

    [Header("Move Settings")]
    public bool autoReset = true;
    public float fallDelay = 2f;
    public float resetDelay = 5f;
    public float fallGravity = 40f;

    [Header("Shake Settings")]
    public bool shake = true;
    public float speed = 45f;
    public float height = 0.1f;

    protected bool active;
    protected bool falling;
    protected Collider[] m_overlaps = new Collider[20];

    protected virtual void Start()
    {
        tag = GameTags.Platform;
        m_collider = GetComponent<Collider>();
        initialPosition = transform.position;
    }

    protected virtual void Update()
    {
        if (falling)
        {
            transform.position += fallGravity * Vector3.down * Time.deltaTime;
        }
    }

    public void OnEntityContact(EntityBase entity)
    {
        if(entity is Player && entity.IsPointUnderStep(m_collider.bounds.max) && !active)
        {
            active = true;
            StartCoroutine(Routine());
        }
    }

    protected virtual IEnumerator Routine()
    {
        var timer = fallDelay;
        while (timer > 0)
        {
            if(shake && (timer <= fallDelay / 2f))
            {
                var shake = Mathf.Sin(Time.time * speed) * height;
                transform.position = initialPosition + Vector3.up * shake;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        Fall();
        if (autoReset)
        {
            yield return new WaitForSeconds(resetDelay);
        }
        Restart();
    }

    protected virtual void Fall()
    {
        if (!falling)
        {
            falling = true;
            m_collider.isTrigger = true;
        }
    }

    protected virtual void Restart()
    {
        falling = active = false;
        transform.position = initialPosition;
        m_collider.isTrigger = false;
        OffSetPlayer();
    }

    protected virtual void OffSetPlayer()
    {
        var center = m_collider.bounds.center;
        var extents = m_collider.bounds.extents;
        var maxY = m_collider.bounds.max.y;

        var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);
        for(int i = 0; i < overlaps; i++)
        {
            if (!m_overlaps[i].CompareTag(GameTags.Player)) continue;
            var distance = maxY - m_overlaps[i].transform.position.y;
            var height = m_overlaps[i].GetComponent<Player>().height;
            var offset = Vector3.up * (distance + height * 0.5f);

            m_overlaps[i].transform.position += offset;
        }
    }
}
