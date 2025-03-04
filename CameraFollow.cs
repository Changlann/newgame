using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // �������ǵ�Transform
    private Vector3 initialOffset; // ��ʼƫ��

    void Start()
    {
        // ��¼����������ǵĳ�ʼ���λ��
        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // ���ֳ�ʼ���λ��
            transform.position = target.position + initialOffset;
        }
    }
}