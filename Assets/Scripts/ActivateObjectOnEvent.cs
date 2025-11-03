using UnityEngine;

public class ActivateObjectOnEvent : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    // Fungsi ini akan dipanggil oleh GameEventListener melalui UnityEvent
    public void ActivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log($"{targetObject.name} diaktifkan melalui event!");
        }
        else
        {
            Debug.LogWarning("Target Object belum di-assign!");
        }
    }
}
