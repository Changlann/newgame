using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 拖入主角的Transform
    private Vector3 initialOffset; // 初始偏移

    void Start()
    {
        // 记录摄像机与主角的初始相对位置
        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // 保持初始相对位置
            transform.position = target.position + initialOffset;
        }
    }
}