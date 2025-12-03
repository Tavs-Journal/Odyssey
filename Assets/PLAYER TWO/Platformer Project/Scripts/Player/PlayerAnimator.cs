using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Player))]
public class PlayerAnimator : MonoBehaviour
{
    [System.Serializable]
    public class ForcedTransition
    {
        [Tooltip("玩家状态机中 'fromStateId' 的状态结束时，强制跳转到某个动画")]
        public int fromStateId;

        [Tooltip("目标所在动画的 Animator 层索引。默认0表示 BaseLayer")]
        public int animationLayer;

        [Tooltip("要强制播放的动画名")]
        public string toAnimationState;
    }
    [Header("Setting")]
    public float minLateralAnimationSpeed = 0.5f;
    public List<ForcedTransition> forcedTransitions;

    [Header("Parameters Names")] // Animator 参数的变量名（可在 Inspector 修改）
    public string stateName = "State";                      // 当前状态
    public string lastStateName = "Last State";             // 上一个状态
    public string lateralSpeedName = "Lateral Speed";       // 横向速度
    public string verticalSpeedName = "Vertical Speed";     // 纵向速度
    public string lateralAnimationSpeedName = "Lateral Animation Speed"; // 横向动画播放速度
    public string healthName = "Health";                    // 血量
    public string jumpCounterName = "Jump Counter";         // 跳跃计数
    public string isGroundedName = "Is Grounded";           // 是否落地
    public string isHoldingName = "Is Holding";             // 是否正在抓取物品
    public string onStateChangedName = "On State Changed";  // 状态切换触发器

    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_healthHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_isHoldingHash;
    protected int m_onStateChangedHash;

    protected Player m_player;

    public Animator animator;

    protected Dictionary<int, ForcedTransition> m_forcedTransition;

    protected virtual void Start()
    {
        InitializePlayer();
        InitializeForcedTransition();
        InitializeParametersHash();
        InitializeAnimatorTriggers();
    }

    protected virtual void LateUpdate()
    {
        HandleAnimatorParameters();
    }

    public virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();
        m_player.states.events.onChange.AddListener(HandleForcedTransitions);
    }

    protected virtual void InitializeForcedTransition()
    {
        m_forcedTransition = new Dictionary<int, ForcedTransition>();
        foreach (var transition in forcedTransitions) 
        {
            if (!m_forcedTransition.ContainsKey(transition.fromStateId))
            {
                m_forcedTransition.Add(transition.fromStateId, transition);
            }
        }
    }

    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_isHoldingHash = Animator.StringToHash(isHoldingName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }

    protected virtual void InitializeAnimatorTriggers()
    {
        m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));
    }

    protected virtual void HandleForcedTransitions()
    {
        var lastStateIndex = m_player.states.lastIndex;
        if (m_forcedTransition.ContainsKey(lastStateIndex))
        {
            var layer = m_forcedTransition[lastStateIndex].animationLayer;
            animator.Play(m_forcedTransition[lastStateIndex].toAnimationState, layer);
        }
    }

    protected virtual void HandleAnimatorParameters()
    {
        var lateralSpeed = m_player.lateralvelocity.magnitude;
        var verticalSpeed = m_player.verticalVelocity.y;
        var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);

        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
    }
}
