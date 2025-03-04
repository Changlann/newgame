using UnityEngine;
using System.Collections.Generic;

public class HealingCircle : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("���Ʒ�Χ�뾶")]
    public float healRadius = 3f;
    [Tooltip("ÿ��������")]
    public float healAmountPerSecond = 5f;
    [Tooltip("����ʱ�䣨�룩")]
    public float duration = 10f;
    [Tooltip("�����Ŀ��")]
    public Transform target;

    [Header("�Ӿ�Ч��")]
    [Tooltip("���ƹ⻷��ɫ")]
    public Color healColor = new Color(0.2f, 1f, 0.2f, 0.4f);
    [Tooltip("�⻷�����ٶ�")]
    public float pulseSpeed = 1.5f;
    [Tooltip("������С�仯��Χ")]
    public float pulseRange = 0.2f;

    // ˽�б���
    private float timer = 0f;
    private MeshRenderer circleRenderer;
    private List<Health> healedTargets = new List<Health>();

    void Start()
    {
        // �����Ӿ�Ч��
        CreateVisualEffect();

        // ���û������Ŀ�꣬����ʹ�ø�����
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }
    }

    void Update()
    {
        // ����Ŀ��
        if (target != null)
        {
            transform.position = target.position;
        }

        // ��ʱ
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // �Ӿ�Ч�� - ����
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseRange;
        transform.localScale = new Vector3(scale, scale, scale) * healRadius;

        // ����Ч��
        HealNearbyAllies();
    }

    void CreateVisualEffect()
    {
        // ����һ��Բ��ƽ����Ϊ���ƹ⻷
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.transform.SetParent(transform);
        circle.transform.localPosition = new Vector3(0, 0.1f, 0); // ��΢���ߣ�����͵����ص�
        circle.transform.localScale = new Vector3(1, 0.02f, 1); // ��ƽ��

        // �Ƴ���ײ��
        Destroy(circle.GetComponent<Collider>());

        // ���ò���
        circleRenderer = circle.GetComponent<MeshRenderer>();
        Material circleMaterial = new Material(Shader.Find("Standard"));
        circleMaterial.color = healColor;
        circleMaterial.EnableKeyword("_EMISSION");
        circleMaterial.SetColor("_EmissionColor", healColor * 0.5f);

        // ������ȾģʽΪ͸��
        circleMaterial.SetFloat("_Mode", 3); // Transparent mode
        circleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        circleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        circleMaterial.SetInt("_ZWrite", 0);
        circleMaterial.DisableKeyword("_ALPHATEST_ON");
        circleMaterial.EnableKeyword("_ALPHABLEND_ON");
        circleMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        circleMaterial.renderQueue = 3000;

        circleRenderer.material = circleMaterial;
    }

    void HealNearbyAllies()
    {
        // �����һ�ε�����Ŀ���б�
        healedTargets.Clear();

        // ���ҷ�Χ�ڵ�������ײ��
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRadius);

        foreach (Collider collider in colliders)
        {
            // ֻ��������
            if (collider.CompareTag("SnakeBody"))
            {
                Health health = collider.GetComponent<Health>();
                if (health != null && !healedTargets.Contains(health))
                {
                    // ÿ������ָ����������ֵ
                    health.Heal(healAmountPerSecond * Time.deltaTime);
                    healedTargets.Add(health);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // ��Scene��ͼ����ʾ���Ʒ�Χ
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}