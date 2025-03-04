using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public float lifetime = 1f;    // ����ʱ�䣨�룩
    public float moveSpeed = 1f;  // �����ٶ�
    private TextMeshProUGUI textMesh;
    private float timer;
    private Color textColor;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textColor = textMesh.color;  // ��ȡ��ʼ��ɫ
            timer = lifetime;            // ���ü�ʱ��
        }
        else
        {
            Debug.LogError("�˺�����Ԥ����ȱ��TextMeshProUGUI�����");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // �����ƶ�
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // ������
        timer -= Time.deltaTime;
        float alpha = timer / lifetime;  // ����͸����
        textColor.a = alpha;             // ����͸����

        if (textMesh != null)
        {
            textMesh.color = textColor;  // Ӧ����ɫ
        }

        // ʱ�䵽������
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamageText(int damage)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }

        if (textMesh != null)
        {
            textMesh.text = damage.ToString();  // ������ʾ������
        }
        else
        {
            Debug.LogError("�޷������˺����֣�ȱ��TextMeshProUGUI�����");
        }
    }
}