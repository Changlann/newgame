using UnityEngine;

public class HealerMovement : MonoBehaviour
{
    [Header("基础参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float rotationSpeed = 3f;

    [Header("地面检测")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("加速参数")]
    public float sprintMultiplier = 1.4f;
    public float maxSprintDuration = 2f;
    public float sprintCooldown = 5f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private bool isJumping;
    private Transform cameraTransform;

    // 加速系统变量
    private float currentSpeed;
    private float sprintTimeUsed;
    private float cooldownTimer;
    private bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        HandleSprint();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);

        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
            isJumping = false;
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 基于摄像机方向的移动
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = vertical * cameraForward + horizontal * cameraTransform.right;

        if (moveDirection.magnitude >= 0.1f)
        {
            // 仅旋转角色面向移动方向
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
        {
            verticalVelocity.y = jumpForce;
            isJumping = true;
        }

        verticalVelocity.y -= gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void HandleSprint()
    {
        // 冷却计时
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // 按下Shift立即加速
        if (Input.GetKeyDown(KeyCode.LeftShift) && sprintTimeUsed < maxSprintDuration)
        {
            isSprinting = true;
            currentSpeed = moveSpeed * sprintMultiplier;
        }

        // 松开Shift或达到最大持续时间
        if (Input.GetKeyUp(KeyCode.LeftShift) || sprintTimeUsed >= maxSprintDuration)
        {
            EndSprint();
        }

        // 持续加速计时
        if (isSprinting)
        {
            sprintTimeUsed += Time.deltaTime;
            if (sprintTimeUsed >= maxSprintDuration)
            {
                EndSprint();
            }
        }
    }

    void EndSprint()
    {
        isSprinting = false;
        currentSpeed = moveSpeed;
        cooldownTimer = sprintCooldown;
        sprintTimeUsed = 0f;
    }

    // 新添加的方法，用于检查是否在冲刺状态
    public bool IsSprinting()
    {
        return isSprinting;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
    }
}