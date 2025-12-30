using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class LevelRespawner : Singleton<LevelRespawner>
{
    protected List<PlayerCamera> m_cameras;
    protected Level m_level => Level.instance;
    protected LevelPauser m_pauser => LevelPauser.instance;
    protected Game m_game => Game.instance;
    protected Fader m_fader => Fader.instance;
    protected LevelScore m_score => LevelScore.instance;    

    public float respawnFadeOutDelay = 1f;
    public float respawnFadeInDelay = .5f;
    public float restartFadeOutDelay = .5f;

    public UnityEvent OnRespawn;
    public UnityEvent OnGameOver;

    protected virtual void Start()
    {
        m_cameras = new List<PlayerCamera>(FindObjectsOfType<PlayerCamera>());
        m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
    }

    public virtual void Respawn(bool constumeRetries)
    {
        StopAllCoroutines();
        StartCoroutine(Routine(constumeRetries));
    }

    public virtual void ReStart()
    {
        StopAllCoroutines();
        StartCoroutine(ReStartRoutine());
    }

    protected virtual IEnumerator Routine(bool consumeRetries)
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.input.enabled = false;
        if (consumeRetries && m_game.retries == 0)
        {
            StartCoroutine(GameOverRoutine());
            yield break;
        }
        yield return new WaitForSeconds(respawnFadeOutDelay);
        m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));
    }

    protected virtual IEnumerator RespawnRoutine(bool consumRetries)
    {
        if (consumRetries)
        {
            m_game.retries--;
        }

        m_level.player.Respawn();
        m_score.coins = 0;
        ResetCamera();
        OnRespawn?.Invoke();
        yield return new WaitForSeconds(respawnFadeInDelay);
        m_fader.FadeIn(() =>
        {
            m_pauser.canPause = true;
            m_level.player.input.enabled = true;
        });
    }

    protected virtual IEnumerator ReStartRoutine()
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.input.enabled = false;
        yield return new WaitForSeconds(restartFadeOutDelay);
        GameLoader.instance.ReLoad();
    }

    protected virtual void ResetCamera()
    {
        foreach (PlayerCamera camera in m_cameras)
        {
            camera.Reset();
        }
    }

    protected virtual IEnumerator GameOverRoutine()
    {
        yield return null;
    }
}
