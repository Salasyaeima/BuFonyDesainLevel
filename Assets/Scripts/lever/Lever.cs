using UnityEngine;

public class Lever : MonoBehaviour
{
    public Animator animator;
    public bool IsTriggered = false;

    public delegate void LeverChanged();
    public static event LeverChanged OnAnyLeverChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Toggle state
        if(IsTriggered)
        {IsTriggered = false;
        animator.SetTrigger("Triggered");}
        else  
        {IsTriggered = true;
        animator.SetTrigger("Triggered");}


        // Notify puzzle controller
        OnAnyLeverChanged?.Invoke();
    }
}
