using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelStarter : Singleton<LevelStarter>
{
    public UnityEvent OnStart;

    public float enablePlayerDelay = 1f;

    protected LevelPauser m_pauser => LevelPauser.instance;
    protected Level m_level => Level.instance;
    protected LevelScore m_score => LevelScore.instance;


    protected virtual void Start()
    {
        StartCoroutine(Routine());
    }

    protected virtual IEnumerator Routine()
    {
        Game.LockCursor();
        m_level.player.controller.enabled = false;
        m_level.player.input.enabled = false;
        yield return new WaitForSeconds(enablePlayerDelay);
        m_score.stopTime = false;
        m_level.player.controller.enabled = true;
        m_level.player.input.enabled = true;
        m_pauser.canPause = true;
        OnStart?.Invoke();
    }
}
