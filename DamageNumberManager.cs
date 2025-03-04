using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public GameObject damageNumberPrefab;  // �˺�����Ԥ����
    public Canvas worldSpaceCanvas;        // ����ռ�Canvas
    public Vector3 offset = new Vector3(0, 2f, 0);  // ƫ���������� Inspector �е���

    private static DamageNumberManager instance;
    public static DamageNumberManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamageNumber(int damage, Vector3 position)
    {
        if (damageNumberPrefab == null || worldSpaceCanvas == null)
        {
            Debug.LogError("ȱ���˺�����Ԥ�����Canvas���ã�����DamageNumberManager���ã�");
            return;
        }

        // ʹ�� offset ��������λ��
        Vector3 spawnPosition = position + offset;

        // �����˺����֣�����ΪCanvas���Ӷ���
        GameObject numberObj = Instantiate(damageNumberPrefab, spawnPosition,
            Quaternion.identity, worldSpaceCanvas.transform);

        // ������ʼ�����������
        numberObj.transform.forward = Camera.main.transform.forward;

        DamageNumber damageNumber = numberObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.SetDamageText(damage);
        }
        else
        {
            Debug.LogError("�˺�����Ԥ����ȱ��DamageNumber�ű���");
        }
    }
}