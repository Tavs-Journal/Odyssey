using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class Toggle : MonoBehaviour
{
    public float delay;
    public bool state = true;
    public Toggle[] multiTigger;

    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    public virtual void Set(bool value)
    {
        StopAllCoroutines();
        StartCoroutine(SetRoutine(value));
    }

    protected virtual IEnumerator SetRoutine(bool value)
    {
        yield return new WaitForSeconds(delay);
        if (value)
        {
            if (!state)
            {
                state = true;
                foreach(var toggle in multiTigger)
                {
                    toggle.Set(true);
                }
                onActivate?.Invoke();
            }
        }
        else if (state)
        {
            state = false;
            foreach(var toggle in multiTigger)
            {
                toggle.Set(false);
            }
            onDeactivate?.Invoke();
        }
    }
}
