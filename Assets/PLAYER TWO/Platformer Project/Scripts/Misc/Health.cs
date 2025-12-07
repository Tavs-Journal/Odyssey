using UnityEngine;
using UnityEngine.Events;
public class Health : MonoBehaviour
{
    public int intail = 3;

    public int max = 3;

    public float coolDown = 1f;

    public UnityEvent onChange;

    public UnityEvent onDamage;

    public int m_currentHealth;

    protected float m_lastDamageTime;

    public int current
    {
        get { return m_currentHealth; }
        protected set
        {
            var last = m_currentHealth;
            if(last != value)
            {
                m_currentHealth = Mathf.Clamp(value, 0, max);
                onChange?.Invoke();
            }
        }
    }

    public virtual bool IsEmpty => m_currentHealth == 0;

    public virtual bool recovering => Time.time < m_lastDamageTime + coolDown;

    public virtual void Set(int amount) => current = amount;

    public virtual void Increase(int amout) => current += amout;

    public virtual void Damage(int amount)
    {
        if (!recovering)
        {
            current -= Mathf.Abs(amount);
            m_lastDamageTime = Time.time;
            onDamage?.Invoke();
        }
    }

    public virtual void Reset() => current = intail;

    protected virtual void Awake() => current = intail;
}