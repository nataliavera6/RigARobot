using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class IdentifiedArt : MonoBehaviour
{
    public static IdentifiedArt Instance;

    [SerializeField]
    private ARTrackedImageManager manager;

    public List<string> pieces = new List<string>();

    private void Awake()
    {
        Debug.LogError("IdentifiedArt Awake ran.");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.LogError("IdentifiedArt instance created.");
        }
        else
        {
            Debug.LogError("Duplicate IdentifiedArt destroyed.");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        Debug.LogError("IdentifiedArt OnEnable ran.");

        if (manager == null)
        {
            Debug.LogError(
                "ARTrackedImageManager is not assigned."
            );

            return;
        }

        manager.trackablesChanged.AddListener(
            OnTrackedImagesChanged
        );

        Debug.LogError(
            "Subscribed to tracked image changes."
        );
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.trackablesChanged.RemoveListener(
                OnTrackedImagesChanged
            );
        }
    }

    private void OnTrackedImagesChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> changes)
    {
        Debug.LogError(
            "Tracked images changed. Added count: " +
            changes.added.Count
        );

        foreach (ARTrackedImage trackedImage in changes.added)
        {
            string imageName =
                trackedImage.referenceImage.name;

            Debug.LogError(
                "New tracked image detected: " + imageName
            );

            AddItem(imageName);
        }
    }

    public void AddItem(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            Debug.LogError("Image name was empty.");
            return;
        }

        if (!pieces.Contains(itemName))
        {
            pieces.Add(itemName);

            Debug.LogError(
                "Artwork added to list: " + itemName
            );

            Debug.LogError(
                "Current list count: " + pieces.Count
            );
        }
        else
        {
            Debug.LogError(
                "Artwork was already in list: " + itemName
            );
        }
    }
}

// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;
// public class IdentifiedArt : MonoBehaviour
// {
//     public ARTrackedImageManager manager; 
//     public static IdentifiedArt Instance;
//     public List<string> pieces = new List<string>();
//     private void Awake(){
//         if (Instance == null){
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }else{
//             Destroy(gameObject);
//         }
//     }private void OnEnable()
//     {
//         manager.trackablesChanged.AddListener(OnTrackedImagesChanged);
//     }

//     private void OnDisable()
//     {
//         manager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
//     }

//     private void OnTrackedImagesChanged(
//         ARTrackablesChangedEventArgs<ARTrackedImage> changes)
//     {
//         foreach (ARTrackedImage trackedImage in changes.added)
//         {
//             string imageName = trackedImage.referenceImage.name;

//             AddItem(imageName);

//             Debug.Log("New artwork identified: " + imageName);
//         }
//     }

//     public void AddItem(string itemName)
//     {
//         if (!pieces.Contains(itemName))
//         {
//             Debug.LogWarning("art identified and added"+itemName);
//             pieces.Add(itemName);
//         }
//     }


// }
