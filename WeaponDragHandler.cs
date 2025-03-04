using UnityEngine;
using UnityEngine.EventSystems;

// 此组件用于处理武器拖放重新分配
// 需要添加到Canvas对象上
public class WeaponDragHandler : MonoBehaviour
{
    [Header("UI设置")]
    [Tooltip("拖动时显示的图标")]
    public GameObject dragIconPrefab; // 拖动时显示的图标

    [Header("交互设置")]
    [Tooltip("可以接收武器的图层")]
    public LayerMask targetLayers; // 可以接收武器的图层
    [Tooltip("最大拖动距离")]
    public float maxDragDistance = 50f; // 最大拖动距离

    private GameObject dragIconInstance;
    private WeaponData draggedWeaponData;
    private WeaponHolder sourceHolder;
    private Camera mainCamera;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[WeaponDragHandler] 找不到主摄像机!");
            enabled = false;
        }
    }

    void Update()
    {
        // 检测鼠标点击，开始拖动
        if (Input.GetMouseButtonDown(0) && !isDragging && !IsPointerOverUI())
        {
            TryStartDrag();
        }

        // 更新拖动图标位置
        if (isDragging)
        {
            UpdateDragPosition();

            // 检测鼠标释放，结束拖动
            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }
    }

    // 尝试开始拖动
    private void TryStartDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, targetLayers))
        {
            // 检查点击的对象是否有WeaponHolder组件
            WeaponHolder holder = hit.collider.gameObject.GetComponent<WeaponHolder>();
            if (holder != null && holder.HasWeapon())
            {
                StartDrag(holder);
            }
        }
    }

    // 开始拖动武器
    private void StartDrag(WeaponHolder holder)
    {
        sourceHolder = holder;
        draggedWeaponData = holder.GetWeaponData();

        if (draggedWeaponData == null) return;

        // 从源持有者移除武器
        sourceHolder.RemoveWeapon();

        // 创建拖动图标
        CreateDragIcon();

        isDragging = true;
        Debug.Log($"[WeaponDragHandler] 开始拖动武器: {draggedWeaponData.weaponName}");
    }

    // 创建拖动图标
    private void CreateDragIcon()
    {
        if (dragIconPrefab == null || draggedWeaponData == null) return;

        dragIconInstance = Instantiate(dragIconPrefab, transform);

        // 设置图标外观
        UnityEngine.UI.Image iconImage = dragIconInstance.GetComponent<UnityEngine.UI.Image>();
        if (iconImage != null && draggedWeaponData.weaponIcon != null)
        {
            iconImage.sprite = draggedWeaponData.weaponIcon;
        }

        // 初始位置设置为鼠标位置
        UpdateDragPosition();
    }

    // 更新拖动图标位置
    private void UpdateDragPosition()
    {
        if (dragIconInstance == null) return;

        // 将图标位置设置为鼠标位置
        dragIconInstance.transform.position = Input.mousePosition;
    }

    // 结束拖动并放置武器
    private void EndDrag()
    {
        if (!isDragging) return;

        // 查找目标持有者
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, targetLayers))
        {
            WeaponHolder targetHolder = hit.collider.gameObject.GetComponent<WeaponHolder>();
            if (targetHolder != null)
            {
                // 如果目标已有武器，则交换
                if (targetHolder.HasWeapon())
                {
                    WeaponData targetWeapon = targetHolder.RemoveWeapon();
                    targetHolder.EquipWeapon(draggedWeaponData);

                    // 如果源持有者仍然存在，将目标的武器给它
                    if (sourceHolder != null)
                    {
                        sourceHolder.EquipWeapon(targetWeapon);
                    }

                    Debug.Log($"[WeaponDragHandler] 交换了武器 {draggedWeaponData.weaponName} 和 {targetWeapon.weaponName}");
                }
                else
                {
                    // 目标没有武器，直接装备
                    targetHolder.EquipWeapon(draggedWeaponData);
                    Debug.Log($"[WeaponDragHandler] 将武器 {draggedWeaponData.weaponName} 放置到新持有者");
                }
            }
            else
            {
                // 没有找到目标持有者，将武器返回源持有者
                ReturnWeaponToSource();
            }
        }
        else
        {
            // 射线没有击中任何目标，将武器返回源持有者
            ReturnWeaponToSource();
        }

        // 清理拖动状态
        CleanupDrag();
    }

    // 将武器返回源持有者
    private void ReturnWeaponToSource()
    {
        if (sourceHolder != null && draggedWeaponData != null)
        {
            sourceHolder.EquipWeapon(draggedWeaponData);
            Debug.Log($"[WeaponDragHandler] 武器 {draggedWeaponData.weaponName} 返回原持有者");
        }
    }

    // 清理拖动状态
    private void CleanupDrag()
    {
        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance);
        }

        draggedWeaponData = null;
        sourceHolder = null;
        isDragging = false;
    }

    // 检查指针是否在UI元素上
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}