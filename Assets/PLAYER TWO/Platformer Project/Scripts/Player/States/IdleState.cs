using UnityEngine;

public class IdleState : PlayerState
{
    protected override void OnContact(Player entity, Collider other)
    {
        
    }

    protected override void OnEnter(Player entity)
    {
        
    }

    protected override void OnExit(Player entity)
    {
        
    }

    protected override void OnStep(Player entity)
    {

        Debug.Log("onstep");
    }
}
