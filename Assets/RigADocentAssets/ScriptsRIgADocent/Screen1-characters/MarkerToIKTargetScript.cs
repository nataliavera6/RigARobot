using UnityEngine;

public class MarkerToIKTargetScript : MonoBehaviour
{
    [Header("Marker")]
    public Transform markerTransform;

    [Header("IK Target")]
    public Transform leftHandIKTarget;

    [Header("Optional")]
    public Transform leftShoulder;

    [Header("Offsets")]
    public Vector3 positionOffset;
    public Vector3 rotationOffsetEuler;

    [Header("Settings")]
    public float followSpeed = 15f;
    public float maxArmReach = 0.75f;
    public bool markerVisible = true;

    private void Update()
    {
        if (!markerVisible || markerTransform == null || leftHandIKTarget == null)
        {
            return;
        }

        Vector3 targetPosition = markerTransform.TransformPoint(positionOffset);

        Quaternion targetRotation =
            markerTransform.rotation * Quaternion.Euler(rotationOffsetEuler);

        if (leftShoulder != null && maxArmReach > 0f)
        {
            Vector3 shoulderToTarget = targetPosition - leftShoulder.position;

            if (shoulderToTarget.magnitude > maxArmReach)
            {
                targetPosition =
                    leftShoulder.position +
                    shoulderToTarget.normalized * maxArmReach;
            }
        }

        float smoothAmount = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

        leftHandIKTarget.position =
            Vector3.Lerp(leftHandIKTarget.position, targetPosition, smoothAmount);

        leftHandIKTarget.rotation =
            Quaternion.Slerp(leftHandIKTarget.rotation, targetRotation, smoothAmount);
    }
}