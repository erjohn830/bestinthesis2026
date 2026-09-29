using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatinteroPlayerRowUI : MonoBehaviour
{
    [Header("PLAYER UI")]
    [SerializeField]
    private Image characterImage;

    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private Image readyStatusImage;


    // =========================================
    // SHOW PLAYER
    // =========================================

    public void ShowPlayer(
        string playerName,
        Sprite characterSprite,
        bool isHost,
        bool isReady,
        Sprite readySprite,
        Sprite notReadySprite)
    {
        gameObject.SetActive(true);


        // ------------------------------
        // NAME
        // ------------------------------

        if (playerNameText != null)
        {
            playerNameText.gameObject
                .SetActive(true);


            playerNameText.text =
                isHost
                ? playerName + " (HOST)"
                : playerName;
        }


        // ------------------------------
        // CHARACTER PORTRAIT
        // ------------------------------

        if (characterImage != null)
        {
            bool hasCharacter =
                characterSprite != null;


            characterImage.gameObject
                .SetActive(
                    hasCharacter
                );


            if (hasCharacter)
            {
                characterImage.sprite =
                    characterSprite;
            }
        }


        // ------------------------------
        // READY IMAGE
        // ------------------------------

        if (readyStatusImage != null)
        {
            // Host does not press Ready.
            if (isHost)
            {
                readyStatusImage.gameObject
                    .SetActive(false);
            }
            else
            {
                readyStatusImage.gameObject
                    .SetActive(true);


                readyStatusImage.sprite =
                    isReady
                    ? readySprite
                    : notReadySprite;
            }
        }
    }


    // =========================================
    // EMPTY SLOT
    // =========================================

    public void ShowEmpty()
    {
        gameObject.SetActive(true);


        // Hide character completely.
        if (characterImage != null)
        {
            characterImage.sprite =
                null;

            characterImage.gameObject
                .SetActive(false);
        }


        // Hide name completely.
        if (playerNameText != null)
        {
            playerNameText.text =
                "";

            playerNameText.gameObject
                .SetActive(false);
        }


        // Hide Ready / Not Ready image.
        if (readyStatusImage != null)
        {
            readyStatusImage.sprite =
                null;

            readyStatusImage.gameObject
                .SetActive(false);
        }
    }
}