using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeoShi_TourneSalo : MonoBehaviour
{
    public Animator animator;
    public string StartShootAnimPalier1;
    // Start is called before the first frame update
    void Start()
    {
        animator.Play(StartShootAnimPalier1);
    }
}
