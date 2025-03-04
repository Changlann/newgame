using UnityEngine;

public class CubeCollector : MonoBehaviour
{
    public float collectRange = 1.5f;
    [SerializeField] private SnakeBody snakeBody;
    public AudioClip collectSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f; // ����������ƻ���
    private AudioSource audioSource;

    void Start()
    {
        // ��ȡ����� SnakeBody ���
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

        // �����Ƶ���
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = soundVolume; // ���ó�ʼ����
    }

    void Update()
    {
        // ���������ʱ�ı����������飬����AudioSource������
        if (audioSource.volume != soundVolume)
        {
            audioSource.volume = soundVolume;
        }

        // ʹ�����μ���ռ���Χ�ڵ�����
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectRange);
        foreach (Collider collider in colliders)
        {
            // ������Ϣ
            Debug.Log($"Found object: {collider.gameObject.name} with tag: {collider.tag}");

            if (collider.CompareTag("Collectable"))
            {
                if (snakeBody != null)
                {
                    Debug.Log("Found collectable, adding body part");
                    snakeBody.AddBodyPart();  // ����� AddCube ��Ϊ AddBodyPart
                    Destroy(collider.gameObject);

                    // �����ռ���Ч
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
        // �ڳ�����ͼ����ʾ�ռ���Χ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}