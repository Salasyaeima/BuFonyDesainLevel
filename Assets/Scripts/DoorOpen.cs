using UnityEngine;
using UnityEngine.Events;

public class DoorOpen : MonoBehaviour
{
    private Animator m_Animator;
    private bool isOpen = false;
        
    void Awake() 
    {
        m_Animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Trigger terdeteksi oleh {other.name}, event dipicu!");
            isOpen = true;
            if(m_Animator == null) 
            {return;}
            m_Animator.SetTrigger("Open");
            Destroy(this);
                                    
        }
    }
}
