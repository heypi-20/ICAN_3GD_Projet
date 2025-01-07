using UnityEngine;

[RequireComponent(typeof(S_GroundCheck))]
[RequireComponent(typeof(S_EnergyStorage))]
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

    [Header("Speed and Movement Settings")]
    public float baseSpeed = 5f;
    public float groundDrag = 10f;
    public float airMultiplier = 0.08f;
    public float maxSlopeAngle = 40f;
    public float extraGravity = 1.5f;

    [Header("Energy Related Settings")]
    public bool enableEnergyBoost = true; // Toggle for speed boost from energy
    public float energyPercentageIncrease = 0.01f; // Speed boost per unit of energy
    public float multiplier = 1f; // Multiplier for energy boost
    [Space (20)]
    public bool enableEnergyConsumptionForMovement = true; // Toggle for energy consumption
    public float energyConsumptionRate = 1f; // Base energy consumed per second
    public bool enableEnergyBonusConsumption = true; // Toggle for additional energy consumption based on percentage
    public float energyConsumptionBonusPercentage = 0.01f; // Additional consumption based on current energy percentage
    
    
    [HideInInspector]
    public float moveSpeed = 0f;

    private S_GroundCheck groundCheck;
    private S_PlayerSpeedController playerSpeedController;
    private S_EnergyStorage energyStorage;
    private Rigidbody rb;

    // Player input axes
    private float h;
    private float v;

    private Vector3 moveDirection;
    private bool isMoving = false;
    private RaycastHit slopeHit;

    // Start is called before the first frame update
    void Start()
    {
        groundCheck = GetComponent<S_GroundCheck>();
        playerSpeedController = GetComponent<S_PlayerSpeedController>();
        energyStorage = GetComponent<S_EnergyStorage>();
        rb = GetComponent<Rigidbody>();

        if (energyStorage == null)
        {
            Debug.LogError("S_EnergyStorage component is not found on this GameObject.");
        }

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
        if (IsInputPressed())
        {
            if (enableEnergyConsumptionForMovement)
            {
                ConsumeEnergy();
            }
            PlayerInputs();
            playerSpeedController.SpeedControl();
            Rotation();
        }
        if (enableEnergyConsumptionForMovement && energyStorage.currentEnergy <= 0)
        {
            moveSpeed = 0f;
        }
        if (!IsInputPressed())
        {
            moveSpeed = 0f; // Stop moving if no energy
        }

        if (groundCheck.IsGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    

    private void PlayerInputs()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        isMoving = h != 0 || v != 0;
    }

    private bool IsInputPressed()
    {
        return Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    }

    private void ConsumeEnergy()
    {
        if (isMoving)
        {
            float energyBonusConsumption = energyStorage.currentEnergy * energyConsumptionBonusPercentage;
            float totalEnergyToConsume = (energyConsumptionRate + energyBonusConsumption) * Time.deltaTime;
            energyStorage.currentEnergy = Mathf.Max(0, energyStorage.currentEnergy - totalEnergyToConsume);
        }
    }

    private void FixedUpdate()
    {
        Movement();
        ApplyExtraGravity();
    }

    private void Movement()
    {
        if (cameraType == CameraType.FPS)
        {
            if (Camera.main != null)
            {
                moveDirection = Camera.main.transform.forward * v + Camera.main.transform.right * h;
                moveDirection.y = 0f;
                moveDirection.Normalize();
            }
        }
        else
        {
            moveDirection = transform.forward * v + transform.right * h;
            moveDirection.y = 0f;
            moveDirection.Normalize();
        }

        if (OnSlope() && groundCheck.IsGrounded)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 10f);
        }
        else if (groundCheck.IsGrounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f);
        }
        else
        {
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier);
        }

        rb.useGravity = !OnSlope();
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
        switch (cameraType)
        {
            case CameraType.FPS:
                transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles.y, 0);
                break;
            case CameraType.TPS:
                if (moveDirection != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
                }
                break;
        }
    }

    private void ActivateCamera()
    {
        switch (cameraType)
        {
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

        for (int i = 0; i < cams.Length; i++)
        {
            if (i == camIndex)
            {
                continue;
            }
            cams[i].SetActive(false);
        }
    }
}
