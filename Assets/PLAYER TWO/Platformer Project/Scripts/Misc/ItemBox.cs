using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(BoxCollider))]
public class ItemBox : MonoBehaviour, IEntityContact
{
    public Collectable[] collectables;

    public MeshRenderer itemBoxRender;

    public Material emptyItemBoxMaterial;

    public UnityEvent OnCollect;

    public UnityEvent OnDisable;

    protected int m_index;

    protected bool m_enabled = true;

    protected Vector3 m_initialScale;

    protected BoxCollider m_collider;

    public void OnEntityContact(EntityBase entity)
    {
        if(entity is Player player)
        {
            if(entity.velocity.y > 0 && entity.position.y < m_collider.bounds.min.y)
            {
                Collect(player);
            }
        }
    }
    public virtual void Start()
    {
        m_collider = GetComponent<BoxCollider>();
        m_initialScale = transform.localScale;
        InitializeCollectables();
    }

    protected virtual void InitializeCollectables()
    {
        foreach(var collectable in collectables)
        {
            if (!collectable.hidden)
            {
                collectable.gameObject.SetActive(false);
            }
            else
            {
                collectable.collectOnContact = false;
            }
        }
    }

    public virtual void Collect(Player player)
    {
        if (m_enabled)
        {
            if(m_index < collectables.Length)
            {
                if (collectables[m_index].hidden)
                {
                    collectables[m_index].Collect(player);
                }
                else
                {
                    collectables[m_index].gameObject.SetActive(true);
                }
                m_index = Mathf.Clamp(m_index + 1, 0, collectables.Length);
                OnCollect?.Invoke();
            }
            if(m_index == collectables.Length)
            {
                Disable();
            }
        }
    }

    public virtual void Disable()
    {
        if (m_enabled)
        {
            m_enabled = false;
            itemBoxRender.sharedMaterial = emptyItemBoxMaterial;
            OnDisable?.Invoke();
        }
    }
}