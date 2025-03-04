using UnityEngine;

public class CubeCollector : MonoBehaviour
{
    public float collectRange = 1.5f;
    [SerializeField] private SnakeBody snakeBody;
    public AudioClip collectSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f; // 添加音量控制滑块
    private AudioSource audioSource;

    void Start()
    {
        // 获取或添加 SnakeBody 组件
        if (snakeBody == null)
        {
            snakeBody = GetComponent<SnakeBody>();
            if (snakeBody == null)
            {
                Debug.LogError("SnakeBody component not found on " + gameObject.name);
                enabled = false;
                return;
            }
        }

        // 添加音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = soundVolume; // 设置初始音量
    }

    void Update()
    {
        // 如果在运行时改变了音量滑块，更新AudioSource的音量
        if (audioSource.volume != soundVolume)
        {
            audioSource.volume = soundVolume;
        }

        // 使用球形检测收集范围内的物体
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectRange);
        foreach (Collider collider in colliders)
        {
            // 调试信息
            Debug.Log($"Found object: {collider.gameObject.name} with tag: {collider.tag}");

            if (collider.CompareTag("Collectable"))
            {
                if (snakeBody != null)
                {
                    Debug.Log("Found collectable, adding body part");
                    snakeBody.AddBodyPart();  // 这里从 AddCube 改为 AddBodyPart
                    Destroy(collider.gameObject);

                    // 播放收集音效
                    if (collectSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(collectSound);
                    }
                }
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        // 在场景视图中显示收集范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}