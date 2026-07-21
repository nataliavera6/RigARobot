using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.Calib3dModule;
// using System.Collections.Generic;
// using UnityEngine;

// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// using OpenCVForUnity.CoreModule;
// using OpenCVForUnity.ObjdetectModule;
// using OpenCVForUnity.Calib3dModule;

using CvDictionary = OpenCVForUnity.ObjdetectModule.Dictionary;

public class ArucoMarkerTracker : MonoBehaviour
{
    [System.Serializable]
    public class MarkerRigBinding
    {
       public float positionSmoothTime = 0.05f;
        public float rotationFollowSpeed = 1f;

        public float positionDeadZone = 0.01f;
        public float rotationDeadZone = 1.5f;

        [HideInInspector]
        public Vector3 positionVelocity;

        [Header("Marker")]
        [Tooltip("Must match the ArUco marker ID.")]
        public int markerID;

        [Header("Rig Target")]
        public Transform rigTarget;

        [Header("Offsets")]
        public Vector3 positionOffset;
        public Vector3 rotationOffsetEuler;

        [Header("Movement")]
        public float followSpeed = 25f;
        public bool copyRotation = false;

        [HideInInspector] public bool isTracking;
        [HideInInspector] public Vector3 trackedPosition;
        [HideInInspector] public Quaternion trackedRotation;

  
        [Tooltip("is Body")]
        public bool isbody;
    }

    [Header("AR Foundation")]
    public ARCameraManager arCameraManager;

    [Header("ArUco Settings")]
    public float markerLengthMeters = 0.1f;

    [Header("Rig")]
    public Transform rig;

    [Header("Marker to Rig Target Bindings")]
    public List<MarkerRigBinding> bindings = new List<MarkerRigBinding>();

    private Dictionary<int, MarkerRigBinding> bindingByMarkerID =
        new Dictionary<int, MarkerRigBinding>();

    private Mat rgbaMat;
    private Mat cameraMatrix;
    private MatOfDouble distCoeffs;

    private CvDictionary arucoDictionary;
    private DetectorParameters detectorParameters;
    private ArucoDetector arucoDetector;
    private float nextDetectionTime;
    private void Awake()
    {
        BuildBindingDictionary();

        arucoDictionary = Objdetect.getPredefinedDictionary(Objdetect.DICT_4X4_50);
        detectorParameters = new DetectorParameters();
        arucoDetector = new ArucoDetector(arucoDictionary, detectorParameters);
    }

    private void BuildBindingDictionary()
    {
        bindingByMarkerID.Clear();

        foreach (MarkerRigBinding binding in bindings)
        {
            if (binding == null)
                continue;

            if (!bindingByMarkerID.ContainsKey(binding.markerID))
            {
                bindingByMarkerID.Add(binding.markerID, binding);
            }
            else
            {
                //Debug.LogWarning("Duplicate marker ID found: " + binding.markerID);
            }
        }
    }

