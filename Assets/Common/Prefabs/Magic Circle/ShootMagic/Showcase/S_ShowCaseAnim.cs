using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_ShowCaseAnim : MonoBehaviour
{
    // Reference to the Animator component
    public Animator animator;
    // Name of the Animator state to play "Is Shooting"
    public string IsShootingAnimPalier1;
    public string IsShootingAnimPalier2;
    public string IsShootingAnimPalier3;
    public string IsShootingAnimPalier4;
    // Start is called before the first frame update
    void Start()
    {
        if(IsShootingAnimPalier1 != null)
        {
            animator.Play(IsShootingAnimPalier1);
        }
        if (IsShootingAnimPalier2 != null)
        {
            animator.Play(IsShootingAnimPalier2);
        }
        if (IsShootingAnimPalier3 != null)
        {
            animator.Play(IsShootingAnimPalier3);
        }
        if (IsShootingAnimPalier4 != null)
        {
            animator.Play(IsShootingAnimPalier4);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
