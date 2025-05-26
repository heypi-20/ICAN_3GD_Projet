using UnityEngine;
using UnityEngine.UI;

public class S_JumpCounter : MonoBehaviour
{
    [Header("Jump icons in order from 1st to 3rd jump")]
    [SerializeField] private Image[] jumpIcons;  // Assign 3 UI Images in Inspector

    [Header("Color when jump is available (white)")]
    [SerializeField] private Color availableColor = Color.white;
    [Header("Color when jump is unavailable (gray)")]
    [SerializeField] private Color unavailableColor = Color.gray;

    private S_SuperJump_Module sSuperJumpModule;

    private void Awake()
    {
        // Cache reference to the SuperJump module on parent
        sSuperJumpModule = FindObjectOfType<S_SuperJump_Module>();
        if (jumpIcons == null || jumpIcons.Length == 0)
            Debug.LogError("Please assign jumpIcons array in the Inspector!");
    }

    private void Update()
    {
        // Get the maximum number of jumps allowed at current level
        int maxJumps = sSuperJumpModule.GetCurrentJumpLevel().maxJumpCount;
        // Get how many jumps have been used so far
        int usedJumps = Mathf.RoundToInt(sSuperJumpModule._currentJumpCount);

        // Iterate through each icon
        for (int i = 0; i < jumpIcons.Length; i++)
        {
            if (i < maxJumps)
            {
                // This slot is active—check if it's already used
                if (i < usedJumps)
                    jumpIcons[i].color = unavailableColor; // Already used: gray
                else
                    jumpIcons[i].color = availableColor;   // Still available: white
            }
            else
            {
                // Beyond current max jumps: always gray
                jumpIcons[i].color = unavailableColor;
            }
        }
    }
}