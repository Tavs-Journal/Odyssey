using UnityEngine;
[RequireComponent (typeof(Player))]
public class PlayerLevelPause : MonoBehaviour
{
    protected Player player;
    protected LevelPauser m_pauser;

    protected virtual void Start()
    {
        player = GetComponent<Player>();
        m_pauser = LevelPauser.instance;
    }

    protected virtual void Update()
    {
        if (player.input.GetPauseDown())
        {
            var value = m_pauser.paused;
            m_pauser.Pause(!value);
        }
    }
}
