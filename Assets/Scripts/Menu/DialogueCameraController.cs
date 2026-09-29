using UnityEngine;
using TMPro;

public class DialogueCameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Camera henryCamera;
    public Camera jorgeCamera;

    public TextMeshProUGUI dialogText;

    void Update()
    {
        string text = dialogText.text;

        if (text.StartsWith("Henry"))
        {
            mainCamera.enabled = false;
            henryCamera.enabled = true;
            jorgeCamera.enabled = false;
        }
        else if (text.StartsWith("Jorge"))
        {
            mainCamera.enabled = false;
            henryCamera.enabled = false;
            jorgeCamera.enabled = true;
        }
    }
}