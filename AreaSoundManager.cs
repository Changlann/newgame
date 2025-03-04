using UnityEngine;

public class AreaSoundManager : MonoBehaviour
{
    [Header("��Ч����")]
    public AudioClip areaSound;        // ������Ч
    public float maxVolume = 1f;       // �������
    public float fadeSpeed = 2f;       // ���뵭���ٶ�
    public bool useDistanceBasedVolume = true; // �Ƿ�ʹ�û��ھ��������

    [Header("������Χ����")]
    public float minDistance = 2f;     // ��С���루�ﵽ���������
    public float maxDistance = 10f;    // �����루����Ϊ0��

    private AudioSource audioSource;
    private bool playerInRange = false;
    private Transform player;
    private float targetVolume = 0f;

    void Start()
    {
        // ȷ������ײ�岢����Ϊ������
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogError("AreaSoundManager��Ҫһ����ײ�������");
            enabled = false;
            return;
        }

        // ������ƵԴ
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = areaSound;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;  // 3D��Ч
        audioSource.volume = 0f;
        audioSource.Play();

        // �������
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("������û���ҵ�Player��ǩ�����壡");
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
            // ������ҵ������ײ�����ľ���
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Vector3 closestPoint = collider.ClosestPoint(player.position);
                float distance = Vector3.Distance(player.position, closestPoint);

                // ���ھ����������
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

        // ƽ����������
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