using UnityEngine;

[RequireComponent(typeof(Health))]
public class PassiveHealing : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("����ֵ�������ֵ�İٷֱ�ʱ����")]
    [Range(0.1f, 0.5f)]
    public float healthThreshold = 0.3f; // Ĭ��30%

    [Header("����Ч��")]
    [Tooltip("���ƹ⻷Ԥ����")]
    public GameObject healingCirclePrefab;
    [Tooltip("����Ч������ʱ��")]
    public float healingDuration = 10f;
    [Tooltip("��ȴʱ�䣨�룩")]
    public float cooldown = 30f;
    [Tooltip("ÿ���������ӵ��������ֵ�ٷֱ�")]
    [Range(0.01f, 0.2f)]
    public float maxHealthIncreasePercent = 0.1f; // 10%

    // ˽�б���
    private Health health;
    private float cooldownTimer = 0f;
    private bool healingActive = false;
    private GameObject currentHealingCircle;

    void Start()
    {
        health = GetComponent<Health>();

        // ���û��Ԥ��ָ�����ƹ⻷Ԥ���壬���Լ���
        if (healingCirclePrefab == null)
        {
            healingCirclePrefab = Resources.Load<GameObject>("HealingCircle");
            if (healingCirclePrefab == null)
            {
                Debug.LogWarning("[PassiveHealing] �Ҳ���HealingCircleԤ���壡��ȷ���Ѵ�������Inspector��ָ����");
            }
        }
    }

    void Update()
    {
        // ����δ����ʱ��ִ��
        if (PassiveSkillManager.Instance == null || !PassiveSkillManager.Instance.IsSkillAUnlocked())
        {
            return;
        }

        // ��ȴ��
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // ��齡��״̬��������ֵ������ֵ������ȴ���ʱ��������
        if (!healingActive && health != null && health.currentHealth < health.maxHealth * healthThreshold)
        {
            ActivateHealing();
        }
    }

    void ActivateHealing()
    {
        healingActive = true;
        cooldownTimer = cooldown;

        // �������ƹ⻷
        if (healingCirclePrefab != null)
        {
            currentHealingCircle = Instantiate(healingCirclePrefab, transform.position, Quaternion.identity);

            // �������ƹ⻷
            HealingCircle healingCircle = currentHealingCircle.GetComponent<HealingCircle>();
            if (healingCircle != null)
            {
                healingCircle.target = transform;
                healingCircle.duration = healingDuration;

                // �����������ֵ
                IncreaseMaxHealth();
            }
            else
            {
                Debug.LogError("[PassiveHealing] ���ɵ����ƹ⻷û��HealingCircle�����");
            }

            // �ָ���ʱ
            Invoke("EndHealing", healingDuration);
        }
    }

    void EndHealing()
    {
        healingActive = false;

        // ���ٹ⻷������������ڣ�
        if (currentHealingCircle != null)
        {
            Destroy(currentHealingCircle);
        }
    }

    void IncreaseMaxHealth()
    {
        if (health != null)
        {
            // ����������
            float increase = health.maxHealth * maxHealthIncreasePercent;

            // �����������ֵ
            health.maxHealth += increase;

            // ͬʱ�ָ���������ֵ
            health.Heal(increase);

            Debug.Log($"[PassiveHealing] {gameObject.name} �������ֵ������ {increase:F1} �㣬����Ϊ {health.maxHealth:F1}");
        }
    }

    void OnDestroy()
    {
        // ȷ��������ʱҲ�������ƹ⻷
        if (currentHealingCircle != null)
        {
            Destroy(currentHealingCircle);
        }
    }
}