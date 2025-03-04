using UnityEngine;

/// <summary>
/// Boss���䴦�� - ����Boss��������似��
/// </summary>
public class BossDropHandler : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("�Ƿ���Boss")]
    public bool isBoss = true;

    [Tooltip("��ͨ���˵��似�ܵĸ���")]
    [Range(0f, 1f)]
    public float dropChance = 0.05f;

    [Tooltip("������ЧԤ����")]
    public GameObject dropEffectPrefab;

    // ����
    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("[BossDropHandler] û��Health�����");
            enabled = false;
            return;
        }

        // ע�������¼�
        health.onDeath.AddListener(OnEnemyDeath);
    }

    /// <summary>
    /// ������������
    /// </summary>
    private void OnEnemyDeath()
    {
        if (ActiveSkillManager.Instance == null) return;

        // Boss�ض����似�ܣ���ͨ���˰����ʵ���
        bool shouldDrop = isBoss || (Random.value < dropChance);

        if (shouldDrop)
        {
            // ���ŵ�����Ч
            if (dropEffectPrefab != null)
            {
                Instantiate(dropEffectPrefab, transform.position, Quaternion.identity);
            }

            // ���ɼ���ʰȡ��
            ActiveSkillManager.Instance.SpawnSkillPickup(transform.position + Vector3.up * 0.5f);

            Debug.Log($"[BossDropHandler] {(isBoss ? "Boss" : "����")}�����˼���!");
        }
    }
}