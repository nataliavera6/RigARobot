using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class IdentifiedArt : MonoBehaviour
{
    public ARTrackedImageManager manager; 
    public static IdentifiedArt Instance;
    public List<string> pieces = new List<string>();
    private void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        }
    }private void OnEnable()
    {
        manager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        manager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> changes)
    {
        foreach (ARTrackedImage trackedImage in changes.added)
        {
            string imageName = trackedImage.referenceImage.name;

            AddItem(imageName);

            Debug.Log("New artwork identified: " + imageName);
        }
    }

    public void AddItem(string itemName)
    {
        if (!pieces.Contains(itemName))
        {
            pieces.Add(itemName);
        }
    }


}
