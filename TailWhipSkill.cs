using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��β˦����������
/// </summary>
public class TailWhipSkill : ActiveSkill
{
    [Header("˦β����")]
    [Tooltip("˦β��������ʱ�䣨�룩")]
    public float whipDuration = 1.5f;

    [Tooltip("˦β�޵�״̬����ʱ�䣨�룩")]
    public float invincibleDuration = 2f;

    [Tooltip("�����˺�ֵ")]
    public int baseDamage = 30;

    [Tooltip("ÿ�����ӵ��˺�")]
    public int damagePerLevel = 10;

    [Tooltip("�ٶ��˺�ϵ�����˺� = �����˺� + �ƶ��ٶ� * ϵ����")]
    public float speedDamageMultiplier = 5f;

    [Tooltip("�����ж��뾶")]
    public float attackRadius = 2.5f;

    [Tooltip("������ЧԤ����")]
    public GameObject whipEffectPrefab;

    [Tooltip("��β���ֱ�ɫ������״̬��")]
    public Color whipActiveColor = new Color(1f, 0.5f, 0f);

    // ����
    private SnakeBody snakeBody;
    private List<Health> affectedEnemies = new List<Health>();

    // ״̬
    private bool isWhipping = false;

    protected override void Awake()
    {
        base.Awake();

        // ����Ĭ��ֵ
        skillName = "��β˦��";
        description = "ʹ��β������ݵ��޵�״̬�����ԽӴ����ĵ�������˺����˺�����β�ƶ��ٶ��йء�";
        skillKey = KeyCode.Q;

        // ��ȡSnakeBody���
        snakeBody = GetComponent<SnakeBody>();
        if (snakeBody == null)
        {
            snakeBody = FindObjectOfType<SnakeBody>();
        }

        if (snakeBody == null)
        {
            Debug.LogError("[TailWhipSkill] �޷��ҵ�SnakeBody�����");
            enabled = false;
        }
    }

    protected override void Execute()
    {
        if (isWhipping || snakeBody == null) return;

        StartCoroutine(PerformTailWhip());
    }

    /// <summary>
    /// ִ����β˦������
    /// </summary>
    private IEnumerator PerformTailWhip()
    {
        isWhipping = true;
        affectedEnemies.Clear();

        // ��ȡ����������
        Transform[] bodyParts = snakeBody.GetBodyPartTransforms();
        if (bodyParts.Length == 0)
        {
            Debug.LogWarning("[TailWhipSkill] û�������ֿ�����˦β����");
            isWhipping = false;
            yield break;
        }

        // ������β�޵�״̬
        foreach (Transform bodyPart in bodyParts)
        {
            Health health = bodyPart.GetComponent<Health>();
            if (health != null)
            {
                health.SetInvincible(invincibleDuration);
            }

            // ������β��ɫ��ָʾ����״̬
            MeshRenderer renderer = bodyPart.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // ����ԭʼ��ɫ
                Color originalColor = renderer.material.color;

                // ���ù���������ɫ
                renderer.material.color = whipActiveColor;

                // ��Э�̽���ʱ�ָ�ԭʼ��ɫ
                StartCoroutine(RestoreColor(renderer, originalColor, whipDuration));
            }
        }

        // ����������Ч������У�
        if (whipEffectPrefab != null && bodyParts.Length > 0)
        {
            Instantiate(whipEffectPrefab, bodyParts[bodyParts.Length - 1].position, Quaternion.identity);
        }

        // ��˦β����ʱ���ڼ����ײ
        float elapsed = 0f;
        while (elapsed < whipDuration)
        {
            elapsed += Time.deltaTime;

            // �������������ּ�����
            foreach (Transform bodyPart in bodyParts)
            {
                // ������һ֡���ƶ��ٶ�
                Vector3 currentPos = bodyPart.position;
                float deltaTime = Time.deltaTime;
                yield return null; // �ȴ���һ֡
                Vector3 newPos = bodyPart.position;
                float speed = Vector3.Distance(currentPos, newPos) / deltaTime;

                // �����Χ�ĵ���
                Collider[] colliders = Physics.OverlapSphere(bodyPart.position, attackRadius);
                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        Health enemyHealth = collider.GetComponent<Health>();
                        if (enemyHealth != null && !affectedEnemies.Contains(enemyHealth))
                        {
                            // �����˺��������˺� + �ȼ��ӳ� + �ٶȼӳɣ�
                            int damage = baseDamage + (level - 1) * damagePerLevel + Mathf.RoundToInt(speed * speedDamageMultiplier);

                            // �Ե�������˺�
                            enemyHealth.TakeDamage(damage, true);

                            // ��ӵ���Ӱ���б���ֹ�ظ��˺�
                            affectedEnemies.Add(enemyHealth);

                            Debug.Log($"[TailWhipSkill] ˦β�������е��ˣ���� {damage} ���˺����ٶȣ�{speed:F2}��");
                        }
                    }
                }
            }

            yield return null;
        }

        isWhipping = false;
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
    /// �ڳ�����ͼ����ʾ������Χ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        SnakeBody snakeBody = GetComponent<SnakeBody>();
        if (snakeBody == null) return;

        Transform[] bodyParts = snakeBody.GetBodyPartTransforms();
        if (bodyParts == null || bodyParts.Length == 0) return;

        Gizmos.color = Color.red;
        foreach (Transform bodyPart in bodyParts)
        {
            Gizmos.DrawWireSphere(bodyPart.position, attackRadius);
        }
    }
}