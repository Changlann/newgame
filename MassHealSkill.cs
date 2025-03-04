using System.Collections;
using UnityEngine;

/// <summary>
/// Ⱥ�����Ƽ��� - �������������ֲ��ṩ��ʱ����
/// </summary>
public class MassHealSkill : ActiveSkill
{
    [Header("��������")]
    [Tooltip("�������������ٷֱ��������ֵ��")]
    [Range(0.1f, 1.0f)]
    public float baseHealPercent = 0.3f;

    [Tooltip("ÿ�����ӵ�������")]
    [Range(0.05f, 0.2f)]
    public float healPercentPerLevel = 0.1f;

    [Tooltip("���Ʒ�Χ")]
    public float healRadius = 15f;

    [Tooltip("��ʱ���ܳ���ʱ�䣨�룩")]
    public float shieldDuration = 5f;

    [Tooltip("ÿ�����ӵĻ��ܳ���ʱ��")]
    public float shieldDurationPerLevel = 1f;

    [Tooltip("���ܼ��˱���")]
    [Range(0.1f, 0.9f)]
    public float damageReduction = 0.5f;

    [Tooltip("������ЧԤ����")]
    public GameObject healEffectPrefab;

    [Tooltip("������ЧԤ����")]
    public GameObject shieldEffectPrefab;

    [Tooltip("����״̬��ɫ")]
    public Color shieldColor = new Color(0.5f, 1f, 0.5f, 0.7f);

    // ����
    private SnakeBody snakeBody;

    protected override void Awake()
    {
        base.Awake();

        // ����Ĭ��ֵ
        skillName = "Ⱥ������";
        description = "��������������һ���ٷֱȵ�����ֵ�����ṩ��ʱ�˺����⻤�ܡ�";
        skillKey = KeyCode.R; // ���Ը�����Ҫ�޸�

        // ��ȡSnakeBody���
        snakeBody = GetComponent<SnakeBody>();
        if (snakeBody == null)
        {
            snakeBody = FindObjectOfType<SnakeBody>();
        }

        if (snakeBody == null)
        {
            Debug.LogError("[MassHealSkill] �޷��ҵ�SnakeBody�����");
            enabled = false;
        }
    }

    protected override void Execute()
    {
        if (snakeBody == null) return;

        StartCoroutine(PerformMassHeal());
    }

    /// <summary>
    /// ִ��Ⱥ������
    /// </summary>
    private IEnumerator PerformMassHeal()
    {
        // ����ʵ���������ͻ��ܳ���ʱ��
        float actualHealPercent = baseHealPercent + (level - 1) * healPercentPerLevel;
        float actualShieldDuration = shieldDuration + (level - 1) * shieldDurationPerLevel;

        // ��ȡ����������
        Transform[] bodyParts = snakeBody.GetBodyPartTransforms();

        if (bodyParts.Length == 0)
        {
            Debug.LogWarning("[MassHealSkill] û�������ֿ�������");
            yield break;
        }

        // �����λ�ô���������Ч
        if (healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            Destroy(healEffect, 3f);
        }

        // Ϊÿ�����岿��Ӧ�����ƺͻ���
        foreach (Transform bodyPart in bodyParts)
        {
            Health health = bodyPart.GetComponent<Health>();
            if (health != null)
            {
                // ����������
                float healAmount = health.maxHealth * actualHealPercent;

                // Ӧ������
                health.Heal(healAmount);

                // Ӧ�û���Ч����ʹ���޵�״̬ģ�⣩
                health.SetInvincible(actualShieldDuration);

                // Ϊÿ�����ִ���������Ч
                if (shieldEffectPrefab != null)
                {
                    GameObject shieldEffect = Instantiate(shieldEffectPrefab, bodyPart.position, Quaternion.identity);
                    shieldEffect.transform.SetParent(bodyPart);
                    Destroy(shieldEffect, actualShieldDuration);
                }

                // ������ɫ��ָʾ����״̬
                MeshRenderer renderer = bodyPart.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // ����ԭʼ��ɫ
                    Color originalColor = renderer.material.color;

                    // ���û�����ɫ
                    renderer.material.color = Color.Lerp(originalColor, shieldColor, 0.5f);

                    // �ڻ��ܽ���ʱ�ָ�ԭʼ��ɫ
                    StartCoroutine(RestoreColor(renderer, originalColor, actualShieldDuration));
                }
            }
        }

        Debug.Log($"[MassHealSkill] Ⱥ��������ʩ�ţ����� {(actualHealPercent * 100):F0}% �������ֵ�����ܳ��� {actualShieldDuration} ��");

        yield return null;
    }

    /// <summary>
    /// �ָ���Ⱦ����ԭʼ��ɫ
    /// </summary>
    private IEnumerator RestoreColor(MeshRenderer renderer, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// �ڳ�����ͼ����ʾ���Ʒ�Χ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}