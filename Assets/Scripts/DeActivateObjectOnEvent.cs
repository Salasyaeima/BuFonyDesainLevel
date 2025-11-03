using UnityEngine;

public class DeActivateObjectOnEvent : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    // Fungsi ini akan dipanggil oleh GameEventListener melalui UnityEvent
    public void DeActivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"{targetObject.name} diaktifkan melalui event!");
        }
        else
        {
            Debug.LogWarning("Target Object belum di-assign!");
        }
    }
}
