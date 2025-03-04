using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    public float speed = 20f;
    public float lifeTime = 3f;
    private bool hasHit = false;

    void Start()
    {
        // ���û�������˺�ֵ��ʹ��Ĭ��ֵ
        if (damage == 0)
        {
            damage = 15;
        }
        Destroy(gameObject, lifeTime);
        Debug.Log($"�ӵ����ɣ��˺�ֵ��{damage}");
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
        Debug.Log($"�ӵ��˺�ֵ����Ϊ��{damage}");
    }

    void Update()
    {
        if (!hasHit)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            hasHit = true;
            Debug.Log($"�ӵ����е��ˣ�׼����� {damage} ���˺�");

            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, true);  // ���� true ��ʾ�˺�����
                Debug.Log($"�ɹ��Ե������ {damage} ���˺�");
            }
            else
            {
                Debug.LogError($"���еĵ���û�� Health ���");
            }

            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet") && !other.CompareTag("SnakeBody"))
        {
            hasHit = true;
            Debug.Log($"�ӵ������������壺{other.tag}");
            Destroy(gameObject);
        }
    }
}