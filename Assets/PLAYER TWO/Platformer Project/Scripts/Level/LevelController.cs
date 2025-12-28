using UnityEngine;
public class LevelController : MonoBehaviour 
{
    protected LevelPauser m_pauser => LevelPauser.instance;
    protected LevelRespawner m_respawner => LevelRespawner.instance;
    public virtual void AddCoins(int amount)
    {

    }

    public virtual void Pause(bool value) => m_pauser.Pause(value);

    public virtual void ResPawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);
}