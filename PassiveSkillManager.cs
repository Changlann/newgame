using UnityEngine;
using System.Collections.Generic;

public class PassiveSkillManager : MonoBehaviour
{
    [Header("���ܽ�������")]
    [Tooltip("����A�Ľ�����ɱ��")]
    public int skillAUnlockKills = 1000;

    [Header("�־û�����")]
    [Tooltip("�ܻ�ɱ��������Ϸ�洢��")]
    private int totalKills = 0;
    private string killCountKey = "TotalEnemyKills";

    [Header("������Ϣ")]
    [SerializeField] private bool isSkillAUnlocked = false;
    [SerializeField] private int currentSessionKills = 0;

    // ����ģʽ
    public static PassiveSkillManager Instance { get; private set; }

    void Awake()
    {
        // ����ʵ��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKillCount(); // ����֮ǰ�Ļ�ɱ��
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ��鼼�ܽ���״̬
        CheckSkillUnlocks();
    }

    // ��PlayerPrefs���ػ�ɱ����
    private void LoadKillCount()
    {
        totalKills = PlayerPrefs.GetInt(killCountKey, 0);
        Debug.Log($"[PassiveSkillManager] �����ܻ�ɱ��: {totalKills}");
    }

    // �����ɱ������PlayerPrefs
    private void SaveKillCount()
    {
        PlayerPrefs.SetInt(killCountKey, totalKills);
        PlayerPrefs.Save();
        Debug.Log($"[PassiveSkillManager] �����ܻ�ɱ��: {totalKills}");
    }

    public void ResetTotalKills()
    {
        totalKills = 0;
        SaveKillCount();
        CheckSkillUnlocks();
        Debug.Log("[PassiveSkillManager] �ܻ�ɱ��������Ϊ0");
    }

    // ���ӻ�ɱ����
    public void AddKill()
    {
        totalKills++;
        currentSessionKills++;

        // ����Ƿ�������¼���
        CheckSkillUnlocks();

        // ÿ10�λ�ɱ����һ�����ݣ�����Ƶ��д��
        if (totalKills % 10 == 0)
        {
            SaveKillCount();
        }
    }

    // ��鼼�ܽ���
    private void CheckSkillUnlocks()
    {
        // ��鼼��A�Ľ���״̬
        if (totalKills >= skillAUnlockKills && !isSkillAUnlocked)
        {
            isSkillAUnlocked = true;
            Debug.Log("[PassiveSkillManager] ����A�ѽ�������β��������");
        }
    }

    // ��ȡ����A�Ľ���״̬
    public bool IsSkillAUnlocked()
    {
        return isSkillAUnlocked;
    }

    // ����Ϸ����ʱ��������
    private void OnApplicationQuit()
    {
        SaveKillCount();
    }

    // ����UI��ʾ
    public int GetTotalKills()
    {
        return totalKills;
    }

    public int GetCurrentSessionKills()
    {
        return currentSessionKills;
    }

    // ���õ�ǰ�Ự��ɱ��������Ϸ���¿�ʼʱ���ã�
    public void ResetSessionKills()
    {
        currentSessionKills = 0;
    }
}