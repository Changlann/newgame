using UnityEngine;

public class AreaSoundManager : MonoBehaviour
{
    [Header("音效设置")]
    public AudioClip areaSound;        // 区域音效
    public float maxVolume = 1f;       // 最大音量
    public float fadeSpeed = 2f;       // 淡入淡出速度
    public bool useDistanceBasedVolume = true; // 是否使用基于距离的音量

    [Header("声音范围设置")]
    public float minDistance = 2f;     // 最小距离（达到最大音量）
    public float maxDistance = 10f;    // 最大距离（音量为0）

    private AudioSource audioSource;
    private bool playerInRange = false;
    private Transform player;
    private float targetVolume = 0f;

    void Start()
    {
        // 确保有碰撞体并设置为触发器
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogError("AreaSoundManager需要一个碰撞体组件！");
            enabled = false;
            return;
        }

        // 设置音频源
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = areaSound;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;  // 3D音效
        audioSource.volume = 0f;
        audioSource.Play();

        // 查找玩家
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("场景中没有找到Player标签的物体！");
        }
    }

    void Update()
    {
        if (!playerInRange || player == null)
        {
            targetVolume = 0f;
        }
        else if (useDistanceBasedVolume)
        {
            // 计算玩家到最近碰撞体表面的距离
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Vector3 closestPoint = collider.ClosestPoint(player.position);
                float distance = Vector3.Distance(player.position, closestPoint);

                // 基于距离计算音量
                if (distance <= minDistance)
                {
                    targetVolume = maxVolume;
                }
                else if (distance >= maxDistance)
                {
                    targetVolume = 0f;
                }
                else
                {
                    float t = 1 - ((distance - minDistance) / (maxDistance - minDistance));
                    targetVolume = maxVolume * t;
                }
            }
        }
        else
        {
            targetVolume = maxVolume;
        }

        // 平滑过渡音量
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}