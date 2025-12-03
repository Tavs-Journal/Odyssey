using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EntityStateManager :MonoBehaviour
{
    public EntityStateManagerEvents events;
}
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();

    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    public EntityState<T> current {  get; private set; }

    public EntityState<T> last { get; private set; }

    public int index => m_list.IndexOf(current);
    public int lastIndex => m_list.IndexOf(last);

    public T entity {  get; private set; }

    protected virtual void Start()
    {
        entity = GetComponent<T>();
        InitializeStates();
    }

    protected abstract List<EntityState<T>> GetStateList();
    
    protected virtual void InitializeStates()
    {
        m_list = GetStateList();

        foreach(var state in m_list)
        {
            var type = state.GetType();
           
            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type, state);
            }   
            
            if(m_list.Count > 0)
            {
                current = m_list[0];
            }
        }
    }

    public virtual void Step()
    {
        if(current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }

    public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);
        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }

    public virtual void Change(EntityState<T> to)
    {
        if(to != null && Time.timeScale > 0)
        {
            if(current != null)
            {
                current.Exit(entity);
                events.onExit.Invoke(current.GetType());
                last = current;
            }

            current = to;
            current.Enter(entity);
            events.onEnter.Invoke(current.GetType());
            events.onChange?.Invoke();
        }
    }
}
