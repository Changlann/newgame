using UnityEngine;

// 使用ScriptableObject使我们可以在编辑器中创建多种武器配置
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("武器名称 - 用于显示和识别")]
    public string weaponName = "默认武器";

    [Tooltip("武器模型预制体 - 将显示在士兵身上")]
    public GameObject weaponPrefab;

    [Tooltip("子弹预制体 - 武器射击时产生的投射物")]
    public GameObject projectilePrefab;

    [Header("武器图标")]
    [Tooltip("武器图标 - 用于UI显示和拖拽时显示")]
    public Sprite weaponIcon;

    [Header("性能参数")]
    [Tooltip("射击速率 - 每秒可以发射的子弹数量")]
    public float fireRate = 1f;

    [Tooltip("伤害值 - 每发子弹造成的伤害")]
    public int damage = 10;

    [Tooltip("射程 - 武器能够检测到敌人的最大距离")]
    public float range = 15f;

    [Tooltip("单次射击子弹数 - 一次开火发射的子弹数量")]
    public int bulletsPerShot = 1;

    [Tooltip("子弹速度 - 子弹飞行的速度")]
    public float bulletSpeed = 20f;

    [Header("弹道设置")]
    [Tooltip("散射角度 - 子弹散布的角度范围")]
    public float spreadAngle = 0f;

    [Tooltip("追踪效果 - 子弹是否会自动追踪敌人")]
    public bool isHoming = false;

    [Tooltip("追踪强度 - 子弹追踪敌人的灵敏度")]
    public float homingStrength = 0f;

    [Header("特殊效果")]
    [Tooltip("穿透效果 - 子弹是否能穿透敌人")]
    public bool hasPiercing = false;

    [Tooltip("弹射效果 - 子弹是否能弹射")]
    public bool hasBounce = false;

    [Tooltip("弹射次数 - 子弹最多能弹射的次数")]
    public int bounceCount = 0;

    [Header("音效")]
    [Tooltip("射击音效 - 武器开火时播放的声音")]
    public AudioClip shootSound;

    [Tooltip("射击音量 - 射击音效的音量大小")]
    [Range(0f, 1f)]
    public float shootVolume = 0.5f;

    [Header("视觉效果")]
    [Tooltip("子弹颜色 - 子弹的主要颜色")]
    public Color bulletColor = Color.yellow;

    [Tooltip("枪口闪光效果 - 武器射击时在枪口显示的特效")]
    public GameObject muzzleFlashPrefab;

    [Tooltip("击中效果 - 子弹击中目标时显示的特效")]
    public GameObject impactEffectPrefab;

    // 提供克隆方法以避免引用同一ScriptableObject
    public WeaponData Clone()
    {
        WeaponData clone = CreateInstance<WeaponData>();
        clone.weaponName = this.weaponName;
        clone.weaponPrefab = this.weaponPrefab;
        clone.projectilePrefab = this.projectilePrefab;
        clone.weaponIcon = this.weaponIcon;
        clone.fireRate = this.fireRate;
        clone.damage = this.damage;
        clone.range = this.range;
        clone.bulletsPerShot = this.bulletsPerShot;
        clone.bulletSpeed = this.bulletSpeed;
        clone.spreadAngle = this.spreadAngle;
        clone.isHoming = this.isHoming;
        clone.homingStrength = this.homingStrength;
        clone.hasPiercing = this.hasPiercing;
        clone.hasBounce = this.hasBounce;
        clone.bounceCount = this.bounceCount;
        clone.shootSound = this.shootSound;
        clone.shootVolume = this.shootVolume;
        clone.bulletColor = this.bulletColor;
        clone.muzzleFlashPrefab = this.muzzleFlashPrefab;
        clone.impactEffectPrefab = this.impactEffectPrefab;
        return clone;
    }
}