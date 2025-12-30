using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    public int retries
    {
        get { return retries; }
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
        m_dataindex = index;
        m_retries = data.retries;
        m_createdAt = DateTime.Parse(data.createdAt);
        m_updatedAt = DateTime.Parse(data.updatedAt);
        for(int i = 0; i < data.levels.Length; i++)
        {
            levels[i].LoadState(data.levels[i]);
        }
    }

    public static void LockCursor(bool value = true)
    {
#if UNITY_STANDALONE || UNITY_WEBGL
        Cursor.visible = !value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif 
    }
}