using UnityEngine;
using TMPro;

public class DialogueAnimatorController : MonoBehaviour
{
    public Animator henryAnimator;
    public Animator jorgeAnimator;

    public TextMeshProUGUI dialogText;

    void Update()
    {
        string text = dialogText.text;

        if (text.StartsWith("Henry"))
        {
            henryAnimator.SetBool("isTalking", true);
            jorgeAnimator.SetBool("isTalking", false);
        }
        else if (text.StartsWith("Jorge"))
        {
            jorgeAnimator.SetBool("isTalking", true);
            henryAnimator.SetBool("isTalking", false);
        }
        else
        {
            henryAnimator.SetBool("isTalking", false);
            jorgeAnimator.SetBool("isTalking", false);
        }
    }
}