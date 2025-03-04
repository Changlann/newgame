using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("相机移动的最大角度")]
    public float maxTiltAngle = 2.0f;
    [Tooltip("相机移动的最大位移")]
    public float maxOffset = 0.1f;
    [Tooltip("相机移动的平滑度")]
    public float smoothSpeed = 5.0f;

    [Header("初始位置")]
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // 目标位置和旋转
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        // 保存初始位置和旋转
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // 初始化目标值
        targetPosition = initialPosition;
        targetRotation = initialRotation;
    }

    void Update()
    {
        // 获取鼠标在屏幕上的位置（归一化到 -1 到 1 之间）
        float mouseX = (Input.mousePosition.x / Screen.width) * 2 - 1;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2 - 1;

        // 计算目标位置
        Vector3 offsetPosition = new Vector3(
            mouseX * maxOffset,
            mouseY * maxOffset,
            0
        );
        targetPosition = initialPosition + offsetPosition;

        // 计算目标旋转
        Vector3 tiltAngles = new Vector3(
            -mouseY * maxTiltAngle,
            mouseX * maxTiltAngle,
            0
        );
        targetRotation = initialRotation * Quaternion.Euler(tiltAngles);

        // 平滑移动相机
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        // 平滑旋转相机
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    // 重置相机位置
    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}