using UnityEngine;
using UnityEngine.Events;
public class Game : Singleton<Game>
{
    public int m_retries;

    public UnityEvent OnReTriesSet;
    public int retries
    {
        get { return retries; }
        set
        {
            m_retries = value;
            OnReTriesSet?.Invoke();
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