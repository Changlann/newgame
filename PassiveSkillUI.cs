using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveSkillUI : MonoBehaviour
{
    [Header("UI����")]
    public TextMeshProUGUI totalKillsText;
    public TextMeshProUGUI sessionKillsText;
    public GameObject skillAPanel;
    public Image skillALockIcon;
    public TextMeshProUGUI skillAStatus;

    [Header("��������")]
    public float updateInterval = 0.5f; // ÿ0.5�����һ�Σ�����ÿ֡����

    private float updateTimer = 0f;
    private PassiveSkillManager skillManager;

    void Start()
    {
        // Ѱ�Ҽ��ܹ�����
        skillManager = PassiveSkillManager.Instance;

        if (skillManager == null)
        {
            Debug.LogWarning("[PassiveSkillUI] �Ҳ���PassiveSkillManagerʵ����");
            enabled = false;
            return;
        }

        // ��ʼ����UI
        UpdateUI();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            UpdateUI();
            updateTimer = 0f;
        }
    }

    void UpdateUI()
    {
        if (skillManager == null) return;

        // ���»�ɱ����
        if (totalKillsText != null)
        {
            totalKillsText.text = $"�ܻ�ɱ: {skillManager.GetTotalKills()}";
        }

        if (sessionKillsText != null)
        {
            sessionKillsText.text = $"���ֻ�ɱ: {skillManager.GetCurrentSessionKills()}";
        }

        // ���¼���A״̬
        bool skillAUnlocked = skillManager.IsSkillAUnlocked();

        if (skillAPanel != null)
        {
            if (skillALockIcon != null)
            {
                skillALockIcon.gameObject.SetActive(!skillAUnlocked);
            }

            if (skillAStatus != null)
            {
                if (skillAUnlocked)
                {
                    skillAStatus.text = "�ѽ���: ��β��������";
                    skillAStatus.color = Color.green;
                }
                else
                {
                    int requiredKills = skillManager.skillAUnlockKills;
                    int currentKills = skillManager.GetTotalKills();
                    skillAStatus.text = $"��������: {currentKills}/{requiredKills}";
                    skillAStatus.color = Color.yellow;
                }
            }
        }
    }
}