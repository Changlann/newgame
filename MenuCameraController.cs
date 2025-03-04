using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [Header("�ƶ�����")]
    [Tooltip("����ƶ������Ƕ�")]
    public float maxTiltAngle = 2.0f;
    [Tooltip("����ƶ������λ��")]
    public float maxOffset = 0.1f;
    [Tooltip("����ƶ���ƽ����")]
    public float smoothSpeed = 5.0f;

    [Header("��ʼλ��")]
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Ŀ��λ�ú���ת
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        // �����ʼλ�ú���ת
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // ��ʼ��Ŀ��ֵ
        targetPosition = initialPosition;
        targetRotation = initialRotation;
    }

    void Update()
    {
        // ��ȡ�������Ļ�ϵ�λ�ã���һ���� -1 �� 1 ֮�䣩
        float mouseX = (Input.mousePosition.x / Screen.width) * 2 - 1;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2 - 1;

        // ����Ŀ��λ��
        Vector3 offsetPosition = new Vector3(
            mouseX * maxOffset,
            mouseY * maxOffset,
            0
        );
        targetPosition = initialPosition + offsetPosition;

        // ����Ŀ����ת
        Vector3 tiltAngles = new Vector3(
            -mouseY * maxTiltAngle,
            mouseX * maxTiltAngle,
            0
        );
        targetRotation = initialRotation * Quaternion.Euler(tiltAngles);

        // ƽ���ƶ����
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        // ƽ����ת���
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    // �������λ��
    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}