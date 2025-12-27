using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{
    [Header("WayPoint Settings")]
    public WayPointMode mode;
    public float waitTime;
    public List<Transform> wayPoints;

    protected Transform currentPoint;

    protected bool m_pong;
    protected bool m_changing;
    protected int index => wayPoints.IndexOf(current);

    public Transform current
    {
        get
        {
            if (!currentPoint)
            {
                currentPoint = wayPoints[0];
            }
            return currentPoint;
        }
        protected set { currentPoint = value; }
    }
    
    public virtual void Next()
    {
        if (m_changing) return;
        if (mode == WayPointMode.PingPong)
        {
            if (!m_pong)
            {
                m_pong = (index + 1 == wayPoints.Count);
            }
            else
            {
                m_pong = (index - 1 >= 0);
            }
            var next = !m_pong ? index + 1 : index - 1;
            StartCoroutine(Change(next));
        }
        else if (mode == WayPointMode.Loop)
        {
            if(index + 1 < wayPoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
            else
            {
                StartCoroutine(Change(0));
            }
        }
        else if(mode == WayPointMode.Once)
        {
            if(index + 1 < wayPoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
        }
    }

    protected virtual IEnumerator Change(int to)
    {
        m_changing = true;
        yield return new WaitForSeconds(waitTime);
        current = wayPoints[to];
        m_changing = false;
    }
}
