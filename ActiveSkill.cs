using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �������ܻ��࣬�����������ܶ��̳��������
/// </summary>
public abstract class ActiveSkill : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("��������")]
    public string skillName = "����";

    [Tooltip("��������")]
    [TextArea(3, 5)]
    public string description = "��������";

    [Tooltip("����ͼ��")]
    public Sprite icon;

    [Tooltip("���ܼ�")]
    public KeyCode skillKey = KeyCode.Q;

    [Header("��ȴ����")]
    [Tooltip("������ȴʱ�䣨�룩")]
    public float baseCooldown = 10f;

    [Tooltip("��ǰ��ȴʱ��")]
    public float currentCooldown = 0f;

    [Tooltip("���ܵȼ�")]
    public int level = 1;

    [Tooltip("ÿ�����ٵ���ȴʱ�䣨�룩")]
    public float cooldownReductionPerLevel = 1f;

    [Header("��Ч����")]
    [Tooltip("����ʹ��ʱ����Ч")]
    public AudioClip skillSound;

    [Tooltip("����ʹ��ʱ����ЧԤ����")]
    public GameObject skillEffectPrefab;

    [Header("�¼�")]
    [Tooltip("����ʹ��ʱ�������¼�")]
    public UnityEvent onSkillActivated;

    [Tooltip("������ȴ���ʱ�������¼�")]
    public UnityEvent onCooldownComplete;

    // ˽�б���
    private AudioSource audioSource;
    private bool isInitialized = false;

    protected virtual void Awake()
    {
        // ��ȡ�����AudioSource���
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
        // ������ȴ
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                currentCooldown = 0;
                onCooldownComplete?.Invoke();
            }
        }

        // ����������
        if (Input.GetKeyDown(skillKey) && currentCooldown <= 0)
        {
            ActivateSkill();
        }
    }

    /// <summary>
    /// �����
    /// </summary>
    public virtual void ActivateSkill()
    {
        if (currentCooldown > 0) return;

        // ����Ƿ��ѳ�ʼ��
        if (!isInitialized)
        {
            Debug.LogWarning($"[{skillName}] ����ִ��δ��ʼ���ļ���");
            return;
        }

        // ���ż�����Ч
        if (skillSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(skillSound);
        }

        // ʵ����������Ч
        if (skillEffectPrefab != null)
        {
            Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
        }

        // ����ʵ���߼�������ʵ��
        Execute();

        // ��ʼ��ȴ
        currentCooldown = GetActualCooldown();

        // �����¼�
        onSkillActivated?.Invoke();
    }

    /// <summary>
    /// ����ʵ��ִ���߼���������ʵ��
    /// </summary>
    protected abstract void Execute();

    /// <summary>
    /// ��ȡʵ����ȴʱ�䣨���ǵȼ����⣩
    /// </summary>
    public float GetActualCooldown()
    {
        return Mathf.Max(1f, baseCooldown - (level - 1) * cooldownReductionPerLevel);
    }

    /// <summary>
    /// ��������
    /// </summary>
    public virtual void LevelUp()
    {
        level++;
        Debug.Log($"���� {skillName} ������ {level} ��");
    }

    /// <summary>
    /// ��ȡ��ȴ���ȣ�0-1��
    /// </summary>
    public float GetCooldownProgress()
    {
        if (currentCooldown <= 0) return 0;
        return currentCooldown / GetActualCooldown();
    }
}