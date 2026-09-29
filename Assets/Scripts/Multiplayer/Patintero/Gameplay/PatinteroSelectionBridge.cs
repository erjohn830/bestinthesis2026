using TMPro;
using UnityEngine;
public class PatinteroSelectionBridge : MonoBehaviour
{
    // =========================================================
    // CHARACTER UI
    // =========================================================
    [Header("CHARACTER UI")]
    [SerializeField]
    private GameObject characterSelectionPanel;
    [SerializeField]
    private TMP_Text selectedCharacterText;
    // =========================================================
    // CHARACTER NAMES
    // IMPORTANT:
    // These indexes must match PatinteroGameplayPlayer prefab.
    // =========================================================
    private readonly string[] characterNames =
    {
        "Angelo",   // 0
        "Jose",     // 1
        "Juan",     // 2
        "Liam",     // 3
        "Lisa",     // 4
        "Lolo",     // 5
        "Maria",    // 6
        "Miguel",   // 7
        "Pedro",    // 8
        "Robert"    // 9
    };
    // Character currently selected.
    private int pendingCharacterIndex = -1;
    // =========================================================
    // START
    // =========================================================
    private void Start()
    {
        pendingCharacterIndex =
            PatinteroLocalSelection
                .CharacterIndex;
        RefreshCharacterText();
    }
    // =========================================================
    // OPEN CHARACTER PANEL
    // =========================================================
    public void OpenCharacterSelection()
    {
        pendingCharacterIndex =
            PatinteroLocalSelection
                .CharacterIndex;
        RefreshCharacterText();
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel
                .SetActive(true);
        }
    }
    // =========================================================
    // TEAM A
    // =========================================================
    public void SelectTeamA()
    {
        PatinteroLocalSelection
            .SetTeam(0);
        if (
            PatinteroLobbyPlayer.Local != null)
        {
            PatinteroLobbyPlayer
                .Local
                .RequestTeam(0);
        }
        Debug.Log(
            "THIS DEVICE SELECTED TEAM A"
        );
    }
    // =========================================================
    // TEAM B
    // =========================================================
    public void SelectTeamB()
    {
        PatinteroLocalSelection
            .SetTeam(1);
        if (
            PatinteroLobbyPlayer.Local != null)
        {
            PatinteroLobbyPlayer
                .Local
                .RequestTeam(1);
        }
        Debug.Log(
            "THIS DEVICE SELECTED TEAM B"
        );
    }
    // =========================================================
    // CHARACTER BUTTON
    // =========================================================
    // Called by Angelo/Jose/Juan/etc.
    //
    // IMPORTANT:
    // We now also save the clicked character into
    // PatinteroLocalSelection.
    //
    // This is needed because PatinteroMenuUI reads
    // PatinteroLocalSelection.CharacterIndex every frame.
    // =========================================================
    public void SelectCharacter(
        int characterIndex)
    {
        if (
            characterIndex < 0 ||
            characterIndex >=
            characterNames.Length)
        {
            Debug.LogError(
                "INVALID CHARACTER INDEX: " +
                characterIndex
            );
            return;
        }
        // Save clicked character.
        pendingCharacterIndex =
            characterIndex;
        // =========================================
        // IMPORTANT FIX
        // =========================================
        // PatinteroMenuUI uses this value for:
        //
        // Selected: Angelo
        // Selected: Jose
        // etc.
        //
        // Without this, MenuUI keeps seeing -1
        // and changes the text back to "None".
        // =========================================
        PatinteroLocalSelection
            .SetCharacter(
                characterIndex
            );
        RefreshCharacterText();
        Debug.Log(
            "CHARACTER SELECTED: " +
            characterNames[
                pendingCharacterIndex
            ] +
            " | INDEX = " +
            pendingCharacterIndex
        );
    }
    // =========================================================
    // SELECT / CONFIRM BUTTON
    // =========================================================
    // This method can still be used if needed.
    //
    // Your current SELECT button is using:
    //
    // PatinteroMenuUI
    // → ConfirmCharacterSelection()
    //
    // That is okay.
    // =========================================================
    public void ConfirmCharacter()
    {
        pendingCharacterIndex =
            PatinteroLocalSelection
                .CharacterIndex;
        if (
            pendingCharacterIndex < 0 ||
            pendingCharacterIndex >=
            characterNames.Length)
        {
            Debug.LogWarning(
                "SELECT A CHARACTER FIRST."
            );
            return;
        }
        // Save locally.
        PatinteroLocalSelection
            .SetCharacter(
                pendingCharacterIndex
            );
        // Send to network lobby player.
        if (
            PatinteroLobbyPlayer.Local != null)
        {
            PatinteroLobbyPlayer
                .Local
                .RequestCharacter(
                    pendingCharacterIndex
                );
        }
        else
        {
            Debug.LogWarning(
                "PatinteroLobbyPlayer.Local " +
                "does not exist yet. " +
                "Character was saved locally."
            );
        }
        RefreshCharacterText();
        Debug.Log(
            "CHARACTER CONFIRMED: " +
            characterNames[
                pendingCharacterIndex
            ] +
            " | INDEX = " +
            pendingCharacterIndex
        );
        PatinteroMenuUI menu =
            FindFirstObjectByType<
                PatinteroMenuUI>();
        if (menu != null)
        {
            menu.ShowWaitingRoom();
        }
        else if (
            characterSelectionPanel != null)
        {
            characterSelectionPanel
                .SetActive(false);
        }
    }
    // =========================================================
    // CANCEL BUTTON
    // =========================================================
    public void CancelCharacterSelection()
    {
        // Restore the character that is actually
        // confirmed in the network lobby.
        if (
            PatinteroLobbyPlayer.Local != null)
        {
            int confirmedIndex =
                PatinteroLobbyPlayer
                    .Local
                    .CharacterIndex;
            if (
                confirmedIndex >= 0 &&
                confirmedIndex <
                    characterNames.Length)
            {
                PatinteroLocalSelection
                    .SetCharacter(
                        confirmedIndex
                    );
                pendingCharacterIndex =
                    confirmedIndex;
            }
            else
            {
                pendingCharacterIndex =
                    -1;
                PatinteroLocalSelection
                    .SetCharacter(-1);
            }
        }
        else
        {
            pendingCharacterIndex =
                PatinteroLocalSelection
                    .CharacterIndex;
        }
        RefreshCharacterText();
        Debug.Log(
            "CHARACTER SELECTION CANCELLED"
        );
        // Return to Waiting Room.
        PatinteroMenuUI menu =
            FindFirstObjectByType<
                PatinteroMenuUI>();
        if (menu != null)
        {
            menu.ShowWaitingRoom();
        }
        else if (
            characterSelectionPanel != null)
        {
            characterSelectionPanel
                .SetActive(false);
        }
    }
    // =========================================================
    // REFRESH TEXT
    // =========================================================
    public void RefreshCharacterText()
    {
        if (
            selectedCharacterText == null)
        {
            return;
        }
        int index =
            PatinteroLocalSelection
                .CharacterIndex;
        pendingCharacterIndex =
            index;
        if (
            index >= 0 &&
            index <
            characterNames.Length)
        {
            selectedCharacterText.text =
                "Selected: " +
                characterNames[index];
        }
        else
        {
            selectedCharacterText.text =
                "Selected: None";
        }
    }
    // =========================================================
    // RESET
    // =========================================================
    public void ResetSelection()
    {
        PatinteroLocalSelection
            .Reset();
        pendingCharacterIndex =
            -1;
        RefreshCharacterText();
        Debug.Log(
            "PATINTERO LOCAL SELECTION RESET"
        );
    }
}   