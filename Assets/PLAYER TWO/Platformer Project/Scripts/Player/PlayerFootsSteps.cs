using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(Player))]
public class PlayerFootsSteps : MonoBehaviour
{
    [System.Serializable]
    public class Surface
    {
        public string tag;
        public AudioClip[] footsteps;
        public AudioClip[] landings;
    }

    public Surface[] surfaces;

    public AudioClip[] defaultFootSteps;
    public AudioClip[] defaultLandings;

    protected Player player;

    protected AudioSource m_audio;

    protected Vector3 m_lastLateralPosition;

    public float stepOffset = 1.25f;
    public float footsStepVolume = 1.25f;

    protected Dictionary<string, AudioClip[]> m_footsteps = new Dictionary<string, AudioClip[]>();
    protected Dictionary<string, AudioClip[]> m_landings = new Dictionary<string, AudioClip[]>();

    protected virtual void Start()
    {
        player = GetComponent<Player>();
        player.entityEvents.OnGroundEnter.AddListener(Landing);
        if(!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
        foreach(var surface in surfaces)
        {
            m_footsteps.Add(surface.tag, surface.footsteps);
            m_landings.Add(surface.tag, surface.landings);
        }
    }

    protected virtual void Update()
    {
        if(player.isGrounded && player.states.IsCurrentOfType(typeof(WalkState)))
        {
            var position = transform.position;
            var lateralPosition = new Vector3(position.x, 0, position.z);
            var distance = (m_lastLateralPosition - lateralPosition).magnitude;
            if(stepOffset <= distance)
            {
                if (m_footsteps.ContainsKey(player.groundHit.collider.tag))
                {
                    PlayRandomClip(m_footsteps[player.groundHit.collider.tag]);
                }
                else
                {
                    PlayRandomClip(defaultFootSteps);
                }
                m_lastLateralPosition = lateralPosition;
            }
        }
    }

    protected virtual void Landing()
    {
        if (!player.onWater)
        {
            if (m_landings.ContainsKey(player.groundHit.collider.tag))
            {
                PlayRandomClip(m_landings[player.groundHit.collider.tag]);
            }
            else
            {
                PlayRandomClip(defaultLandings);
            }
        }
    }

    protected virtual void PlayRandomClip(AudioClip[] clips)
    {
        if(clips.Length > 0)
        {
            var index = Random.Range(0, clips.Length);
            m_audio.PlayOneShot(clips[index], footsStepVolume);
        }
    }
}
