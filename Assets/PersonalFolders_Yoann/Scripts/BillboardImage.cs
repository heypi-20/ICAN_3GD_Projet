using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BillboardImage : MonoBehaviour
{
    public Sprite image;
    public Color tintColor = Color.white;
    public Transform player;

    [Header("Réglage de la taille")]
    [Range(0.1f, 3f)]
    public float scale = 1f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (image != null)
            spriteRenderer.sprite = image;

        spriteRenderer.color = tintColor;

        if (player == null && GameObject.FindWithTag("Player") != null)
            player = GameObject.FindWithTag("Player").transform;

        ApplyScale();
    }

    void Update()
    {
        RotateOnlyOnY();
        ApplyScale();
    }

    void RotateOnlyOnY()
    {
        if (player == null) return;

        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0f; // Ignore Y for horizontal rotation
        if (lookPos != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookPos);
    }

    void ApplyScale()
    {
        if (spriteRenderer.sprite == null) return;

        float spriteRatio = spriteRenderer.sprite.bounds.size.x / spriteRenderer.sprite.bounds.size.y;

        // On garde la plus petite dimension à scale, l’autre est ajustée pour respecter le ratio
        float x = scale;
        float y = scale;

        if (spriteRatio > 1f)
        {
            y = scale / spriteRatio;
        }
        else
        {
            x = scale * spriteRatio;
        }

        transform.localScale = new Vector3(x, y, 1f);
    }
}