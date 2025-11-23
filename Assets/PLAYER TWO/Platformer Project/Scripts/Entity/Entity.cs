using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour { }
public abstract class Entity<T> :EntityBase where T :Entity<T>
{
    public EntityStateManager<T> states {  get; private set; }

    public Vector3 velocity { get; set; }

    public Vector3 lateralvelocity
    {
        get { return new Vector3(velocity.x, 0, velocity.z); }
        set { velocity = new Vector3(value.x, velocity.y, value.z);}
    }

    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }

    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    protected virtual void HandleState() => states.Step();

    protected virtual void Awake()
    {
        InitializeStateManager();
    }
    
    protected virtual void Update()
    {
        HandleState();
    }
}
