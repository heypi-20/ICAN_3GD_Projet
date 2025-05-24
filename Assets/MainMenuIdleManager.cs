using UnityEngine;

public class MainMenuIdleManager : MonoBehaviour
{
    [Header("Game Object")]
    public GameObject[] menuElements; 
    public RectTransform titleTransform;
    public TMPro.TMP_Text titleText;
    
    [Header("Anchors")]
    public Vector2 originalTitlePosition;
    private Vector2 centerPosition = Vector2.zero;
    
    [Header("Idle Values")]
    public float idleTimeThreshold = 5f;
    private float idleTimer = 0f;
    private bool isIdle = false;
    
    [Header("Changement Dynamique Titre")]
    public float moveSpeed = 3f; // Vitesse de déplacement du titre
    public float idleFontSize = 48f;
    private float originalFontSize;

    void Start()
    {
        if (titleTransform != null)
            originalTitlePosition = titleTransform.anchoredPosition;

        if (titleText != null)
        {
            originalFontSize = titleText.fontSize;
        }
    }

    void Update()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        bool mouseMoved = mouseDelta.sqrMagnitude > 0.01f; // évite les micro fluctuations

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

        // Déplacement fluide du titre
        if (titleTransform != null)
        {
            Vector2 targetPosition = isIdle ? centerPosition : originalTitlePosition;
            titleTransform.anchoredPosition = Vector2.Lerp(titleTransform.anchoredPosition, targetPosition, Time.deltaTime * moveSpeed);
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
        
        if (titleText != null)
        {
            titleText.fontSize = idleFontSize;
        }
        
    }

    void ShowMenu()
    {
        isIdle = false;
        foreach (GameObject obj in menuElements)
        {
            obj.SetActive(true);
        }
        Cursor.visible = true;
        
        if (titleText != null)
        {
            titleText.fontSize = originalFontSize;
        }
    }
}