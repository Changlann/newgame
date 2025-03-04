using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("生命值设置")]
    [Tooltip("最大生命值")]
    public float maxHealth = 100f;
    [Tooltip("当前生命值")]
    public float currentHealth;

    [Header("无敌时间设置")]
    [Tooltip("无敌时间结束时间点")]
    private float invincibleUntil = 0f;

    [Header("颜色设置")]
    [Tooltip("满血时的颜色")]
    public Color fullHealthColor = Color.white;   // 满血颜色
    [Tooltip("无血时的颜色")]
    public Color noHealthColor = Color.red;       // 低血颜色
    [Tooltip("是否将颜色应用到子物体")]
    public bool applyColorToChildren = true;      // 是否应用颜色到子物体
    [Tooltip("受伤颜色的混合强度")]
    [Range(0f, 1f)]
    public float colorIntensity = 0.5f;          // 受伤颜色的混合强度

    [Header("事件")]
    [Tooltip("死亡时触发的事件")]
    public UnityEvent onDeath;
    [Tooltip("受伤时触发的事件")]
    public UnityEvent<float> onDamaged;
    [Tooltip("治疗时触发的事件")]
    public UnityEvent<float> onHealed;

    private MaterialPropertyBlock propBlock;
    private MeshRenderer[] allRenderers;

    void Start()
    {
        currentHealth = maxHealth;
        // Debug.Log($"[Health] {gameObject.name} 初始化 - 最大生命值: {maxHealth}, 当前生命值: {currentHealth}");

        // 获取所有相关的渲染器
        if (applyColorToChildren)
        {
            allRenderers = GetComponentsInChildren<MeshRenderer>();
        }
        else
        {
            MeshRenderer ownRenderer = GetComponent<MeshRenderer>();
            if (ownRenderer != null)
            {
                allRenderers = new MeshRenderer[] { ownRenderer };
            }
        }

        if (allRenderers == null || allRenderers.Length == 0)
        {
            // Debug.LogWarning($"[Health] {gameObject.name} 没有找到任何 MeshRenderer 组件!");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        // 延迟一帧更新颜色，确保材质已经加载
        Invoke("UpdateColor", 0.1f);
    }

    /// <summary>
    /// 设置无敌状态
    /// </summary>
    /// <param name="duration">无敌持续时间(秒)</param>
    public void SetInvincible(float duration)
    {
        invincibleUntil = Time.time + duration;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="showNumber">是否显示伤害数字</param>
    public void TakeDamage(float damage, bool showNumber = false)
    {
        if (Time.time < invincibleUntil)
        {
            // Debug.Log($"[Health] {gameObject.name} 处于无敌状态，免疫伤害!");
            return;
        }

        // Debug.Log($"[Health] {gameObject.name} 受到 {damage} 点伤害. 之前血量: {currentHealth}");

        currentHealth -= damage;
        onDamaged?.Invoke(damage);

        // 只在 showNumber 为 true 时显示伤害数字
        if (showNumber && CompareTag("Enemy"))  // 确保只有敌人才显示伤害数字
        {
            DamageNumberManager.Instance?.ShowDamageNumber((int)damage, transform.position + Vector3.up);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Debug.Log($"[Health] {gameObject.name} 血量降至0，即将死亡");
            Die();
        }
        else
        {
            // Debug.Log($"[Health] {gameObject.name} 受伤后剩余血量: {currentHealth}");
            UpdateColor();
        }
    }

    /// <summary>
    /// 更新颜色以反映当前生命值
    /// </summary>
    void UpdateColor()
    {
        if (allRenderers == null || allRenderers.Length == 0) return;

        float healthPercent = currentHealth / maxHealth;
        Color healthColor = Color.Lerp(noHealthColor, fullHealthColor, healthPercent);

        // 遍历所有渲染器
        foreach (MeshRenderer renderer in allRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // 获取当前的材质颜色
                Color currentColor = renderer.material.color;

                // 将当前材质颜色与血量颜色进行混合
                Color finalColor = Color.Lerp(currentColor, healthColor, colorIntensity);

                // 应用新颜色
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", finalColor);
                renderer.SetPropertyBlock(propBlock);
            }
        }

        // Debug.Log($"[Health] {gameObject.name} 更新颜色 - 血量百分比: {healthPercent:P0}");
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    /// <param name="amount">治疗量</param>
    public void Heal(float amount)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        float actualHealAmount = currentHealth - oldHealth;

        if (actualHealAmount > 0)
        {
            // Debug.Log($"[Health] {gameObject.name} 恢复 {actualHealAmount} 点生命值. 当前血量: {currentHealth}");
            onHealed?.Invoke(actualHealAmount);
            UpdateColor();
        }
    }

    /// <summary>
    /// 处理死亡逻辑
    /// </summary>
    void Die()
    {
        // Debug.Log($"[Health] {gameObject.name} 死亡!");
        onDeath?.Invoke();
        Destroy(gameObject, 0.1f);
    }
}