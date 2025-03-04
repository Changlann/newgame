using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    public float speed = 20f;
    public float lifeTime = 3f;
    private bool hasHit = false;

    void Start()
    {
        // 如果没有设置伤害值，使用默认值
        if (damage == 0)
        {
            damage = 15;
        }
        Destroy(gameObject, lifeTime);
        Debug.Log($"子弹生成，伤害值：{damage}");
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
        Debug.Log($"子弹伤害值设置为：{damage}");
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
            Debug.Log($"子弹击中敌人，准备造成 {damage} 点伤害");

            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, true);  // 传入 true 显示伤害数字
                Debug.Log($"成功对敌人造成 {damage} 点伤害");
            }
            else
            {
                Debug.LogError($"击中的敌人没有 Health 组件");
            }

            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet") && !other.CompareTag("SnakeBody"))
        {
            hasHit = true;
            Debug.Log($"子弹击中其他物体：{other.tag}");
            Destroy(gameObject);
        }
    }
}