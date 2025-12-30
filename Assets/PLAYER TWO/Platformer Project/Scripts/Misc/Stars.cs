using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stars : Collectable
{
    public int index;
    protected LevelScore m_score => LevelScore.instance;

    protected override void Awake()
    {
        base.Awake();
        m_score.OnScoreLoaded.AddListener(() =>
        {
            if (m_score.stars[index])
            {
                Disable();
            }
        });
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    public override void Collect(Player player)
    {
        m_score.CollectStar(index);
        base.Collect(player);
    }
}
