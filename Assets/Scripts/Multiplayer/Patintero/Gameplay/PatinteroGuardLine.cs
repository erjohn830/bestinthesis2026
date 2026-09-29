using UnityEngine;

public class PatinteroGuardLine : MonoBehaviour
{
    [Header("LINE ENDS")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("INPUT")]
    [Tooltip(
        "OFF = joystick left/right moves on this line. " +
        "ON = joystick up/down moves on this line."
    )]
    [SerializeField] private bool useVerticalJoystick;


    public Vector3 PointA =>
        pointA != null
        ? pointA.position
        : transform.position;


    public Vector3 PointB =>
        pointB != null
        ? pointB.position
        : transform.position;


    public Vector3 Axis
    {
        get
        {
            Vector3 direction =
                PointB - PointA;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return Vector3.right;
            }

            return direction.normalized;
        }
    }


    public Vector3 MidPoint
    {
        get
        {
            return (
                PointA +
                PointB
            ) * 0.5f;
        }
    }


    public float GetMovementInput(
        Vector2 joystick)
    {
        return useVerticalJoystick
            ? joystick.y
            : joystick.x;
    }


    public Vector3 ClampToLine(
        Vector3 worldPosition)
    {
        Vector3 a =
            PointA;

        Vector3 b =
            PointB;

        Vector3 line =
            b - a;

        float lengthSquared =
            line.sqrMagnitude;

        if (lengthSquared <= 0.001f)
        {
            return a;
        }

        float t =
            Vector3.Dot(
                worldPosition - a,
                line
            )
            /
            lengthSquared;

        t = Mathf.Clamp01(t);

        Vector3 result =
            a +
            line * t;

        result.y =
            worldPosition.y;

        return result;
    }


    public Vector3 GetFacingDirection(
        int sign)
    {
        Vector3 perpendicular =
            Vector3.Cross(
                Vector3.up,
                Axis
            ).normalized;

        return perpendicular *
               (sign >= 0 ? 1f : -1f);
    }


    public Quaternion GetFacingRotation(
        int sign)
    {
        Vector3 direction =
            GetFacingDirection(sign);

        if (direction.sqrMagnitude <
            0.001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(
            direction,
            Vector3.up
        );
    }


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (pointA == null ||
            pointB == null)
        {
            return;
        }

        Gizmos.DrawLine(
            pointA.position,
            pointB.position
        );

        Gizmos.DrawSphere(
            pointA.position,
            0.15f
        );

        Gizmos.DrawSphere(
            pointB.position,
            0.15f
        );
    }

#endif
}