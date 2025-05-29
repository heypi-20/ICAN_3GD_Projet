using UnityEngine;

using UnityEngine;
using UnityEngine.UI;

public class MainMenuIdleManager : MonoBehaviour
{
    [Header("Game Object")]
    public GameObject[] menuElements; 
    public RectTransform titleTransform; // Transforme du logo (Image)
    
    [Header("Anchors")]
    public Vector2 originalTitlePosition;
    private Vector2 centerPosition = Vector2.zero;
    
    [Header("Idle Values")]
    public float idleTimeThreshold = 5f;
    private float idleTimer = 0f;
    private bool isIdle = false;
    
    [Header("Changement Dynamique Logo")]
    public float moveSpeed = 3f;
    public Vector3 idleScale = new Vector3(1.2f, 1.2f, 1f); // Échelle en idle
    private Vector3 originalScale;

    void Start()
    {
        Time.timeScale = 1f;
        if (titleTransform != null)
            originalTitlePosition = titleTransform.anchoredPosition;
            originalScale = titleTransform.localScale;
    }

    void Update()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        bool mouseMoved = mouseDelta.sqrMagnitude > 0.01f;

        bool hasInput = Input.anyKeyDown 
                        || Input.GetMouseButtonDown(0) 
                        || Input.GetMouseButtonDown(1) 
                        || Input.GetMouseButtonDown(2)
                        || mouseMoved;

        if (hasInput)
        {
            idleTimer = 0f;
            if (isIdle)
                ShowMenu();
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (!isIdle && idleTimer >= idleTimeThreshold)
            {
                HideMenu();
            }
        }

        // Déplacement fluide du logo
        if (titleTransform != null)
        {
            Vector2 targetPosition = isIdle ? centerPosition : originalTitlePosition;
            titleTransform.anchoredPosition = Vector2.Lerp(titleTransform.anchoredPosition, targetPosition, Time.deltaTime * moveSpeed);
            
            Vector3 targetScale = isIdle ? idleScale : originalScale;
            titleTransform.localScale = Vector3.Lerp(titleTransform.localScale, targetScale, Time.deltaTime * moveSpeed);
        }
    }

    void HideMenu()
    {
        isIdle = true;
        foreach (GameObject obj in menuElements)
        {
            obj.SetActive(false);
        }
        Cursor.visible = false;
    }

    void ShowMenu()
    {
        isIdle = false;
        foreach (GameObject obj in menuElements)
        {
            obj.SetActive(true);
        }
        Cursor.visible = true;
    }
}
