using UnityEngine;

public class TayaLevelController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Taya Visual Model")]
    public Transform tayaModel;

    [Header("Animation State Names")]
    public string level1State = "Lv1";
    public string level2State = "Lv2";
    public string level3State = "Lv3";

    [Header("Height Per Level")]
    public float level1Height = 0f;
    public float level2Height = 0f;
    public float level3Height = 0f;

    [Header("Lock Root Position")]
    public bool lockRootPosition = true;

    private Vector3 fixedPosition;
    private Quaternion fixedRotation;

    private int currentLevel = 1;

    void Awake()
    {
        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        fixedPosition =
            transform.position;

        fixedRotation =
            transform.rotation;
    }

    void Start()
    {
        ApplyModelHeight();
    }

    void LateUpdate()
    {
        if (lockRootPosition)
        {
            transform.position =
                fixedPosition;

            transform.rotation =
                fixedRotation;
        }

        ApplyModelHeight();
    }

    public void PlayLevel(int level)
    {
        level =
            Mathf.Clamp(level, 1, 3);

        currentLevel = level;

        ApplyModelHeight();

        if (animator == null)
        {
            Debug.LogError(
                "TAYA ANIMATOR NOT ASSIGNED!"
            );

            return;
        }

        string stateName = "";

        if (level == 1)
            stateName = level1State;

        else if (level == 2)
            stateName = level2State;

        else if (level == 3)
            stateName = level3State;

        int hash =
            Animator.StringToHash(stateName);

        if (!animator.HasState(0, hash))
        {
            hash =
                Animator.StringToHash(
                    "Base Layer." +
                    stateName
                );
        }

        if (!animator.HasState(0, hash))
        {
            Debug.LogError(
                "TAYA ANIMATION NOT FOUND: "
                + stateName
            );

            return;
        }

        animator.CrossFade(
            hash,
            0.1f,
            0
        );

        Debug.Log(
            "TAYA CHANGED TO LEVEL "
            + level
        );
    }

    void ApplyModelHeight()
    {
        if (tayaModel == null)
            return;

        float height = 0f;

        if (currentLevel == 1)
            height = level1Height;

        else if (currentLevel == 2)
            height = level2Height;

        else if (currentLevel == 3)
            height = level3Height;

        Vector3 localPosition =
            tayaModel.localPosition;

        localPosition.y = height;

        tayaModel.localPosition =
            localPosition;
    }
}