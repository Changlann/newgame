using UnityEngine;

public class HealerMovement : MonoBehaviour
{
    [Header("��������")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float rotationSpeed = 3f;

    [Header("������")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("���ٲ���")]
    public float sprintMultiplier = 1.4f;
    public float maxSprintDuration = 2f;
    public float sprintCooldown = 5f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private bool isJumping;
    private Transform cameraTransform;

    // ����ϵͳ����
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

        // ���������������ƶ�
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = vertical * cameraForward + horizontal * cameraTransform.right;

        if (moveDirection.magnitude >= 0.1f)
        {
            // ����ת��ɫ�����ƶ�����
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
        // ��ȴ��ʱ
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // ����Shift��������
        if (Input.GetKeyDown(KeyCode.LeftShift) && sprintTimeUsed < maxSprintDuration)
        {
            isSprinting = true;
            currentSpeed = moveSpeed * sprintMultiplier;
        }

        // �ɿ�Shift��ﵽ������ʱ��
        if (Input.GetKeyUp(KeyCode.LeftShift) || sprintTimeUsed >= maxSprintDuration)
        {
            EndSprint();
        }

        // �������ټ�ʱ
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

    // ����ӵķ��������ڼ���Ƿ��ڳ��״̬
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