using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;
[RequireComponent(typeof(ARTrackedImage))]
public class ImagePanelController : MonoBehaviour
{
    [SerializeField] private GameObject namee;
    [SerializeField] private GameObject panelPrefab;
    
   
    private GameObject spawnedPanel;
    private GameObject painting;

    private void Awake()
    {
        ARTrackedImage trackedImage = GetComponent<ARTrackedImage>();
        
        painting = Instantiate(namee);
        TextMesh paintingText = painting.GetComponent<TextMesh>();
        paintingText.text = trackedImage.name;
        painting.transform.localPosition = Vector3.zero;

        // Instantiate the panel as a child of the detected image
        spawnedPanel = Instantiate(panelPrefab, trackedImage.transform);
        spawnedPanel.transform.localPosition = Vector3.zero;
        spawnedPanel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Panels face up; adjust if needed

        // Scale the panel to match the physical image size
        Vector2 imageSize = trackedImage.size;
        spawnedPanel.transform.localScale = new Vector3(imageSize.x, imageSize.y, 1f);
    }
}
