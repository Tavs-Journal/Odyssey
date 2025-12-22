using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public virtual void AddHealth(Player player) => AddHealth(player, 1);

    public virtual void AddHealth(Player player, int amount) => player.health.Increase(amount);
}
