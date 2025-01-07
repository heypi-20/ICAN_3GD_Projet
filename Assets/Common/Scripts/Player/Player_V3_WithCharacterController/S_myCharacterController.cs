using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_myCharacterController : MonoBehaviour
{
    public float speed = 12f;
    [Range(0f, 1f)]
    public float friction = 0.5f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.4f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 inputDirection;
    public Vector3 velocity;

    [SerializeField]
    private float currentSpeed;  // 实时速度监控

    private void Start()
    {
        initialized();
    }

    private void Update()
    {
        playerMovement();

        if (GroundCheck() && velocity.y < 0)
        {
            velocity.y = -speed;
        }
        else if (!GroundCheck())
        {
            addGravity();
        }
    }

    private void initialized()
    {
        controller = GetComponent<CharacterController>();
    }

    private void playerMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 获取玩家输入方向
        inputDirection = transform.right * x + transform.forward * z;

        // 归一化输入方向
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // 计算并更新实时速度
        currentSpeed = (inputDirection * speed).magnitude;

        // 移动角色
        controller.Move(speedResult(inputDirection));
    }

    private Vector3 speedResult(Vector3 direction)
    {
        return direction * speed * Time.deltaTime;
    }

    private void applyFriction()
    {
        // TODO: 实现摩擦力逻辑
    }

    private void addGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private bool GroundCheck()
    {
        return Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        // 可视化地面检测
        Gizmos.color = GroundCheck() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
