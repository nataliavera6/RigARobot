using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultiARMarkersToRigTargets : MonoBehaviour
{
    //Unity inputs for elements in the bindings list 
    [System.Serializable]
    public class MarkerRigBinding
    {
        [Header("Marker")]
        [Tooltip("Must match the image name in the XR Reference Image Library.")]
        // name of image that will be detected 
        public string referenceImageName;


        // Software version of a marker- midpoint between limb and physical marker- limb moves toward target. target moves toward marker
        [Header("Rig Target")]
        public Transform rigTarget=null;

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
    //unity input
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
        //Add list elements to dictionary and set key to marker name and MarkerRigBinding instance as value
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
    //Called in Character renderer to set rig targets for each new character 
        //since characters change in code and not in unity inspector this does what you would normally do in the unity inspector
    public void setRigTargets(Dictionary<string, Transform> MarkerToRigDic){
        foreach(MarkerRigBinding binding in bindings){

            if (MarkerToRigDic.TryGetValue(binding.referenceImageName, out Transform Rig)){
                binding.rigTarget = Rig;
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
            if (string.IsNullOrEmpty(imageName)){
                continue;
            }
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
        if (string.IsNullOrEmpty(imageName)){
            return;
        }
        if (!bindingByImageName.TryGetValue(imageName, out MarkerRigBinding binding))
        {
            return;
        }

        binding.trackedImage = image;
        binding.isTracking = image.trackingState == TrackingState.Tracking;
    }

    //move rig to marker position
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

            //Move position
            binding.rigTarget.position =
                Vector3.Lerp(binding.rigTarget.position, targetPosition, smoothAmount);

            //Move rotation
            if (binding.copyRotation)
            {

                binding.rigTarget.rotation =
                    Quaternion.Slerp(binding.rigTarget.rotation, targetRotation, smoothAmount);
                    
            }
    
                    Debug.Log(
                        $"Image : {binding.trackedImage.referenceImage.name}, State: {binding.trackedImage.trackingState}, Position: {targetPosition}, Rotation: {targetRotation.eulerAngles}");
        }
    }
}
