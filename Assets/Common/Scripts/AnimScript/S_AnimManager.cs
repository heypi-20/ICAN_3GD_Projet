using UnityEngine;

public class S_AnimManager : MonoBehaviour
{
    public Animator[] animators;
    [SerializeField] private string triggerName = "PlayDefault";

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Boucle sur chaque Animator et active le Trigger
            foreach (var animator in animators)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}