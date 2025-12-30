using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    protected Game m_game => Game.instance;
    protected GameLoader m_loader => GameLoader.instance;
    public virtual void AddRetires(int amount) => m_game.retries += amount;
    public virtual void Load(string scene) => m_loader.Load(scene);
}
