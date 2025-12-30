using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public class Game : Singleton<Game>
{
    public int initialRetriesSet = 3;

    protected int m_retries;
    protected int m_dataindex;
    protected DateTime m_createdAt;
    protected DateTime m_updatedAt;

    public List<GameLevel> levels;

    public UnityEvent<int> OnReTriesSet;
    public UnityEvent OnSavingRequested;
    public int retries
    {
        get { return m_retries; }
        set
        {
            m_retries = value;
            OnReTriesSet?.Invoke(m_retries);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        retries = initialRetriesSet;
        DontDestroyOnLoad(gameObject);   
    }

    public virtual void LoadState(int index, GameData data)
    {
        Debug.Log($"[Game.LoadState] slot={index} retries={data.retries} levelsLen={data.levels?.Length}");

        m_dataindex = index;
        m_retries = data.retries;
        m_createdAt = DateTime.Parse(data.createdAt);
        m_updatedAt = DateTime.Parse(data.updatedAt);
        for(int i = 0; i < data.levels.Length; i++)
        {
            levels[i].LoadState(data.levels[i]);
        }
    }

    public virtual GameLevel GetCurrentLevel()
    {
        var scene = GameLoader.instance.currentScene;
        return levels.Find((level) => level.scene == scene);
    }

    public virtual int GetCurrentLevelIndex()
    {
        var scene = GameLoader.instance.currentScene;
        return levels.FindIndex((level) => level.scene == scene);
    }

    public virtual void RequestSaving()
    {
        Debug.Log($"[Game.RequestSaving] saving slot={m_dataindex} retries={retries}");
        GameSaver.instance.Save(ToData(), m_dataindex);
        OnSavingRequested?.Invoke();
    }

    public virtual void UnlockNextLevel()
    {
        var index = GetCurrentLevelIndex() + 1;

        if (index >= 0 && index < levels.Count)
        {
            levels[index].locked = false;
        }
    }

    public virtual GameData ToData()
    {
        return new GameData()
        {
            retries = m_retries,
            levels = LevelsData(),
            createdAt = m_createdAt.ToString(),
            updatedAt = DateTime.UtcNow.ToString()
        };
    }
    public virtual LevelData[] LevelsData()
    {
        return levels.Select(level => level.ToData()).ToArray();
    }

    public static void LockCursor(bool value = true)
    {
#if UNITY_STANDALONE || UNITY_WEBGL
        Cursor.visible = !value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif 
    }
}