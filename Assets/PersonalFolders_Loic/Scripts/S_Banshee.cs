using UnityEngine;

public class S_Banshee : EnemyBase
{
    [Header("Enemy Properties")]
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float avoidDist = 10f;
    public LayerMask obstacleMask;

    [Header("Attack Properties")]
    public float range = 50f;
    
    private Transform player;

    private Rigidbody rb;
    
    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>().transform;
        if (player == null) {
            Debug.LogWarning("No player found");
        }
        
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist < range) {
            Attack();
        } else {
            Chase();
        }
    }

    private void Chase()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        
        if (Physics.SphereCast(transform.position, transform.localScale.y/2f, transform.forward, out RaycastHit hit, avoidDist, obstacleMask)) {
            Vector3 avoidDir = Vector3.ProjectOnPlane(toPlayer, hit.normal).normalized;
            
            rb.velocity = avoidDir * speed;
            Debug.Log("Avoiding");
        } else {
            rb.velocity = toPlayer * speed;
            Debug.Log("Chasing");
        }

        if (transform.position.y < transform.localScale.y / 2) {
            transform.position = new Vector3(transform.position.x, transform.localScale.y / 2, transform.position.z);
        }
    }

    private void Attack()
    {
        
    }
}