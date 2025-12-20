using UnityEngine;
public class PlayerAudio : MonoBehaviour
{
    [Header("Voices")] // 玩家语音类音效
    public AudioClip[] jump;       // 跳跃音效（多个，可随机播放）
    public AudioClip[] hurt;       // 受伤音效
    public AudioClip[] attack;     // 攻击音效
    public AudioClip[] lift;       // 举起物品、攀爬音效
    public AudioClip[] maneuver;   // 高级动作（比如后空翻）的音效

    [Header("Effects")] // 玩家动作相关的效果音
    public AudioClip spin;         // 旋转攻击音效
    public AudioClip pickUp;       // 捡起物品音效
    public AudioClip drop;         // 丢下物品音效
    public AudioClip airDive;      // 空中俯冲音效
    public AudioClip stompSpin;    // 踩踏开始旋转音效
    public AudioClip stompLanding; // 踩踏落地音效
    public AudioClip ledgeGrabbing;// 抓住墙边音效
    public AudioClip dash;         // 冲刺音效
    public AudioClip startRailGrind;// 开始滑轨音效
    public AudioClip railGrind;    // 滑轨过程音效（循环播放）

    [Header("Other Sources")]
    public AudioSource grindAudio; // 独立的音源，用于播放滑轨音效

    protected Player player;
    protected AudioSource m_audio;

    protected virtual void Start()
    {
        InitialPlayer();
        InitialAudio();
        InitialCallBacks();
    }

    protected virtual void InitialPlayer() => player = GetComponent<Player>();

    protected virtual void InitialAudio()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void InitialCallBacks()
    {
        player.playerEvents.OnJump.AddListener(() => PlayRandom(jump));

        // 受伤时播放随机受伤音效
        player.playerEvents.OnHurt.AddListener(() => PlayRandom(hurt));

        // 丢东西时播放 drop 音效（不会打断其它音效）
        player.playerEvents.OnThrow.AddListener(() => Play(drop, false));

        // 踩踏开始时播放旋转音效
        player.playerEvents.OnStompStarted.AddListener(() => Play(stompSpin, false));

        // 踩踏落地时播放落地音效
        player.playerEvents.OnStompLanding.AddListener(() => Play(stompLanding));

        // 抓住边缘时播放 ledgeGrabbing 音效（不打断其它）
        player.playerEvents.OnLedgeGrabbed.AddListener(() => Play(ledgeGrabbing, false));

        // 爬上边缘时播放 lift 音效（随机挑选）
        player.playerEvents.OnLedgeClimbing.AddListener(() => PlayRandom(lift));

        // 后空翻等特殊动作时播放 maneuver 音效
        player.playerEvents.OnBackflip.AddListener(() => PlayRandom(maneuver));

        // 冲刺开始时播放 dash 音效
        player.playerEvents.OnDashStarted.AddListener(() => Play(dash));

        // 离开滑轨时，停止 grindAudio
        player.entityEvents.OnRailsExit.AddListener(() => grindAudio?.Stop());

        // 拾取物品时：先播放 lift 音效，再叠加 pickUp 音效
        player.playerEvents.OnPickUp.AddListener(() =>
        {
            PlayRandom(lift);
            m_audio.PlayOneShot(pickUp);
        });

        // 旋转攻击时：播放 attack 音效，并叠加 spin 音效
        player.playerEvents.OnSpin.AddListener(() =>
        {
            PlayRandom(attack);
            m_audio.PlayOneShot(spin);
        });

        // 空中俯冲时：播放 attack 音效，并叠加 airDive 音效
        player.playerEvents.OnAirDive.AddListener(() =>
        {
            PlayRandom(attack);
            m_audio.PlayOneShot(airDive);
        });

        // 进入滑轨时：播放开始滑轨音效，并让 grindAudio 播放循环音效
        player.entityEvents.OnRailsEnter.AddListener(() =>
        {
            Play(startRailGrind, false);
            grindAudio?.Play();
        });

        // 游戏暂停时：暂停玩家音频和滑轨音频
        LevelPauser.instance?.OnPause.AddListener(() =>
        {
            m_audio.Pause();
            grindAudio.Pause();
        });

        // 游戏恢复时：继续播放音效
        LevelPauser.instance?.OnUnPause.AddListener(() =>
        {
            m_audio.UnPause();
            grindAudio.UnPause();
        });
    }

    protected virtual void PlayRandom(AudioClip[] clips)
    {
        if(clips != null && clips.Length > 0)
        {
            var index = Random.Range(0, clips.Length);
            if(clips[index] != null) 
                Play(clips[index]);
        }
    }

    protected virtual void Play(AudioClip audio, bool stopPrevious = true)
    {
        if (audio == null) return;
        if(stopPrevious) m_audio.Stop();
        m_audio.PlayOneShot(audio);
    }
}