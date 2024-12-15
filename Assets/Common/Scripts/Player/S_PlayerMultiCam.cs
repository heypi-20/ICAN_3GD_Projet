using UnityEngine;

[RequireComponent(typeof(S_GroundCheck))]
public class S_PlayerMultiCam : MonoBehaviour
{
    public enum CameraType
    {
        FPS,
        TPS,
        TOPDOWN,
    }
    
    [Header("Camera Settings")]
    public CameraType cameraType;
    public GameObject[] cams;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float groundDrag = 10f;
    public float airMultiplier = 0.08f;
    public float maxSlopeAngle = 40f;
    public float extraGravity = 1.5f;
    
    private S_GroundCheck groundCheck;
    private Rigidbody rb;
    
    // Player input axes
    private float h;
    private float v;
    
    private Vector3 moveDirection;
    private bool isJumping = false;
    private int currentJumps = 0;
    private RaycastHit slopeHit;

    // Start is called before the first frame update
    void Start()
    {
        groundCheck = GetComponent<S_GroundCheck>();
        rb = GetComponent<Rigidbody>();

        ActivateCamera();
        UpdateCursorState();
    }
    
    void UpdateCursorState()
    {
        if (cameraType == CameraType.FPS)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInputs();
        SpeedControl();
        Rotation();
        
        if (groundCheck.IsGrounded)
        {
            rb.drag = groundDrag;
            currentJumps = 0;
        } else
        {
            rb.drag = 0;
        }
    }

    private void PlayerInputs()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Movement();
        ApplyExtraGravity();
    }

    private void Movement()
    {
        if (cameraType == CameraType.FPS) {
            if (Camera.main != null) {
                moveDirection = Camera.main.transform.forward * v + Camera.main.transform.right * h;
                moveDirection.y = 0f;
                moveDirection.Normalize();
            }
        } else {
            moveDirection = transform.forward * v + transform.right * h;
            moveDirection.y = 0f;
            moveDirection.Normalize();
        }

        if (OnSlope() && groundCheck.IsGrounded) {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 10f);
        } else if (groundCheck.IsGrounded) {
            rb.AddForce(moveDirection * moveSpeed * 10f);
        } else {
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier);
        }

        rb.useGravity = !OnSlope();
    }
        
    private void SpeedControl()
    {
        if (OnSlope())
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        } else {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
    
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    
    private void ApplyExtraGravity()
    {
        if (!groundCheck.IsGrounded && rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.down * extraGravity * 10f, ForceMode.Acceleration);
        }
    }

    private void Rotation()
    {
        switch(cameraType) {
            case CameraType.FPS:
                transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles.y, 0);
                break;
            case CameraType.TPS:
                if (moveDirection != Vector3.zero) {
                    Quaternion lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
                }
                break;
        }
    }

    private void ActivateCamera()
    {
        switch(cameraType) {
            case CameraType.FPS:
                SetCameraState((int)CameraType.FPS);
                break;
            case CameraType.TPS:
                SetCameraState((int)CameraType.TPS);
                break;
        }
    }

    private void SetCameraState(int camIndex)
    {
        cams[camIndex].SetActive(true);

        for (int i = 0; i < cams.Length; i++) {
            if (i == camIndex) {
                continue;
            }
            cams[i].SetActive(false);
        }
    }
}
