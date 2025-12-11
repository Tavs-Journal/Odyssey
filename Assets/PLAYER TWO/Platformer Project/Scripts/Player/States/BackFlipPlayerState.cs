using UnityEngine;
public class BackFlipPlayerState : PlayerState
{
    protected override void OnContact(Player player, Collider other)
    {

    }

    protected override void OnEnter(Player player)
    {
        player.SetJumps(1);
        player.playerEvents.OnJump?.Invoke();
        if (player.stats.current.canBackflip)
        {
            player.input.LockMovementDirection();
        }
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        player.Gravity(player.stats.current.backflipGravity);

        player.BackFlipAccelerate();

        if (player.isGrounded)
        {
            player.lateralvelocity = Vector3.zero;
            player.states.Change<IdleState>();
        }
        else if(player.verticalVelocity.y < 0)
        {
            player.StomAttack();
            player.SpinAttack();
            player.AirDive();
        }
        //这里是我自己想改动的代码。
        //原先这里没有速度大于零时候的判断
        //而我希望加上它使其在空中随时可以下落攻击
        //else
        //{
        //    player.StomAttack();
        //    if(player.verticalVelocity.y < 0)
        //    {
                
        //    }
        //}
    }
}