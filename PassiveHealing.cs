using UnityEngine;

[RequireComponent(typeof(Health))]
public class PassiveHealing : MonoBehaviour
{
    [Header("触发条件")]
    [Tooltip("生命值低于最大值的百分比时触发")]
    [Range(0.1f, 0.5f)]
    public float healthThreshold = 0.3f; // 默认30%

    [Header("治疗效果")]
    [Tooltip("治疗光环预制体")]
    public GameObject healingCirclePrefab;
    [Tooltip("治疗效果持续时间")]
    public float healingDuration = 10f;
    [Tooltip("冷却时间（秒）")]
    public float cooldown = 30f;
    [Tooltip("每次治疗增加的最大生命值百分比")]
    [Range(0.01f, 0.2f)]
    public float maxHealthIncreasePercent = 0.1f; // 10%

    // 私有变量
    private Health health;
    private float cooldownTimer = 0f;
    private bool healingActive = false;
    private GameObject currentHealingCircle;

    void Start()
    {
        health = GetComponent<Health>();

        // 如果没有预先指定治疗光环预制体，尝试加载
        if (healingCirclePrefab == null)
        {
            healingCirclePrefab = Resources.Load<GameObject>("HealingCircle");
            if (healingCirclePrefab == null)
            {
                Debug.LogWarning("[PassiveHealing] 找不到HealingCircle预制体！请确保已创建或在Inspector中指定。");
            }
        }
    }

    void Update()
    {
        // 技能未解锁时不执行
        if (PassiveSkillManager.Instance == null || !PassiveSkillManager.Instance.IsSkillAUnlocked())
        {
            return;
        }

        // 冷却中
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // 检查健康状态，当生命值低于阈值并且冷却完成时触发治疗
        if (!healingActive && health != null && health.currentHealth < health.maxHealth * healthThreshold)
        {
            ActivateHealing();
        }
    }

    void ActivateHealing()
    {
        healingActive = true;
        cooldownTimer = cooldown;

        // 生成治疗光环
        if (healingCirclePrefab != null)
        {
            currentHealingCircle = Instantiate(healingCirclePrefab, transform.position, Quaternion.identity);

            // 配置治疗光环
            HealingCircle healingCircle = currentHealingCircle.GetComponent<HealingCircle>();
            if (healingCircle != null)
            {
                healingCircle.target = transform;
                healingCircle.duration = healingDuration;

                // 增加最大生命值
                IncreaseMaxHealth();
            }
            else
            {
                Debug.LogError("[PassiveHealing] 生成的治疗光环没有HealingCircle组件！");
            }

            // 恢复计时
            Invoke("EndHealing", healingDuration);
        }
    }

    void EndHealing()
    {
        healingActive = false;

        // 销毁光环（如果它还存在）
        if (currentHealingCircle != null)
        {
            Destroy(currentHealingCircle);
        }
    }

    void IncreaseMaxHealth()
    {
        if (health != null)
        {
            // 计算增加量
            float increase = health.maxHealth * maxHealthIncreasePercent;

            // 增加最大生命值
            health.maxHealth += increase;

            // 同时恢复等量生命值
            health.Heal(increase);

            Debug.Log($"[PassiveHealing] {gameObject.name} 最大生命值增加了 {increase:F1} 点，现在为 {health.maxHealth:F1}");
        }
    }

    void OnDestroy()
    {
        // 确保在销毁时也销毁治疗光环
        if (currentHealingCircle != null)
        {
            Destroy(currentHealingCircle);
        }
    }
}