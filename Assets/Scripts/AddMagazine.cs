using UnityEngine;
using UnityEngine.Events;

public class AddMagazine : MonoBehaviour
{
    [SerializeField] private UnityEvent OnUpdateMagazine;  // Event yang akan dipicu
    [SerializeField] private UnityEvent OnDisableGameObject;

    private void OnTriggerEnter(Collider other)
    {
        // Pastikan hanya objek tertentu yang bisa memicu (opsional)
        // Misalnya: hanya Player
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Trigger terdeteksi oleh {other.name}, event dipicu!");
            OnUpdateMagazine.Invoke(); // Memanggil semua listener
            OnDisableGameObject.Invoke();
        }
    }
}
