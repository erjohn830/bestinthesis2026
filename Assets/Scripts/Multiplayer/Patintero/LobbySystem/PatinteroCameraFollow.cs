using UnityEngine;

public class PatinteroCameraFollow :
    MonoBehaviour
{
    private Transform target;

    [SerializeField]
    private Vector3 offset =
        new Vector3(
            0f,
            7f,
            -8f
        );

    [SerializeField]
    private float smoothSpeed = 7f;

    [SerializeField]
    private float lookHeight = 1.5f;


    public void SetTarget(
        Transform newTarget)
    {
        target =
            newTarget;
    }


    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired =
            target.position +
            target.TransformDirection(
                offset
            );

        transform.position =
            Vector3.Lerp(
                transform.position,
                desired,
                smoothSpeed *
                Time.deltaTime
            );

        Vector3 lookAt =
            target.position +
            Vector3.up *
            lookHeight;

        Quaternion rotation =
            Quaternion.LookRotation(
                lookAt -
                transform.position
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotation,
                smoothSpeed *
                Time.deltaTime
            );
    }
}