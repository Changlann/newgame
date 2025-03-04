using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������ܹ����� - �����ܵĻ�ȡ��������ʹ��
/// </summary>
public class ActiveSkillManager : MonoBehaviour
{
    // ����
    public static ActiveSkillManager Instance { get; private set; }

    [Header("��������")]
    [Tooltip("���ü���Ԥ�����б�")]
    public List<GameObject> skillPrefabs = new List<GameObject>();

    [Tooltip("���ܻ�ȡ��ƷԤ����")]
    public GameObject skillPickupPrefab;

    [Tooltip("ʰȡ��Ʒ�����Ч")]
    public AudioClip pickupSound;

    [Tooltip("������ͬʱӵ�еļ�������")]
    public int maxSkillSlots = 3;

    // �ѻ�õļ���
    [HideInInspector]
    public List<ActiveSkill> acquiredSkills = new List<ActiveSkill>();

    // ��ƵԴ
    private AudioSource audioSource;

    void Awake()
    {
        // ����ģʽ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // ��ȡ�����AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // ��ʼ������UI
        ActiveSkillUI.Instance?.InitializeSkillUI(maxSkillSlots);
    }

    /// <summary>
    /// ��������ʰȡ��
    /// </summary>
    /// <param name="position">����λ��</param>
    public void SpawnSkillPickup(Vector3 position)
    {
        if (skillPickupPrefab == null || skillPrefabs.Count == 0) return;

        // ���ѡ��һ������
        int randomSkillIndex = Random.Range(0, skillPrefabs.Count);
        GameObject randomSkillPrefab = skillPrefabs[randomSkillIndex];

        // ����ʰȡ��
        GameObject pickup = Instantiate(skillPickupPrefab, position, Quaternion.identity);

        // ����ʰȡ������
        SkillPickup pickupComponent = pickup.GetComponent<SkillPickup>();
        if (pickupComponent != null)
        {
            // ��ȡ������Ϣ
            ActiveSkill skillInfo = randomSkillPrefab.GetComponent<ActiveSkill>();
            if (skillInfo != null)
            {
                pickupComponent.skillPrefab = randomSkillPrefab;
                pickupComponent.skillName = skillInfo.skillName;
                pickupComponent.skillIcon = skillInfo.icon;
                pickupComponent.skillDescription = skillInfo.description;
            }
        }

        Debug.Log($"[ActiveSkillManager] �� {position} ���ɼ���ʰȡ��");
    }

    /// <summary>
    /// ��ȡ����
    /// </summary>
    /// <param name="skillPrefab">����Ԥ����</param>
    public void AcquireSkill(GameObject skillPrefab)
    {
        if (skillPrefab == null) return;

        // ����ʰȡ��Ч
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // ��ȡ��������
        ActiveSkill newSkillPrototype = skillPrefab.GetComponent<ActiveSkill>();
        if (newSkillPrototype == null)
        {
            Debug.LogError("[ActiveSkillManager] ����Ԥ���岻����ActiveSkill���");
            return;
        }

        // ����Ƿ�������ͬ����
        ActiveSkill existingSkill = null;
        foreach (ActiveSkill skill in acquiredSkills)
        {
            if (skill.GetType() == newSkillPrototype.GetType())
            {
                existingSkill = skill;
                break;
            }
        }

        // ���������ͬ���ܣ�������
        if (existingSkill != null)
        {
            existingSkill.LevelUp();
            Debug.Log($"[ActiveSkillManager] �������� {existingSkill.skillName} �� {existingSkill.level} ��");

            // ����UI
            ActiveSkillUI.Instance?.UpdateSkillUI();
        }
        // ��������¼���
        else
        {
            // ��鼼�ܲ��Ƿ�����
            if (acquiredSkills.Count >= maxSkillSlots)
            {
                Debug.Log("[ActiveSkillManager] ���ܲ��������޷���ȡ�¼���");
                return;
            }

            // ʵ��������
            GameObject skillObj = Instantiate(skillPrefab, transform);
            ActiveSkill newSkill = skillObj.GetComponent<ActiveSkill>();

            if (newSkill != null)
            {
                // ��ӵ��ѻ�ü����б�
                acquiredSkills.Add(newSkill);
                Debug.Log($"[ActiveSkillManager] ����¼���: {newSkill.skillName}");

                // ����UI
                ActiveSkillUI.Instance?.AddSkillToUI(newSkill);
            }
        }
    }

    /// <summary>
    /// �Ƴ�����
    /// </summary>
    /// <param name="skill">Ҫ�Ƴ��ļ���</param>
    public void RemoveSkill(ActiveSkill skill)
    {
        if (skill == null) return;

        // ���б����Ƴ�
        acquiredSkills.Remove(skill);

        // �Ƴ����ܶ���
        Destroy(skill.gameObject);

        // ����UI
        ActiveSkillUI.Instance?.UpdateSkillUI();

        Debug.Log($"[ActiveSkillManager] �Ƴ�����: {skill.skillName}");
    }

    /// <summary>
    /// ���ɼ���ʰȡ����ڲ��ԣ�
    /// </summary>
    public void SpawnRandomSkillPickupNearby()
    {
        if (skillPickupPrefab == null || skillPrefabs.Count == 0) return;

        // ����Ҹ������λ������
        Vector3 randomOffset = new Vector3(
            Random.Range(-5f, 5f),
            0.5f,
            Random.Range(-5f, 5f)
        );

        Vector3 spawnPosition = transform.position + randomOffset;
        SpawnSkillPickup(spawnPosition);
    }

    /// <summary>
    /// ֱ�ӻ�ȡ������ܣ����ڲ��ԣ�
    /// </summary>
    public void AcquireRandomSkill()
    {
        if (skillPrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, skillPrefabs.Count);
        AcquireSkill(skillPrefabs[randomIndex]);
    }
}