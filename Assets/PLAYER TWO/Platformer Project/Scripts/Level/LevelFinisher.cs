using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelFinisher : Singleton<LevelFinisher>
{
    public UnityEvent OnFinish;
    public UnityEvent OnExit;

    public bool unlockNextLevel;
    public string nextScene;
    public string exitScene;
    public float loadingDelay = 1f;

    protected Game m_game => Game.instance;
    protected Level m_level => Level.instance;
    protected LevelScore m_score => LevelScore.instance;
    protected LevelPauser m_pauser => LevelPauser.instance;
    protected GameLoader m_loader => GameLoader.instance;
    protected Fader m_fader => Fader.instance;

    public virtual void Exit()
    {
        StopAllCoroutines();
        StartCoroutine(ExitRoutine());
    }

    public virtual void Finish()
    {
        StopAllCoroutines();
        StartCoroutine(FinishRoutine());
    }

    protected virtual IEnumerator ExitRoutine()
    {
        Debug.Log("[LevelFinisher] Exit called");
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.input.enabled = false;
        yield return new WaitForSeconds(loadingDelay);
        Game.LockCursor(false);
        m_loader.Load(exitScene);
        OnExit?.Invoke();
    }

    protected virtual IEnumerator FinishRoutine()
    {
        Debug.Log("[LevelFinisher] Finish called");
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.input.enabled = false;

        yield return new WaitForSeconds(loadingDelay);
        if (unlockNextLevel)
        {
            m_game.UnlockNextLevel();
        }
        Game.LockCursor(false);
        m_score.Consolidate();
        m_loader.Load(exitScene);
        OnFinish?.Invoke();
    }
}
