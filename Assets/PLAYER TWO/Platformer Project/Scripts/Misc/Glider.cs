using System.Collections;
using UnityEngine;
public class Glide : MonoBehaviour
{
    public Player player;
    protected AudioSource m_audio;

    public TrailRenderer[] trails;
    public float scaleDuration = 0.7f;


    [Header("Audio Settings")]
    public AudioClip openAudio;
    public AudioClip closeAudio;

    protected virtual void Start()
    {
        InitializePlayer();
        InitializeAudioSorce();
        InitializeCallBack();
        InitializeGlider();
    }

    protected virtual void InitializePlayer()
    {
        if(!player)
            player = GetComponentInParent<Player>();
    }

    protected virtual void InitializeAudioSorce()
    {
        if(!TryGetComponent(out m_audio))
            m_audio = gameObject.AddComponent<AudioSource>();
    }

    protected virtual void InitializeCallBack()
    {
        player.playerEvents.OnGlidingStart.AddListener(ShowGlider);
        player.playerEvents.OnGlidingStop.AddListener(HideGlider);
    }   

    protected virtual void InitializeGlider()
    {
        SetTrailsEmitting(false);   
        transform.localScale = Vector3.zero;
    }

    protected virtual void ShowGlider()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleGliderRoutine(Vector3.zero, Vector3.one));
        SetTrailsEmitting(true);
        m_audio.PlayOneShot(openAudio);
    }

    protected virtual void HideGlider()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleGliderRoutine(Vector3.one, Vector3.zero));
        SetTrailsEmitting(false);
        m_audio.PlayOneShot(closeAudio);
    }

    protected virtual void SetTrailsEmitting(bool value)
    {
        if(trails == null) return;
        foreach(var trail in trails)
        {
            trail.emitting = value;
        }
    }
    protected IEnumerator ScaleGliderRoutine(Vector3 from, Vector3 to)
    {
        var time = 0f;
        transform.localScale = from;
        while(time < scaleDuration)
        {
            var scale = Vector3.Lerp(from, to, time / scaleDuration);
            transform.localScale = scale;
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = to;
    }
}