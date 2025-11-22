using UnityEngine;

public class IdleState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {
        
    }

    protected override void OnEnter(Player player)
    {
        
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        var inputDireaction = player.input.GetMovementDirection();
        Debug.Log(inputDireaction);
    }
}
