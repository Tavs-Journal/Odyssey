using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Vector3 offset;
    public Vector3 initiaPosition;

    public float duration;
    public float resetDuration;

    protected virtual void Start()
    {
        initiaPosition = transform.localPosition;
    }

    public virtual void ApplyOffset()
    {
        StopAllCoroutines();
        StartCoroutine(ApplyOffsetRoutine(initiaPosition, initiaPosition + offset, duration));
    }

    public virtual void ResetOffset()
    {
        StopAllCoroutines();
        StartCoroutine(ApplyOffsetRoutine(initiaPosition + offset, initiaPosition, resetDuration));
    }

    protected virtual IEnumerator ApplyOffsetRoutine(Vector3 from, Vector3 to, float duration)
    {
        var elapsed = 0f;
        while(elapsed <= duration)
        {
            var t = elapsed / duration;
            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = to;
    }
}
