using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsBasedPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float maxVelocity = 8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.2f;
    [SerializeField] private int maxAirJumps = 0;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float slopeLimit = 45f;
    
    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool canJump = true;
    private int jumpCount;
    private float lastJumpTime;
    private RaycastHit slopeHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        // Configure rigidbody constraints
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        GroundCheck();
    }

    void FixedUpdate()
    {
        Move();
    }

    private bool IsOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundCheckRadius + 0.1f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < slopeLimit && angle != 0;
        }
        return false;
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
            canJump = true;
        }
    }

    public void HandleMovementInput(Vector2 input)
    {
      //  Debug.Log(input);
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        float controlMultiplier = isGrounded ? 1f : airControl;
        moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized * controlMultiplier;
    }

    public void HandleJumpInput()
    {
        if (Time.time - lastJumpTime < jumpCooldown) return;
        
        if (isGrounded || jumpCount < maxAirJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
            lastJumpTime = Time.time;
            canJump = false;
        }
    }

    private void Move()
    {
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        if (IsOnSlope())
        {
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, slopeHit.normal);
        }

        targetVelocity.y = rb.linearVelocity.y;

        float currentAccel = moveDirection.magnitude > 0 ? acceleration : deceleration;
        
        Vector3 velocityDiff = targetVelocity - rb.linearVelocity;
        velocityDiff.y = 0f;
        
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        if (horizontalVelocity.magnitude > maxVelocity)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxVelocity;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            return;
        }

        rb.AddForce(velocityDiff * currentAccel, ForceMode.Acceleration);
      //  Debug.Log(rb.linearVelocity);

        // Modified rotation handling to only affect Y rotation
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            float currentYRotation = transform.eulerAngles.y;
            float targetYRotation = targetRotation.eulerAngles.y;
            
            transform.rotation = Quaternion.Euler(
                0f, 
                Mathf.LerpAngle(currentYRotation, targetYRotation, Time.fixedDeltaTime * 10f),
                0f
            );
        }
        
        moveDirection = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
