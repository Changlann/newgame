using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 主动技能基类，所有主动技能都继承自这个类
/// </summary>
public abstract class ActiveSkill : MonoBehaviour
{
    [Header("基础设置")]
    [Tooltip("技能名称")]
    public string skillName = "技能";

    [Tooltip("技能描述")]
    [TextArea(3, 5)]
    public string description = "技能描述";

    [Tooltip("技能图标")]
    public Sprite icon;

    [Tooltip("技能键")]
    public KeyCode skillKey = KeyCode.Q;

    [Header("冷却设置")]
    [Tooltip("基础冷却时间（秒）")]
    public float baseCooldown = 10f;

    [Tooltip("当前冷却时间")]
    public float currentCooldown = 0f;

    [Tooltip("技能等级")]
    public int level = 1;

    [Tooltip("每级减少的冷却时间（秒）")]
    public float cooldownReductionPerLevel = 1f;

    [Header("特效设置")]
    [Tooltip("技能使用时的音效")]
    public AudioClip skillSound;

    [Tooltip("技能使用时的特效预制体")]
    public GameObject skillEffectPrefab;

    [Header("事件")]
    [Tooltip("技能使用时触发的事件")]
    public UnityEvent onSkillActivated;

    [Tooltip("技能冷却完成时触发的事件")]
    public UnityEvent onCooldownComplete;

    // 私有变量
    private AudioSource audioSource;
    private bool isInitialized = false;

    protected virtual void Awake()
    {
        // 获取或添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        isInitialized = true;
    }

    protected virtual void Update()
    {
        // 处理冷却
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                currentCooldown = 0;
                onCooldownComplete?.Invoke();
            }
        }

        // 检测键盘输入
        if (Input.GetKeyDown(skillKey) && currentCooldown <= 0)
        {
            ActivateSkill();
        }
    }

    /// <summary>
    /// 激活技能
    /// </summary>
    public virtual void ActivateSkill()
    {
        if (currentCooldown > 0) return;

        // 检查是否已初始化
        if (!isInitialized)
        {
            Debug.LogWarning($"[{skillName}] 尝试执行未初始化的技能");
            return;
        }

        // 播放技能音效
        if (skillSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(skillSound);
        }

        // 实例化技能特效
        if (skillEffectPrefab != null)
        {
            Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
        }

        // 技能实际逻辑由子类实现
        Execute();

        // 开始冷却
        currentCooldown = GetActualCooldown();

        // 触发事件
        onSkillActivated?.Invoke();
    }

    /// <summary>
    /// 技能实际执行逻辑，由子类实现
    /// </summary>
    protected abstract void Execute();

    /// <summary>
    /// 获取实际冷却时间（考虑等级减免）
    /// </summary>
    public float GetActualCooldown()
    {
        return Mathf.Max(1f, baseCooldown - (level - 1) * cooldownReductionPerLevel);
    }

    /// <summary>
    /// 升级技能
    /// </summary>
    public virtual void LevelUp()
    {
        level++;
        Debug.Log($"技能 {skillName} 升级到 {level} 级");
    }

    /// <summary>
    /// 获取冷却进度（0-1）
    /// </summary>
    public float GetCooldownProgress()
    {
        if (currentCooldown <= 0) return 0;
        return currentCooldown / GetActualCooldown();
    }
}