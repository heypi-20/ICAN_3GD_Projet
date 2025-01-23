using UnityEngine;

public class S_ToggleWall : MonoBehaviour
{
    [Tooltip("Number of round to toggle the wall")]
    public int roundToToggle = 1;

    [Tooltip("If the body is actif on start of the game")]
    public bool wallStateOnStart = true;
    
    [Header("Read-only properties")]
    [SerializeField] private bool toggle; // Wall body active state
    
    private int currentRoundToToggle = 0; // Current round until toggle
    private GameObject wallObject; // Get body's gameObject
    
    // Start is called before the first frame update
    void Start()
    {
        wallObject = transform.GetChild(0).gameObject;
        wallObject.SetActive(wallStateOnStart);
        toggle = wallStateOnStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            currentRoundToToggle++;
        }

        if (currentRoundToToggle == roundToToggle) {
            toggle = !toggle;
            wallObject.gameObject.SetActive(toggle);
            currentRoundToToggle = 0;
        }
    }
}
