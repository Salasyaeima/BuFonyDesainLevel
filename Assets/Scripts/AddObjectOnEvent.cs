using UnityEngine;

public class SetAsChildAndActivate : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;   // Objek yang akan dijadikan child
    [SerializeField] private Transform parentObject;    // Parent baru untuk targetObject

    // Fungsi ini bisa dipanggil dari GameEventListener atau UnityEvent lainnya
    public void SetChildAndActivate()
    {
        if (targetObject == null || parentObject == null)
        {
            Debug.LogWarning("Target atau Parent belum di-assign!");
            return;
        }

        // Jadikan target sebagai child
        targetObject.transform.SetParent(parentObject);

        // Aktifkan object
        targetObject.SetActive(true);

        Debug.Log($"{targetObject.name} kini menjadi child dari {parentObject.name} dan telah diaktifkan.");
    }
}
