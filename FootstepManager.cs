using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [Header("��������")]
    public AudioClip[] footstepSounds;     // �Ų���������
    public float stepInterval = 0.5f;       // �Ų������
    public float minMoveSpeed = 0.1f;       // ��С�ƶ��ٶ���ֵ
    public float volumeMin = 0.4f;          // ��С����
    public float volumeMax = 0.8f;          // �������

    [Header("���°ڶ�����")]
    public float bobSpeed = 10f;            // �ڶ��ٶ�
    public float bobAmount = 0.1f;          // �ڶ�����
    public float sprintBobAmount = 0.15f;   // ���ʱ�İڶ�����

    private AudioSource audioSource;
    private float stepTimer;
    private float bobTimer;
    private Vector3 originalHeight;
    private CharacterController controller;
    private HealerMovement healerMovement;  // ����HealerMovement�ű�
    private int lastSoundIndex = -1;

    void Start()
    {
        // ��ȡ�������Ҫ�����
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;  // 3D��Ч
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

        // ����Ų���
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

        // �������°ڶ�
        if (isMoving && isGrounded)
        {
            float currentBobAmount = healerMovement != null && healerMovement.IsSprinting()
                ? this.sprintBobAmount
                : this.bobAmount;

            bobTimer += Time.deltaTime * bobSpeed;

            // �������°ڶ���λ��
            float yOffset = Mathf.Sin(bobTimer) * currentBobAmount;

            // Ӧ��λ�Ƶ������
            Vector3 newPosition = new Vector3(
                originalHeight.x,
                originalHeight.y + yOffset,
                originalHeight.z
            );
            Camera.main.transform.localPosition = newPosition;
        }
        else
        {
            // �𽥻ָ���ԭʼ�߶�
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

        // ���ѡ��һ����ͬ���ϴε���Ч
        int newIndex;
        do
        {
            newIndex = Random.Range(0, footstepSounds.Length);
        } while (footstepSounds.Length > 1 && newIndex == lastSoundIndex);

        lastSoundIndex = newIndex;

        // �����ƶ��ٶȵ�������
        float volume = Mathf.Lerp(volumeMin, volumeMax, speed / 10f);
        audioSource.PlayOneShot(footstepSounds[newIndex], volume);
    }
}