using UnityEngine;
using UnityEngine.Events;

public class LevelScore : Singleton<LevelScore>
{
    public UnityEvent<int> OnCoinsSet;
    public UnityEvent<bool[]> OnStarsSet;
    public UnityEvent OnScoreLoaded;

    public float time { get; protected set; }
    public bool stopTime = true;
    public bool[] stars => (bool[])m_stars.Clone();

    protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];
    protected int m_coins;
    public int coins
    {
        get { return m_coins; }
        set
        {
            m_coins = value;
            OnCoinsSet?.Invoke(m_coins);
        }
    }

    protected virtual void Update()
    {
        if (!stopTime)
        {
            time += Time.deltaTime;
        }
    }
}