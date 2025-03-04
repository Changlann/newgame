using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [Header("声音设置")]
    public AudioClip[] footstepSounds;     // 脚步声音数组
    public float stepInterval = 0.5f;       // 脚步声间隔
    public float minMoveSpeed = 0.1f;       // 最小移动速度阈值
    public float volumeMin = 0.4f;          // 最小音量
    public float volumeMax = 0.8f;          // 最大音量

    [Header("上下摆动设置")]
    public float bobSpeed = 10f;            // 摆动速度
    public float bobAmount = 0.1f;          // 摆动幅度
    public float sprintBobAmount = 0.15f;   // 冲刺时的摆动幅度

    private AudioSource audioSource;
    private float stepTimer;
    private float bobTimer;
    private Vector3 originalHeight;
    private CharacterController controller;
    private HealerMovement healerMovement;  // 引用HealerMovement脚本
    private int lastSoundIndex = -1;

    void Start()
    {
        // 获取或添加需要的组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;  // 3D音效
            audioSource.volume = volumeMin;
        }

        controller = GetComponent<CharacterController>();
        healerMovement = GetComponent<HealerMovement>();
        if (Camera.main != null)
        {
            originalHeight = Camera.main.transform.localPosition;
        }
    }

    void Update()
    {
        if (controller == null || Camera.main == null) return;

        float speed = controller.velocity.magnitude;
        bool isMoving = speed > minMoveSpeed;
        bool isGrounded = controller.isGrounded;

        // 处理脚步声
        if (isMoving && isGrounded)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstep(speed);
                stepTimer = 0;
            }
        }
        else
        {
            stepTimer = 0;
        }

        // 处理上下摆动
        if (isMoving && isGrounded)
        {
            float currentBobAmount = healerMovement != null && healerMovement.IsSprinting()
                ? this.sprintBobAmount
                : this.bobAmount;

            bobTimer += Time.deltaTime * bobSpeed;

            // 计算上下摆动的位移
            float yOffset = Mathf.Sin(bobTimer) * currentBobAmount;

            // 应用位移到摄像机
            Vector3 newPosition = new Vector3(
                originalHeight.x,
                originalHeight.y + yOffset,
                originalHeight.z
            );
            Camera.main.transform.localPosition = newPosition;
        }
        else
        {
            // 逐渐恢复到原始高度
            bobTimer = 0;
            Camera.main.transform.localPosition = Vector3.Lerp(
                Camera.main.transform.localPosition,
                originalHeight,
                Time.deltaTime * 5f
            );
        }
    }

    void PlayFootstep(float speed)
    {
        if (footstepSounds.Length == 0) return;

        // 随机选择一个不同于上次的音效
        int newIndex;
        do
        {
            newIndex = Random.Range(0, footstepSounds.Length);
        } while (footstepSounds.Length > 1 && newIndex == lastSoundIndex);

        lastSoundIndex = newIndex;

        // 根据移动速度调整音量
        float volume = Mathf.Lerp(volumeMin, volumeMax, speed / 10f);
        audioSource.PlayOneShot(footstepSounds[newIndex], volume);
    }
}