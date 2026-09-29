using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0f, 3.5f, -7f);

    [Header("Look At")]
    public float lookHeight = 1.3f;

    [Header("Movement")]
    public float smooth = 8f;

    void LateUpdate()
    {
        if (target == null)
            return;

        FollowTarget(false);
    }

    void FollowTarget(bool instant)
    {
        Vector3 desiredPosition =
            target.TransformPoint(offset);

        Vector3 lookPosition =
            target.position +
            Vector3.up * lookHeight;

        if (instant)
        {
            transform.position =
                desiredPosition;

            transform.LookAt(lookPosition);

            return;
        }

        float t =
            1f -
            Mathf.Exp(
                -smooth *
                Time.unscaledDeltaTime
            );

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                t
            );

        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookPosition -
                transform.position
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                t
            );
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        FollowTarget(true);
    }
}