using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("����ֵ����")]
    [Tooltip("�������ֵ")]
    public float maxHealth = 100f;
    [Tooltip("��ǰ����ֵ")]
    public float currentHealth;

    [Header("�޵�ʱ������")]
    [Tooltip("�޵�ʱ�����ʱ���")]
    private float invincibleUntil = 0f;

    [Header("��ɫ����")]
    [Tooltip("��Ѫʱ����ɫ")]
    public Color fullHealthColor = Color.white;   // ��Ѫ��ɫ
    [Tooltip("��Ѫʱ����ɫ")]
    public Color noHealthColor = Color.red;       // ��Ѫ��ɫ
    [Tooltip("�Ƿ���ɫӦ�õ�������")]
    public bool applyColorToChildren = true;      // �Ƿ�Ӧ����ɫ��������
    [Tooltip("������ɫ�Ļ��ǿ��")]
    [Range(0f, 1f)]
    public float colorIntensity = 0.5f;          // ������ɫ�Ļ��ǿ��

    [Header("�¼�")]
    [Tooltip("����ʱ�������¼�")]
    public UnityEvent onDeath;
    [Tooltip("����ʱ�������¼�")]
    public UnityEvent<float> onDamaged;
    [Tooltip("����ʱ�������¼�")]
    public UnityEvent<float> onHealed;

    private MaterialPropertyBlock propBlock;
    private MeshRenderer[] allRenderers;

    void Start()
    {
        currentHealth = maxHealth;
        // Debug.Log($"[Health] {gameObject.name} ��ʼ�� - �������ֵ: {maxHealth}, ��ǰ����ֵ: {currentHealth}");

        // ��ȡ������ص���Ⱦ��
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
            // Debug.LogWarning($"[Health] {gameObject.name} û���ҵ��κ� MeshRenderer ���!");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        // �ӳ�һ֡������ɫ��ȷ�������Ѿ�����
        Invoke("UpdateColor", 0.1f);
    }

    /// <summary>
    /// �����޵�״̬
    /// </summary>
    /// <param name="duration">�޵г���ʱ��(��)</param>
    public void SetInvincible(float duration)
    {
        invincibleUntil = Time.time + duration;
    }

    /// <summary>
    /// �ܵ��˺�
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
    /// <param name="showNumber">�Ƿ���ʾ�˺�����</param>
    public void TakeDamage(float damage, bool showNumber = false)
    {
        if (Time.time < invincibleUntil)
        {
            // Debug.Log($"[Health] {gameObject.name} �����޵�״̬�������˺�!");
            return;
        }

        // Debug.Log($"[Health] {gameObject.name} �ܵ� {damage} ���˺�. ֮ǰѪ��: {currentHealth}");

        currentHealth -= damage;
        onDamaged?.Invoke(damage);

        // ֻ�� showNumber Ϊ true ʱ��ʾ�˺�����
        if (showNumber && CompareTag("Enemy"))  // ȷ��ֻ�е��˲���ʾ�˺�����
        {
            DamageNumberManager.Instance?.ShowDamageNumber((int)damage, transform.position + Vector3.up);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Debug.Log($"[Health] {gameObject.name} Ѫ������0����������");
            Die();
        }
        else
        {
            // Debug.Log($"[Health] {gameObject.name} ���˺�ʣ��Ѫ��: {currentHealth}");
            UpdateColor();
        }
    }

    /// <summary>
    /// ������ɫ�Է�ӳ��ǰ����ֵ
    /// </summary>
    void UpdateColor()
    {
        if (allRenderers == null || allRenderers.Length == 0) return;

        float healthPercent = currentHealth / maxHealth;
        Color healthColor = Color.Lerp(noHealthColor, fullHealthColor, healthPercent);

        // ����������Ⱦ��
        foreach (MeshRenderer renderer in allRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // ��ȡ��ǰ�Ĳ�����ɫ
                Color currentColor = renderer.material.color;

                // ����ǰ������ɫ��Ѫ����ɫ���л��
                Color finalColor = Color.Lerp(currentColor, healthColor, colorIntensity);

                // Ӧ������ɫ
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", finalColor);
                renderer.SetPropertyBlock(propBlock);
            }
        }

        // Debug.Log($"[Health] {gameObject.name} ������ɫ - Ѫ���ٷֱ�: {healthPercent:P0}");
    }

    /// <summary>
    /// �ָ�����ֵ
    /// </summary>
    /// <param name="amount">������</param>
    public void Heal(float amount)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        float actualHealAmount = currentHealth - oldHealth;

        if (actualHealAmount > 0)
        {
            // Debug.Log($"[Health] {gameObject.name} �ָ� {actualHealAmount} ������ֵ. ��ǰѪ��: {currentHealth}");
            onHealed?.Invoke(actualHealAmount);
            UpdateColor();
        }
    }

    /// <summary>
    /// ���������߼�
    /// </summary>
    void Die()
    {
        // Debug.Log($"[Health] {gameObject.name} ����!");
        onDeath?.Invoke();
        Destroy(gameObject, 0.1f);
    }
}