    private void Update()
    {
        DetectArucoMarkers();
        MoveRigTargets();
    // if (Time.unscaledTime >= nextDetectionTime)
    // {
    //     nextDetectionTime =
    //         Time.unscaledTime + (1f / 30f);

    //     // DetectArucoMarkers();
    // }
    //     // DetectArucoMarkers();
    //     // MoveRigTargets();
    }
// private void LateUpdate()
// {
//     MoveRigTargets();
// }
    private void DetectArucoMarkers()
    {
        if (arCameraManager == null)
        {
            Debug.LogWarning("ARCameraManager is not assigned.");
            return;
        }

        if (arucoDetector == null)
            return;

        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
            return;

        using (cpuImage)
        {
            XRCpuImage.ConversionParams conversionParams =
            new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(
                    0,
                    0,
                    cpuImage.width,
                    cpuImage.height
                ),

                outputDimensions = new Vector2Int(
                    cpuImage.width,
                    cpuImage.height
                ),

                outputFormat = TextureFormat.RGBA32,

                // Do not mirror ArUco input.
                transformation = XRCpuImage.Transformation.None
            };
            // XRCpuImage.ConversionParams conversionParams =
            //     new XRCpuImage.ConversionParams
            //     {
            //         inputRect = new RectInt(
            //             0,
            //             0,
            //             cpuImage.width,
            //             cpuImage.height
            //         ),

            //         outputDimensions = new Vector2Int(
            //             cpuImage.width,
            //             cpuImage.height
            //         ),

            //         outputFormat = TextureFormat.RGBA32,

            //         // transformation = XRCpuImage.Transformation.MirrorY
            //     };
        int size = cpuImage.GetConvertedDataSize(conversionParams);

        NativeArray<byte> buffer = new NativeArray<byte>(
            size,
            Allocator.Temp
        );

        cpuImage.Convert(
            conversionParams,
            new NativeSlice<byte>(buffer)
        );

        if (rgbaMat == null ||
            rgbaMat.width() != cpuImage.width ||
            rgbaMat.height() != cpuImage.height)
        {
            if (rgbaMat != null)
                rgbaMat.Dispose();

            rgbaMat = new Mat(cpuImage.height, cpuImage.width, CvType.CV_8UC4);

            SetupCameraCalibration(cpuImage.width, cpuImage.height);
        }

        byte[] managedBuffer = buffer.ToArray();

        rgbaMat.put(0, 0, managedBuffer);

        buffer.Dispose();
        }

        // foreach (MarkerRigBinding binding in bindings)
        // {
        //     if (binding != null)
        //         binding.isTracking = false;
        // }

        List<Mat> corners = new List<Mat>();
        Mat ids = new Mat();
        List<Mat> rejected = new List<Mat>();
        Mat grayMat = new Mat();

        // Convert the AR camera image from RGBA color to grayscale.
        Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

        // Detect markers using the grayscale image.
        arucoDetector.detectMarkers(grayMat, corners, ids, rejected);

        // Debug.Log("Detected marker count: " + ids.rows());
        // Debug.Log("Rejected marker candidates: " + rejected.Count);

        // Release the temporary grayscale image.
        grayMat.Dispose();

        if (!ids.empty())
        {
            for (int i = 0; i < ids.rows(); i++)
            {
                int markerID = (int)ids.get(i, 0)[0];

                // Debug.Log("Detected ArUco ID: " + markerID);

                RegisterTrackedMarker(markerID, corners[i]);
            }
        }

        ids.Dispose();

        foreach (Mat corner in corners)
            corner.Dispose();

