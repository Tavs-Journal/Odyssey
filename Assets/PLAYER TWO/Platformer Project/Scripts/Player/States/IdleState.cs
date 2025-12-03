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
        player.Gravity();
        var inputDireaction = player.input.GetMovementDirection();

        if(inputDireaction.sqrMagnitude > 0 || player.lateralvelocity.sqrMagnitude > 0) 
        {
            player.states.Change<WalkState>();
        }
    }
}
