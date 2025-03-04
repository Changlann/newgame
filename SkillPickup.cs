using UnityEngine;
using TMPro;

/// <summary>
/// ����ʰȡ�� - �������ʰȡ��ü���
/// </summary>
public class SkillPickup : MonoBehaviour
{
    [Header("������Ϣ")]
    [Tooltip("����Ԥ��������")]
    public GameObject skillPrefab;

    [Tooltip("��������")]
    public string skillName = "δ֪����";

    [Tooltip("����ͼ��")]
    public Sprite skillIcon;

    [Tooltip("��������")]
    [TextArea(3, 5)]
    public string skillDescription = "��������";

    [Header("�������")]
    [Tooltip("��ת�ٶ�")]
    public float rotationSpeed = 50f;

    [Tooltip("�����߶�")]
    public float hoverHeight = 0.5f;

    [Tooltip("�����ٶ�")]
    public float hoverSpeed = 1f;

    [Tooltip("ʰȡ��ⷶΧ")]
    public float pickupRadius = 2f;

    [Tooltip("��ʾ�ı�Ԥ����")]
    public GameObject tooltipPrefab;

    // ˽�б���
    private float initialY;
    private TextMeshProUGUI tooltipText;
    private GameObject tooltipInstance;
    private Transform player;
    private bool isPlayerNearby = false;

    void Start()
    {
        initialY = transform.position.y;

        // �������
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // ������ʾ�ı�
        if (tooltipPrefab != null)
        {
            tooltipInstance = Instantiate(tooltipPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            tooltipInstance.transform.SetParent(transform);

            tooltipText = tooltipInstance.GetComponent<TextMeshProUGUI>();
            if (tooltipText != null)
            {
                tooltipText.text = $"�� F ��ȡ����: {skillName}";
                tooltipInstance.SetActive(false);
            }
        }

        // ��ӷ���Ч��
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.cyan * 2f);
        }
    }

    void Update()
    {
        // ��תЧ��
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // ����Ч��
        float newY = initialY + Mathf.Sin(Time.time * hoverSpeed) * 0.2f + hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // �������Ƿ��ڷ�Χ��
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= pickupRadius)
            {
                if (!isPlayerNearby)
                {
                    isPlayerNearby = true;
                    if (tooltipInstance != null)
                    {
                        tooltipInstance.SetActive(true);
                    }
                }

                // ��ⰴ��ʰȡ
                if (Input.GetKeyDown(KeyCode.F))
                {
                    PickupSkill();
                }
            }
            else if (isPlayerNearby)
            {
                isPlayerNearby = false;
                if (tooltipInstance != null)
                {
                    tooltipInstance.SetActive(false);
                }
            }

            // ʹ��ʾ�ı�ʼ���������
            if (tooltipInstance != null && tooltipInstance.activeSelf)
            {
                tooltipInstance.transform.LookAt(Camera.main.transform);
                tooltipInstance.transform.Rotate(0, 180, 0);
            }
        }
    }

    /// <summary>
    /// ʰȡ����
    /// </summary>
    private void PickupSkill()
    {
        if (skillPrefab != null && ActiveSkillManager.Instance != null)
        {
            ActiveSkillManager.Instance.AcquireSkill(skillPrefab);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��ײ��⣨���÷�����
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (tooltipInstance != null)
            {
                tooltipInstance.SetActive(true);
            }
        }
    }

    /// <summary>
    /// ��ײ�������
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (tooltipInstance != null)
            {
                tooltipInstance.SetActive(false);
            }
        }
    }

    /// <summary>
    /// �ڳ�����ͼ����ʾʰȡ��Χ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}