        foreach (Mat rejectedCorner in rejected)
            rejectedCorner.Dispose();
    }
    private void RegisterTrackedMarker(int markerID, Mat markerCorners)
{
    if (!bindingByMarkerID.TryGetValue(markerID, out MarkerRigBinding binding))
    {
        // Debug.LogWarning("No binding found for ArUco marker ID: " + markerID);
        return;
    }

    if (!TryEstimateMarkerPose(
            markerCorners,
            out Vector3 cameraLocalPosition,
            out Quaternion cameraLocalRotation))
    {
        binding.isTracking = false;
        return;
    }

    Transform cameraTransform = arCameraManager.transform;

    // Correct the 90-degree difference between the CPU camera image
    // and Unity's camera coordinate system.
    Quaternion imageOrientationCorrection =
        Quaternion.Euler(0f, 0f, -90f);

    Vector3 correctedLocalPosition =
        imageOrientationCorrection * cameraLocalPosition;

    Quaternion correctedLocalRotation =
        imageOrientationCorrection * cameraLocalRotation;

    // Convert camera-local pose to Unity world space.
    binding.trackedPosition =
        cameraTransform.TransformPoint(correctedLocalPosition);

    binding.trackedRotation =
        cameraTransform.rotation * correctedLocalRotation;

    binding.isTracking = true;
}
    // private void RegisterTrackedMarker(int markerID, Mat markerCorners)
    // {
    //     if (!bindingByMarkerID.TryGetValue(markerID, out MarkerRigBinding binding))
    //     {
    //         Debug.LogWarning("No binding found for ArUco marker ID: " + markerID);
    //         return;
    //     }

    //     if (!TryEstimateMarkerPose(
    //             markerCorners,
    //             out Vector3 cameraLocalPosition,
    //             out Quaternion cameraLocalRotation))
    //     {
    //         binding.isTracking = false;
    //         Debug.LogWarning("Pose estimation failed for marker ID: " + markerID);
    //         return;
    //     }

    //     Transform cameraTransform = arCameraManager.transform;

    //     // OpenCV returns the marker pose relative to the camera.
    //     // Convert that pose into Unity world space.
    //     binding.trackedPosition =
    //         cameraTransform.TransformPoint(cameraLocalPosition);

    //     binding.trackedRotation =
    //         cameraTransform.rotation * cameraLocalRotation;

    //     binding.isTracking = true;

    //     Debug.Log(
    //         $"Marker {markerID} | " +
    //         $"Camera local: {cameraLocalPosition} | " +
    //         $"World: {binding.trackedPosition}"
    //     );
    // }
    // private void RegisterTrackedMarker(int markerID, Mat markerCorners)
    // {
    //     if (!bindingByMarkerID.TryGetValue(markerID, out MarkerRigBinding binding))
    //     {
    //         Debug.LogWarning("No binding found for ArUco marker ID: " + markerID);
    //         return;
    //     }

    //     if (!TryEstimateMarkerPose(markerCorners, out Vector3 position, out Quaternion rotation))
    //     {
    //         binding.isTracking = false;
    //         Debug.LogWarning("Pose estimation failed for marker ID: " + markerID);
    //         return;
    //     }

    //     binding.trackedPosition = position;
    //     binding.trackedRotation = rotation;
    //     binding.isTracking = true;
    // }
    // var float prevRotation = 0f;





    private void MoveRigTargets()
{
    foreach (MarkerRigBinding binding in bindings)
    {
        if (binding == null)
            continue;

        if (binding.rigTarget == null)
            continue;

        if (!binding.isTracking)
            continue;

        Vector3 targetPosition =
            binding.trackedPosition +
            binding.trackedRotation * binding.positionOffset;

        Quaternion targetRotation =
            binding.trackedRotation *
            Quaternion.Euler(binding.rotationOffsetEuler);

        // POSITION
        float positionDifference = Vector3.Distance(
            binding.rigTarget.position,
            targetPosition
        );
        Vector3 positionChange = targetPosition - binding.rigTarget.position;

        float smoothAmount = 1f - Mathf.Exp(-binding.followSpeed * Time.deltaTime);
        Debug.LogError("before:"+binding.rigTarget.position);
        //binding.rigTarget.position= GetFilteredPos(binding.trackedPosition,targetPosition, 0.10f);
        if (Mathf.Abs(positionChange.x)<=binding.positionDeadZone){
            targetPosition.x=binding.rigTarget.position.x;
        }if (Mathf.Abs(positionChange.y)<=binding.positionDeadZone){
            targetPosition.y=binding.rigTarget.position.y;
        }if (Mathf.Abs(positionChange.z)<=binding.positionDeadZone){
            targetPosition.z=binding.rigTarget.position.z;
        }

        // if (positionDifference > binding.positionDeadZone)
        // {
            binding.rigTarget.position = Vector3.SmoothDamp(
                binding.rigTarget.position,
                targetPosition,
                ref binding.positionVelocity,
                binding.positionSmoothTime
            );
        //     Debug.LogError("after:"+binding.rigTarget.position);
        // }else{
        //     Debug.LogError("ignored");
        // }

        // ROTATION
        if (binding.copyRotation)
        {
            float rotationDifference = Quaternion.Angle(
                binding.rigTarget.rotation,
                targetRotation
            );

            // Ignore extremely small rotational noise.
            if (rotationDifference > binding.rotationDeadZone)
            {
                float rotationAmount =
                    1f - Mathf.Exp(
                        -binding.rotationFollowSpeed *
                        Time.deltaTime
                    );

                binding.rigTarget.rotation =
                    Quaternion.Slerp(
                        binding.rigTarget.rotation,
                        targetRotation,
                        rotationAmount
                    );
            }
            //       Debug.Log(
            //     $"Marker ID: {binding.markerID}, Position: {binding.rigTarget.position}, Rotation: {targetRotation.eulerAngles}"
            // );
        }
            // Debug.Log(
            //     $"Marker ID: {binding.markerID}, Position : {binding.rigTarget.position}"
            // );
    }
}
    // private void MoveRigTargets()
    // {
        
    //     foreach (MarkerRigBinding binding in bindings)
    //     {

    //         if (binding == null)
    //             continue;

    //         if (binding.rigTarget == null)
    //             continue;

    //         if (!binding.isTracking)
    //             continue;

    //         Vector3 targetPosition =
    //             binding.trackedPosition + binding.trackedRotation * binding.positionOffset;

    //         Quaternion targetRotation =
    //             binding.trackedRotation * Quaternion.Euler(binding.rotationOffsetEuler);
            
    //         float smoothAmount =
    //             1f - Mathf.Exp(-binding.followSpeed * Time.deltaTime);

    //         binding.rigTarget.position =
    //             Vector3.Lerp(binding.rigTarget.position, targetPosition, smoothAmount);
    //         // binding.rigTarget.position = targetPosition;
    //         if (binding.copyRotation)
    //         {

    //             binding.rigTarget.rotation =
    //                 Quaternion.Slerp(binding.rigTarget.rotation, targetRotation, smoothAmount);
    //         }

    //         Debug.Log(
    //             $"Marker ID: {binding.markerID}, Position: {targetPosition}, Rotation: {targetRotation.eulerAngles}"
    //         );

            
    //     }
    // }

    private bool TryEstimateMarkerPose(
        Mat markerCorners,
        out Vector3 unityPosition,
        out Quaternion unityRotation)
    {
        unityPosition = Vector3.zero;
        unityRotation = Quaternion.identity;

        if (markerCorners == null || markerCorners.empty())
            return false;

        if (cameraMatrix == null || distCoeffs == null)
            return false;

        float halfSize = markerLengthMeters * 0.5f;

        MatOfPoint3f objectPoints = new MatOfPoint3f(
            new Point3(-halfSize,  halfSize, 0),
            new Point3( halfSize,  halfSize, 0),
            new Point3( halfSize, -halfSize, 0),
            new Point3(-halfSize, -halfSize, 0)
        );

        Point[] imagePointArray = new Point[4];

        for (int i = 0; i < 4; i++)
        {
            double[] corner = markerCorners.get(0, i);

            if (corner == null || corner.Length < 2)
            {
                objectPoints.Dispose();
                return false;
            }

            imagePointArray[i] = new Point(corner[0], corner[1]);
        }

        MatOfPoint2f imagePoints = new MatOfPoint2f(imagePointArray);

        Mat rvec = new Mat();
        Mat tvec = new Mat();

        bool success = Calib3d.solvePnP(
            objectPoints,
            imagePoints,
            cameraMatrix,
            distCoeffs,
            rvec,
            tvec
        );

        if (!success)
        {
            objectPoints.Dispose();
            imagePoints.Dispose();
            rvec.Dispose();
            tvec.Dispose();
            return false;
        }

        double[] rvecArray = new double[3];
        double[] tvecArray = new double[3];

        rvec.get(0, 0, rvecArray);
        tvec.get(0, 0, tvecArray);

        unityPosition = ConvertOpenCVPositionToUnity(tvecArray);
        unityRotation = ConvertOpenCVRotationToUnity(rvecArray);

        objectPoints.Dispose();
        imagePoints.Dispose();
        rvec.Dispose();
        tvec.Dispose();

        return true;
    }

    private void SetupCameraCalibration(int width, int height)
    {
        if (cameraMatrix != null)
            cameraMatrix.Dispose();

        if (distCoeffs != null)
            distCoeffs.Dispose();

        double fx = width;
        double fy = width;
        double cx = width * 0.5;
        double cy = height * 0.5;

        cameraMatrix = new Mat(3, 3, CvType.CV_64FC1);

        cameraMatrix.put(0, 0,
            fx, 0, cx,
            0, fy, cy,
            0, 0, 1
        );

        distCoeffs = new MatOfDouble(0, 0, 0, 0);
    }

    private Vector3 ConvertOpenCVPositionToUnity(double[] tvec)
    {
        float x = (float)tvec[0];
        float y = (float)tvec[1];
        float z = (float)tvec[2];

        return new Vector3(x, -y, z);
    }
    private Quaternion ConvertOpenCVRotationToUnity(double[] rvec)
    {
        Mat rvecMat = new Mat(3, 1, CvType.CV_64FC1);
        rvecMat.put(0, 0, rvec);

        Mat rotationMatrix = new Mat();
        Calib3d.Rodrigues(rvecMat, rotationMatrix);

        double[] r = new double[9];
        rotationMatrix.get(0, 0, r);

        /*
        * OpenCV:
        * X = right
        * Y = down
        * Z = forward
        *
        * Unity:
        * X = right
        * Y = up
        * Z = forward
        *
        * We must flip Y on both sides of the rotation matrix.
        */
        Matrix4x4 matrix = Matrix4x4.identity;

        matrix.m00 =  (float)r[0];
        matrix.m01 = -(float)r[1];
        matrix.m02 =  (float)r[2];

        matrix.m10 = -(float)r[3];
        matrix.m11 =  (float)r[4];
        matrix.m12 = -(float)r[5];

        matrix.m20 =  (float)r[6];
        matrix.m21 = -(float)r[7];
        matrix.m22 =  (float)r[8];

        Vector3 forward = matrix.GetColumn(2);
        Vector3 up = matrix.GetColumn(1);

        Quaternion rotation = Quaternion.LookRotation(
            forward.normalized,
            up.normalized
        );

        rvecMat.Dispose();
        rotationMatrix.Dispose();

        return rotation;
    }
    // private Quaternion ConvertOpenCVRotationToUnity(double[] rvec)
    // {
    //     Mat rvecMat = new Mat(3, 1, CvType.CV_64FC1);
    //     rvecMat.put(0, 0, rvec);

    //     Mat rotationMatrix = new Mat();
    //     Calib3d.Rodrigues(rvecMat, rotationMatrix);

    //     double[] r = new double[9];
    //     rotationMatrix.get(0, 0, r);

    //     Matrix4x4 matrix = Matrix4x4.identity;

    //     matrix.m00 = (float)r[0];
    //     matrix.m01 = (float)r[1];
    //     matrix.m02 = (float)r[2];

    //     matrix.m10 = (float)-r[3];
    //     matrix.m11 = (float)-r[4];
    //     matrix.m12 = (float)-r[5];

    //     matrix.m20 = (float)r[6];
    //     matrix.m21 = (float)r[7];
    //     matrix.m22 = (float)r[8];

    //     Quaternion rotation = Quaternion.LookRotation(
    //         matrix.GetColumn(2),
    //         matrix.GetColumn(1)
    //     );

    //     rvecMat.Dispose();
    //     rotationMatrix.Dispose();

    //     return rotation;
    // }

    public void setRigTargets(Dictionary<int, Transform> newTargets)
    {
        if (newTargets == null)
        {
            Debug.LogError("setRigTargets received a null dictionary.");
            return;
        }

        foreach (var pair in newTargets)
        {
            int markerID = pair.Key;
            Transform rigTarget = pair.Value;

            if (rigTarget == null)
            {
                Debug.LogWarning("Skipped marker ID " + markerID + " because rig target was null.");
                continue;
            }

            if (bindingByMarkerID.TryGetValue(markerID, out MarkerRigBinding binding))
            {
                binding.rigTarget = rigTarget;
            }
            else
            {
                Debug.LogWarning("No ArUco binding exists for marker ID: " + markerID);
            }
        }
    }
    private Vector3 GetFilteredPos(Vector3 prev,Vector3 cur,float smooth){
            Vector3 result = (smooth * cur)+((1f-smooth)*prev);
            return result;
    }

    private void OnDestroy()
    {
        if (rgbaMat != null)
            rgbaMat.Dispose();

        if (cameraMatrix != null)
            cameraMatrix.Dispose();

        if (distCoeffs != null)
            distCoeffs.Dispose();

        if (arucoDetector != null)
            arucoDetector.Dispose();

        if (detectorParameters != null)
            detectorParameters.Dispose();

        if (arucoDictionary != null)
            arucoDictionary.Dispose();
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using OpenCVForUnity.CoreModule;
// using OpenCVForUnity.ObjdetectModule;
// using OpenCVForUnity.Calib3dModule;
// using OpenCVForUnity.UnityUtils;

// using CvDictionary = OpenCVForUnity.ObjdetectModule.Dictionary;

// public class ArucoMarkerTracker : MonoBehaviour
// {
//     [System.Serializable]
//     public class MarkerRigBinding
//     {
//         [Header("Marker")]
//         [Tooltip("Must match the ArUco marker ID.")]
//         public int markerID;

//         [Header("Rig Target")]
//         public Transform rigTarget;

//         [Header("Offsets")]
//         public Vector3 positionOffset;
//         public Vector3 rotationOffsetEuler;

//         [Header("Movement")]
//         public float followSpeed = 25f;
//         public bool copyRotation = false;

//         [HideInInspector] public bool isTracking;
//         [HideInInspector] public Vector3 trackedPosition;
//         [HideInInspector] public Quaternion trackedRotation;
//     }

//     [Header("Camera Input")]
//     // public WebCamTexture webCamTexture;
//     public ARCameraManager arCameraManager;

//     [Header("ArUco Settings")]
//     public float markerLengthMeters = 0.1f;

//     [Header("Rig")]
//     public Transform rig;

//     [Header("Marker to Rig Target Bindings")]
//     public List<MarkerRigBinding> bindings = new List<MarkerRigBinding>();

//     private Dictionary<int, MarkerRigBinding> bindingByMarkerID =
//         new Dictionary<int, MarkerRigBinding>();

//     private Mat rgbaMat;
//     private Mat cameraMatrix;
//     private MatOfDouble distCoeffs;

//     private CvDictionary arucoDictionary;
//     private DetectorParameters detectorParameters;
//     private ArucoDetector arucoDetector;

//     private void Awake()
//     {
//         BuildBindingDictionary();
//     }

//     private void Start()
//     {
//         StartCoroutine(StartCamera());
//     }

//     private void BuildBindingDictionary()
//     {
//         bindingByMarkerID.Clear();

//         foreach (MarkerRigBinding binding in bindings)
//         {
//             if (binding == null)
//                 continue;

//             if (!bindingByMarkerID.ContainsKey(binding.markerID))
//             {
//                 bindingByMarkerID.Add(binding.markerID, binding);
//             }
//             else
//             {
//                 Debug.LogWarning("Duplicate marker ID found: " + binding.markerID);
//             }
//         }
//     }

//     private IEnumerator StartCamera()
//     {
//         webCamTexture = new WebCamTexture();
//         webCamTexture.Play();

//         while (webCamTexture.width <= 16 || webCamTexture.height <= 16)
//         {
//             yield return null;
//         }

//         rgbaMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);

//         arucoDictionary = Objdetect.getPredefinedDictionary(Objdetect.DICT_4X4_50);
//         detectorParameters = new DetectorParameters();
//         arucoDetector = new ArucoDetector(arucoDictionary, detectorParameters);

//         SetupCameraCalibration();

//         Debug.Log("ArucoMarkerTracker camera started: " + webCamTexture.width + "x" + webCamTexture.height);
//     }

//     private void Update()
//     {
//         if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
//             return;
//         DetectArucoMarkers();
//         MoveRigTargets();
//     }

//     private void DetectArucoMarkers()
//     {
//         if (webCamTexture == null)
//             return;

//         if (!webCamTexture.didUpdateThisFrame)
//             return;

//         if (rgbaMat == null || arucoDetector == null)
//             return;

//         foreach (MarkerRigBinding binding in bindings)
//         {
//             if (binding != null)
//             {
//                 binding.isTracking = false;
//             }
//         }

//         Utils.webCamTextureToMat(webCamTexture, rgbaMat);

//         List<Mat> corners = new List<Mat>();
//         Mat ids = new Mat();

//         arucoDetector.detectMarkers(rgbaMat, corners, ids);
//         Debug.Log("Detected marker count: " + ids.rows());
//         if (ids.empty())
//         {
//             ids.Dispose();
//             return;
//         }

//         for (int i = 0; i < ids.rows(); i++)
//         {
//             int markerID = (int)ids.get(i, 0)[0];

//             RegisterTrackedMarker(markerID, corners[i]);
//             Debug.Log("Detected ArUco ID: " + markerID);
//         }

//         ids.Dispose();

//         foreach (Mat corner in corners)
//         {
//             corner.Dispose();
//         }
//     }

//     private void RegisterTrackedMarker(int markerID, Mat markerCorners)
//     {
//         if (!bindingByMarkerID.TryGetValue(markerID, out MarkerRigBinding binding))
//         {
//             return;
//         }

//         if (!TryEstimateMarkerPose(markerCorners, out Vector3 position, out Quaternion rotation))
//         {
//             binding.isTracking = false;
//             return;
//         }

//         binding.trackedPosition = position;
//         binding.trackedRotation = rotation;
//         binding.isTracking = true;
//     }

//     private void MoveRigTargets()
//     {
//         foreach (MarkerRigBinding binding in bindings)
//         {
//             if (binding == null)
//                 continue;

//             if (binding.rigTarget == null)
//                 continue;

//             if (!binding.isTracking)
//                 continue;

//             Vector3 targetPosition =
//                 binding.trackedPosition + binding.trackedRotation * binding.positionOffset;

//             Quaternion targetRotation =
//                 binding.trackedRotation * Quaternion.Euler(binding.rotationOffsetEuler);

//             float smoothAmount =
//                 1f - Mathf.Exp(-binding.followSpeed * Time.deltaTime);

//             binding.rigTarget.position =
//                 Vector3.Lerp(binding.rigTarget.position, targetPosition, smoothAmount);

//             if (binding.copyRotation)
//             {
//                 binding.rigTarget.rotation =
//                     Quaternion.Slerp(binding.rigTarget.rotation, targetRotation, smoothAmount);
//             }

//             Debug.Log(
//                 $"Marker ID: {binding.markerID}, Position: {targetPosition}, Rotation: {targetRotation.eulerAngles}"
//             );
//         }
//     }

//     private bool TryEstimateMarkerPose(
//         Mat markerCorners,
//         out Vector3 unityPosition,
//         out Quaternion unityRotation)
//     {
//         unityPosition = Vector3.zero;
//         unityRotation = Quaternion.identity;

//         if (markerCorners == null || markerCorners.empty())
//             return false;

//         float halfSize = markerLengthMeters * 0.5f;

//         MatOfPoint3f objectPoints = new MatOfPoint3f(
//             new Point3(-halfSize,  halfSize, 0),
//             new Point3( halfSize,  halfSize, 0),
//             new Point3( halfSize, -halfSize, 0),
//             new Point3(-halfSize, -halfSize, 0)
//         );

//         Point[] imagePointArray = new Point[4];

//         for (int i = 0; i < 4; i++)
//         {
//             double[] corner = markerCorners.get(0, i);

//             if (corner == null || corner.Length < 2)
//             {
//                 objectPoints.Dispose();
//                 return false;
//             }

//             imagePointArray[i] = new Point(corner[0], corner[1]);
//         }

//         MatOfPoint2f imagePoints = new MatOfPoint2f(imagePointArray);

//         Mat rvec = new Mat();
//         Mat tvec = new Mat();

//         bool success = Calib3d.solvePnP(
//             objectPoints,
//             imagePoints,
//             cameraMatrix,
//             distCoeffs,
//             rvec,
//             tvec
//         );

//         if (!success)
//         {
//             objectPoints.Dispose();
//             imagePoints.Dispose();
//             rvec.Dispose();
//             tvec.Dispose();
//             return false;
//         }

//         double[] rvecArray = new double[3];
//         double[] tvecArray = new double[3];

//         rvec.get(0, 0, rvecArray);
//         tvec.get(0, 0, tvecArray);

//         unityPosition = ConvertOpenCVPositionToUnity(tvecArray);
//         unityRotation = ConvertOpenCVRotationToUnity(rvecArray);

//         objectPoints.Dispose();
//         imagePoints.Dispose();
//         rvec.Dispose();
//         tvec.Dispose();

//         return true;
//     }

//     private void SetupCameraCalibration()
//     {
//         double width = webCamTexture.width;
//         double height = webCamTexture.height;

//         double fx = width;
//         double fy = width;
//         double cx = width * 0.5;
//         double cy = height * 0.5;

//         cameraMatrix = new Mat(3, 3, CvType.CV_64FC1);

//         cameraMatrix.put(0, 0,
//             fx, 0, cx,
//             0, fy, cy,
//             0, 0, 1
//         );

//         distCoeffs = new MatOfDouble(0, 0, 0, 0);
//     }

//     private Vector3 ConvertOpenCVPositionToUnity(double[] tvec)
//     {
//         float x = (float)tvec[0];
//         float y = (float)tvec[1];
//         float z = (float)tvec[2];

//         // OpenCV: x right, y down, z forward
//         // Unity:  x right, y up,   z forward
//         return new Vector3(x, -y, z);
//     }

//     private Quaternion ConvertOpenCVRotationToUnity(double[] rvec)
//     {
//         Mat rvecMat = new Mat(3, 1, CvType.CV_64FC1);
//         rvecMat.put(0, 0, rvec);

//         Mat rotationMatrix = new Mat();
//         Calib3d.Rodrigues(rvecMat, rotationMatrix);

//         double[] r = new double[9];
//         rotationMatrix.get(0, 0, r);

//         Matrix4x4 matrix = Matrix4x4.identity;

//         matrix.m00 = (float)r[0];
//         matrix.m01 = (float)r[1];
//         matrix.m02 = (float)r[2];

//         matrix.m10 = (float)-r[3];
//         matrix.m11 = (float)-r[4];
//         matrix.m12 = (float)-r[5];

//         matrix.m20 = (float)r[6];
//         matrix.m21 = (float)r[7];
//         matrix.m22 = (float)r[8];

//         Quaternion rotation = Quaternion.LookRotation(
//             matrix.GetColumn(2),
//             matrix.GetColumn(1)
//         );

//         rvecMat.Dispose();
//         rotationMatrix.Dispose();

//         return rotation;
//     }

//     public void setRigTargets(Dictionary<int, Transform> newTargets)
//     {
//         if (newTargets == null)
//         {
//             Debug.LogError("setRigTargets received a null dictionary.");
//             return;
//         }

//         foreach (var pair in newTargets)
//         {
//             int markerID = pair.Key;
//             Transform rigTarget = pair.Value;

//             if (rigTarget == null)
//             {
//                 Debug.LogWarning("Skipped marker ID " + markerID + " because rig target was null.");
//                 continue;
//             }

//             if (bindingByMarkerID.TryGetValue(markerID, out MarkerRigBinding binding))
//             {
//                 binding.rigTarget = rigTarget;
//             }
//             else
//             {
//                 Debug.LogWarning("No ArUco binding exists for marker ID: " + markerID);
//             }
//         }
//     }

//     private void OnDestroy()
//     {
//         if (webCamTexture != null)
//         {
//             webCamTexture.Stop();
//         }

//         if (rgbaMat != null)
//             rgbaMat.Dispose();

//         if (cameraMatrix != null)
//             cameraMatrix.Dispose();

//         if (distCoeffs != null)
//             distCoeffs.Dispose();

//         if (arucoDetector != null)
//             arucoDetector.Dispose();

//         if (detectorParameters != null)
//             detectorParameters.Dispose();

//         if (arucoDictionary != null)
//             arucoDictionary.Dispose();
//     }
// }