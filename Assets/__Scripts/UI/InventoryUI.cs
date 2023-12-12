using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Dray dray; // Reference to the Dray script

    [SerializeField] private Text keyCountText;

    void Start()
    {
        if (dray == null)
        {
            // Assuming Dray is in the same GameObject as InventoryUI
            dray = GetComponent<Dray>();
            
        }

        if (dray != null && keyCountText != null)
        {
            // Accessing the keyCount variable from the Dray script
            UpdateKeyCount(dray.keyCount);
           
            
        }
        else
        {
            Debug.LogWarning("Dray script or keyCountText not assigned properly.");
        }
    }

    // Method to update the key count text
    public void UpdateKeyCount(int count)
    {
        keyCountText.text = "Key: " + count;
        
    }
}
