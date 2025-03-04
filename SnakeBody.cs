using System.Collections.Generic;
using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    [Header("Ԥ��������")]
    [Tooltip("����Ԥ����")]
    public GameObject bodyPartPrefab;
    [Tooltip("�ӵ�Ԥ����")]
    public GameObject bulletPrefab;

    [Header("�ƶ�����")]
    [Tooltip("���岿��֮��ľ���")]
    public float spacing = 1f;
    [Tooltip("�ƶ�ƽ����")]
    public float moveSpeed = 8f;
    [Tooltip("��תƽ����")]
    public float rotateSpeed = 8f;

    [Header("����ֵ����")]
    [Tooltip("���岿�ֵĳ�ʼ����ֵ")]
    public float bodyPartHealth = 100f;
    [Tooltip("���������岿�ֵ��޵�ʱ��")]
    public float invincibleDuration = 2f;

    // �洢�������岿��
    private List<BodyPart> bodyParts = new List<BodyPart>();

    // �����洢���岿����Ϣ����
    [System.Serializable]
    private class BodyPart
    {
        public GameObject gameObject;
        public Transform transform;
        public SoldierShooter shooter;
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public float invincibleTime;  // �������޵�ʱ��

        public BodyPart(GameObject go)
        {
            gameObject = go;
            transform = go.transform;
            shooter = go.GetComponent<SoldierShooter>();
            targetPosition = transform.position;
            targetRotation = transform.rotation;
            invincibleTime = Time.time + 2f;  // Ĭ��2���޵�ʱ��
        }

        public bool IsValid()
        {
            return gameObject != null && transform != null;
        }
    }

    private void Start()
    {
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (bodyPartPrefab == null)
        {
            Debug.LogError("[SnakeBody] Body Part Prefab δ���ã����� Inspector �����ã�", this);
            enabled = false;
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("[SnakeBody] Bullet Prefab δ���ã����� Inspector �����ã�", this);
            enabled = false;
            return;
        }
    }

    public void AddBodyPart()
    {
        // �����Ѿ���Ч�����岿��
        bodyParts.RemoveAll(part => part == null || !part.IsValid());

        Vector3 newPosition;
        Quaternion newRotation;

        if (bodyParts.Count == 0)
        {
            // ��һ�����岿�ָ���ͷ��
            newPosition = transform.position - transform.forward * spacing;
            newRotation = transform.rotation;
        }
        else
        {
            // ��ȡ���һ����Ч�����岿��
            BodyPart lastPart = bodyParts[bodyParts.Count - 1];
            if (!lastPart.IsValid())
            {
                Debug.LogError("[SnakeBody] ���һ�����岿����Ч��");
                return;
            }
            newPosition = lastPart.transform.position - lastPart.transform.forward * spacing;
            newRotation = lastPart.transform.rotation;
        }

        try
        {
            GameObject newPart = Instantiate(bodyPartPrefab, newPosition, newRotation);

            // ���ñ�ǩ�Ͳ㼶
            newPart.tag = "SnakeBody";
            newPart.layer = LayerMask.NameToLayer("SnakeBody");

            // ȷ������ײ��
            SphereCollider sphereCollider = newPart.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = newPart.AddComponent<SphereCollider>();
                sphereCollider.radius = 0.5f;
            }
            sphereCollider.isTrigger = true;

            // ���ø���
            Rigidbody rb = newPart.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newPart.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;

            // ����Health���
            Health health = newPart.GetComponent<Health>();
            if (health == null)
            {
                health = newPart.AddComponent<Health>();
            }
            health.maxHealth = bodyPartHealth;
            health.currentHealth = bodyPartHealth;
            health.SetInvincible(invincibleDuration);  // ���ó�ʼ�޵�ʱ��

            // ��ӱ����������
            PassiveHealing passiveHealing = newPart.AddComponent<PassiveHealing>();
            if (passiveHealing != null)
            {
                passiveHealing.healthThreshold = 0.3f; // 30%����ֵʱ����
                passiveHealing.cooldown = 30f; // 30����ȴ
                passiveHealing.maxHealthIncreasePercent = 0.1f; // ����10%�������ֵ
            }

            // ����������������
            WeaponHolder weaponHolder = newPart.AddComponent<WeaponHolder>();
            if (weaponHolder != null)
            {
                // �����������ص�
                GameObject socketObj = new GameObject("WeaponSocket");
                socketObj.transform.SetParent(newPart.transform);
                socketObj.transform.localPosition = new Vector3(0, 0.5f, 0); // ����λ��
                socketObj.transform.localRotation = Quaternion.identity;

                weaponHolder.weaponSocket = socketObj.transform;
            }

            // ����������
            SoldierShooter shooter = newPart.GetComponent<SoldierShooter>();
            if (shooter == null)
            {
                shooter = newPart.AddComponent<SoldierShooter>();
            }
            shooter.SetBulletPrefab(this.bulletPrefab);
            shooter.fireRate = 1f;
            shooter.detectionRange = 15f;
            shooter.bulletDamage = 15;
            shooter.enabled = true;

            // ��ӵ��б�
            bodyParts.Add(new BodyPart(newPart));
            Debug.Log($"[SnakeBody] �ɹ�����µ����岿�� - ��ǰ����: {bodyParts.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SnakeBody] ������岿��ʱ����{e.Message}");
        }
    }

    private void Update()
    {
        UpdateBodyParts();
    }

    private void UpdateBodyParts()
    {
        // ������Ч�����岿��
        bodyParts.RemoveAll(part => part == null || !part.IsValid());

        for (int i = 0; i < bodyParts.Count; i++)
        {
            BodyPart part = bodyParts[i];
            if (!part.IsValid()) continue;

            Transform target = (i == 0) ? transform : bodyParts[i - 1].transform;
            if (target == null) continue;

            // ����Ŀ��λ�ú���ת
            part.targetPosition = target.position - target.forward * spacing;
            part.targetRotation = target.rotation;

            // ƽ���ƶ���Ŀ��λ��
            part.transform.position = Vector3.Lerp(
                part.transform.position,
                part.targetPosition,
                moveSpeed * Time.deltaTime
            );

            // ƽ����ת��Ŀ�곯��
            part.transform.rotation = Quaternion.Slerp(
                part.transform.rotation,
                part.targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    // �ṩһ��������ȡ�������岿�ֵ�Transform����WeaponHolderʹ��
    public Transform[] GetBodyPartTransforms()
    {
        Transform[] transforms = new Transform[bodyParts.Count];
        for (int i = 0; i < bodyParts.Count; i++)
        {
            transforms[i] = bodyParts[i].transform;
        }
        return transforms;
    }

    private void OnDrawGizmos()
    {
        // ������Ч�����岿��
        if (Application.isPlaying)
        {
            bodyParts.RemoveAll(part => part == null || !part.IsValid());
        }

        Gizmos.color = Color.green;

        // ����ͷ������һ�����岿�ֵ�����
        if (bodyParts.Count > 0 && bodyParts[0] != null && bodyParts[0].IsValid())
        {
            Gizmos.DrawLine(transform.position, bodyParts[0].transform.position);
        }

        // �������岿��֮�������
        for (int i = 1; i < bodyParts.Count; i++)
        {
            if (bodyParts[i] != null && bodyParts[i].IsValid() &&
                bodyParts[i - 1] != null && bodyParts[i - 1].IsValid())
            {
                Gizmos.DrawLine(
                    bodyParts[i].transform.position,
                    bodyParts[i - 1].transform.position
                );
            }
        }
    }
}