using UnityEngine;
using UnityEngine.Events;

public class TriggerGameEventOnCollision : MonoBehaviour
{
    [SerializeField] private UnityEvent OnAddObject;  // Event yang akan dipicu
    [SerializeField] private UnityEvent OnEnableGameObject;  // Event yang akan dipicu
    [SerializeField] private UnityEvent OnDisableGameObject;  // Event yang akan dipicu
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Trigger terdeteksi oleh {other.name}, event dipicu!");
            OnAddObject.Invoke(); // Memanggil semua listener
            OnEnableGameObject.Invoke();
            OnDisableGameObject.Invoke();
                                    
        }
    }
}
