using UnityEngine;
public class LevelController : MonoBehaviour 
{
    protected LevelPauser m_pauser => LevelPauser.instance;
    protected LevelRespawner m_respawner => LevelRespawner.instance;
    protected LevelScore m_score => LevelScore.instance;
    protected LevelFinisher m_finisher => LevelFinisher.instance;
    public virtual void AddCoins(int amount)
    {
        m_score.coins += amount;
    }

    public virtual void Pause(bool value) => m_pauser.Pause(value);

    public virtual void ResPawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);

    public virtual void ReStart() => m_respawner.ReStart();

    public virtual void Exit() => m_finisher.Exit();

    public virtual void Finish() => m_finisher.Finish();
}