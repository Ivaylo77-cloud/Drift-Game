using UnityEngine;

public class DriftCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Rigidbody targetRb;

    [Header("Camera Position")]
    public float height = 3f;
    public float distance = 8f;
    public float pitch = 12f;

    [Header("Smoothness")]
    [Tooltip("How quickly the camera moves to its target position.")]
    public float followSmooth = 8f;

    [Tooltip("How quickly the camera rotates. Lower = more lag.")]
    public float rotationSmooth = 2f;

    [Header("Drift Camera")]
    [Range(0f, 1f)]
    [Tooltip("0 = follow car direction, 1 = follow velocity direction.")]
    public float velocityInfluence = 0.6f;

    [Header("Field of View")]
    public Camera cam;
    public float minFOV = 60f;
    public float maxFOV = 80f;
    public float maxSpeed = 50f;
    public float fovSmooth = 5f;

    private float currentYaw;

    void Start()
    {
        if (target != null)
            currentYaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Direction to follow
        Vector3 followDirection = target.forward;

        if (targetRb != null && targetRb.linearVelocity.magnitude > 1f)
        {
            followDirection = Vector3.Slerp(
                target.forward,
                targetRb.linearVelocity.normalized,
                velocityInfluence
            );
        }

        // Smooth yaw
        float targetYaw = Quaternion.LookRotation(followDirection).eulerAngles.y;

        currentYaw = Mathf.LerpAngle(
            currentYaw,
            targetYaw,
            rotationSmooth * Time.deltaTime
        );

        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0);

        // Desired position
        Vector3 desiredPosition =
            target.position
            - rotation * Vector3.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmooth * Time.deltaTime
        );

        // Look at the car
        Vector3 lookPoint = target.position + Vector3.up * 1.2f;
        Quaternion lookRotation = Quaternion.LookRotation(lookPoint - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            followSmooth * Time.deltaTime
        );

        // Speed FOV
        if (cam != null && targetRb != null)
        {
            float speed = targetRb.linearVelocity.magnitude;
            float targetFOV = Mathf.Lerp(minFOV, maxFOV, speed / maxSpeed);

            cam.fieldOfView = Mathf.Lerp(
                cam.fieldOfView,
                targetFOV,
                fovSmooth * Time.deltaTime
            );
        }
    }
}