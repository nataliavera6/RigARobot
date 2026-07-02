using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultiARMarkersToRigTargets : MonoBehaviour
{
    [System.Serializable]
    public class MarkerRigBinding
    {
        [Header("Marker")]
        [Tooltip("Must match the image name in the XR Reference Image Library.")]
        // name of image that will be detected 
        public string referenceImageName;


        // Software version of a marker- midpoint between limb and physical marker- limb moves toward target. target moves toward marker
        [Header("Rig Target")]
        public Transform rigTarget;

        // offset (input by user)- how far away the avatar should be from the marker 
        [Header("Offsets")]
        public Vector3 positionOffset;
        public Vector3 rotationOffsetEuler;

        // movement (input by user)- follow speed is how fast the limb will move toward the detected marker and copy rotation toggle determines whether the limb will rotate with the marker  
        [Header("Movement")]
        public float followSpeed = 25f;
        public bool copyRotation = false;

        [HideInInspector] public ARTrackedImage trackedImage;
        [HideInInspector] public bool isTracking;
        
    }

    [Header("AR Foundation")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Rig")]
    public Transform rig;



    [Header("Marker to Rig Target Bindings")]
    public List<MarkerRigBinding> bindings = new List<MarkerRigBinding>();

    private Dictionary<string, MarkerRigBinding> bindingByImageName =
        new Dictionary<string, MarkerRigBinding>();

    private void Awake()
    {
        bindingByImageName.Clear();

        foreach (MarkerRigBinding binding in bindings)
        {
            if (binding == null || string.IsNullOrEmpty(binding.referenceImageName))
                continue;

            if (!bindingByImageName.ContainsKey(binding.referenceImageName))
            {
                bindingByImageName.Add(binding.referenceImageName, binding);
            }
            else
            {
                Debug.LogWarning(
                    $"Duplicate marker name found: {binding.referenceImageName}");
            }
        }
    }

    private void OnEnable()
    {
        if (trackedImageManager == null)
        {
            Debug.LogError("MultiARMarkersToRigTargets: ARTrackedImageManager is not assigned.");
            return;
        }

        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }
    }

    private void OnTrackedImagesChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (ARTrackedImage image in eventArgs.added)
        {
            RegisterTrackedImage(image);
        }

        foreach (ARTrackedImage image in eventArgs.updated)
        {
            RegisterTrackedImage(image);
        }

        foreach (var removedImage in eventArgs.removed)
        {
            ARTrackedImage image = removedImage.Value;

            if (image == null)
                continue;

            string imageName = image.referenceImage.name;

            if (bindingByImageName.TryGetValue(imageName, out MarkerRigBinding binding))
            {
                binding.trackedImage = null;
                binding.isTracking = false;
            }
        }
    }

    private void RegisterTrackedImage(ARTrackedImage image)
    {
        string imageName = image.referenceImage.name;

        if (!bindingByImageName.TryGetValue(imageName, out MarkerRigBinding binding))
        {
            return;
        }

        binding.trackedImage = image;
        binding.isTracking = image.trackingState == TrackingState.Tracking;
    }

    private void Update()
    {
        foreach (MarkerRigBinding binding in bindings)
        {

            if (binding == null)
                continue;

            if (binding.rigTarget == null)
                continue;

            if (!binding.isTracking || binding.trackedImage == null)
                continue;

            Transform markerTransform = binding.trackedImage.transform;

            Vector3 targetPosition =
                markerTransform.TransformPoint(binding.positionOffset);

            Quaternion targetRotation =
                markerTransform.rotation * Quaternion.Euler(binding.rotationOffsetEuler);

            float smoothAmount =
                1f - Mathf.Exp(-binding.followSpeed * Time.deltaTime);

            binding.rigTarget.position =
                Vector3.Lerp(binding.rigTarget.position, targetPosition, smoothAmount);

            if (binding.copyRotation)
            {
                // Vector3 currentEuler = binding.rigTarget.rotation.eulerAngles;
                // Vector3 targetEuler = targetRotation.eulerAngles; 
                // float newX = Mathf.LerpAngle (currentEuler.x,targetEuler.x,smoothAmount);
                // float newY = Mathf.LerpAngle (currentEuler.y,targetEuler.y,smoothAmount);
                // binding.rigTarget.rotation =
                // //     Quaternion.Slerp(binding.rigTarget.rotation, targetRotation, smoothAmount);
                binding.rigTarget.rotation =
                    Quaternion.Slerp(binding.rigTarget.rotation, targetRotation, smoothAmount);
                    
            }
    
                    Debug.Log(
                        $"Image : {binding.trackedImage.referenceImage.name}, State: {binding.trackedImage.trackingState}, Position: {targetPosition}, Rotation: {targetRotation.eulerAngles}");
        }
    }
}

// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// public class ARTrackedImageToIKTarget : MonoBehaviour
// {
//     [Header("AR Foundation")]
//     public ARTrackedImageManager trackedImageManager;

//     [Tooltip("Leave empty to accept any tracked image.")]
//     public string targetReferenceImageName = "";

//     [Header("Animation Rigging")]
//     public Transform leftHandIKTarget;

//     [Header("Offsets")]
//     public Vector3 positionOffset;
//     public Vector3 rotationOffsetEuler;

//     [Header("Debug")]
//     public bool printDebugLogs = true;

//     private ARTrackedImage currentTrackedImage;
//     private bool markerIsTracking;

//     private void OnEnable()
//     {
//         if (trackedImageManager == null)
//         {
//             Debug.LogError("ARTrackedImageToIKTarget: Tracked Image Manager is not assigned.");
//             return;
//         }

//         trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);

//         if (printDebugLogs)
//             Debug.Log("ARTrackedImageToIKTarget: Listening for tracked images.");
//     }

//     private void OnDisable()
//     {
//         if (trackedImageManager != null)
//             trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
//     }

//     private void OnTrackedImagesChanged(
//         ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
//     {
//         foreach (ARTrackedImage image in eventArgs.added)
//         {
//             TryUseTrackedImage(image, "ADDED");
//         }

//         foreach (ARTrackedImage image in eventArgs.updated)
//         {
//             TryUseTrackedImage(image, "UPDATED");
//         }
//     }

//     private void TryUseTrackedImage(ARTrackedImage image, string eventType)
//     {
//         string detectedName = image.referenceImage.name;

//         if (!string.IsNullOrEmpty(targetReferenceImageName) &&
//             detectedName != targetReferenceImageName)
//         {
//             if (printDebugLogs)
//             {
//                 Debug.Log(
//                     $"Image ignored. Detected '{detectedName}', but script expects '{targetReferenceImageName}'.");
//             }

//             return;
//         }

//         currentTrackedImage = image;
//         markerIsTracking = image.trackingState == TrackingState.Tracking;

//         if (printDebugLogs)
//         {
//             Debug.Log(
//                 $"Image {eventType}: {detectedName}, State: {image.trackingState}, Position: {image.transform.position}");
//         }
//     }

//     private void Update()
//     {
//         if (leftHandIKTarget == null)
//         {
//             Debug.LogError("ARTrackedImageToIKTarget: Left Hand IK Target is not assigned.");
//             return;
//         }

//         if (currentTrackedImage == null)
//             return;

//         if (!markerIsTracking)
//             return;

//         Transform markerTransform = currentTrackedImage.transform;

//         Vector3 targetPosition = markerTransform.TransformPoint(positionOffset);

//         Quaternion targetRotation =
//             markerTransform.rotation * Quaternion.Euler(rotationOffsetEuler);

//         // Direct movement for debugging.
//         leftHandIKTarget.position = targetPosition;
//         leftHandIKTarget.rotation = targetRotation;

//         if (printDebugLogs)
//         {
//             Debug.Log(
//                 $"Moving IK Target to marker position: {targetPosition}");
//         }
//     }
// }