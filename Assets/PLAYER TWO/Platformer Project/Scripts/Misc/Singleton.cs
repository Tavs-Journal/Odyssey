using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T m_instance;

    protected virtual void Awake()
    {
        if(instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public static T instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<T>();
            }
            return m_instance;
        }
    }
}