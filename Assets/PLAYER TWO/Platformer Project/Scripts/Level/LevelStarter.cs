using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelStarter : Singleton<LevelStarter>
{
    public UnityEvent OnStart;

    public float enablePlayerDelay = 1f;

    protected LevelPauser m_pauser => LevelPauser.instance;

    protected virtual void Start()
    {
        StartCoroutine(Routine());
    }

    protected virtual IEnumerator Routine()
    {
        Game.LockCursor();
        yield return new WaitForSeconds(enablePlayerDelay);
        m_pauser.canPause = true;
        OnStart?.Invoke();
    }


}
