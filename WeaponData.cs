using UnityEngine;

// ʹ��ScriptableObjectʹ���ǿ����ڱ༭���д���������������
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("������Ϣ")]
    [Tooltip("�������� - ������ʾ��ʶ��")]
    public string weaponName = "Ĭ������";

    [Tooltip("����ģ��Ԥ���� - ����ʾ��ʿ������")]
    public GameObject weaponPrefab;

    [Tooltip("�ӵ�Ԥ���� - �������ʱ������Ͷ����")]
    public GameObject projectilePrefab;

    [Header("����ͼ��")]
    [Tooltip("����ͼ�� - ����UI��ʾ����קʱ��ʾ")]
    public Sprite weaponIcon;

    [Header("���ܲ���")]
    [Tooltip("������� - ÿ����Է�����ӵ�����")]
    public float fireRate = 1f;

    [Tooltip("�˺�ֵ - ÿ���ӵ���ɵ��˺�")]
    public int damage = 10;

    [Tooltip("��� - �����ܹ���⵽���˵�������")]
    public float range = 15f;

    [Tooltip("��������ӵ��� - һ�ο�������ӵ�����")]
    public int bulletsPerShot = 1;

    [Tooltip("�ӵ��ٶ� - �ӵ����е��ٶ�")]
    public float bulletSpeed = 20f;

    [Header("��������")]
    [Tooltip("ɢ��Ƕ� - �ӵ�ɢ���ĽǶȷ�Χ")]
    public float spreadAngle = 0f;

    [Tooltip("׷��Ч�� - �ӵ��Ƿ���Զ�׷�ٵ���")]
    public bool isHoming = false;

    [Tooltip("׷��ǿ�� - �ӵ�׷�ٵ��˵�������")]
    public float homingStrength = 0f;

    [Header("����Ч��")]
    [Tooltip("��͸Ч�� - �ӵ��Ƿ��ܴ�͸����")]
    public bool hasPiercing = false;

    [Tooltip("����Ч�� - �ӵ��Ƿ��ܵ���")]
    public bool hasBounce = false;

    [Tooltip("������� - �ӵ�����ܵ���Ĵ���")]
    public int bounceCount = 0;

    [Header("��Ч")]
    [Tooltip("�����Ч - ��������ʱ���ŵ�����")]
    public AudioClip shootSound;

    [Tooltip("������� - �����Ч��������С")]
    [Range(0f, 1f)]
    public float shootVolume = 0.5f;

    [Header("�Ӿ�Ч��")]
    [Tooltip("�ӵ���ɫ - �ӵ�����Ҫ��ɫ")]
    public Color bulletColor = Color.yellow;

    [Tooltip("ǹ������Ч�� - �������ʱ��ǹ����ʾ����Ч")]
    public GameObject muzzleFlashPrefab;

    [Tooltip("����Ч�� - �ӵ�����Ŀ��ʱ��ʾ����Ч")]
    public GameObject impactEffectPrefab;

    // �ṩ��¡�����Ա�������ͬһScriptableObject
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