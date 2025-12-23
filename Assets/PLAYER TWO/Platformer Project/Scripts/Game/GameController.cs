using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    protected Game m_game => Game.instance;
    public virtual void AddRetires(int amount) => m_game.retries += amount;
}
