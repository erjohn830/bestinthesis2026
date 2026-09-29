using UnityEngine;

public static class PatinteroLocalSelection
{
    // =========================================================
    // LOCAL PLAYER SELECTION
    // =========================================================

    // -1 = no team selected
    //  0 = Team A
    //  1 = Team B
    public static int TeamId = -1;


    // -1 = no character selected
    // 0-9 = character
    public static int CharacterIndex = -1;


    // =========================================================
    // TEAM
    // =========================================================

    public static void SetTeam(
        int teamId)
    {
        TeamId =
            Mathf.Clamp(
                teamId,
                0,
                1
            );

        Debug.Log(
            "LOCAL PATINTERO TEAM SAVED: " +
            (TeamId == 0
                ? "TEAM A"
                : "TEAM B")
        );
    }


    // =========================================================
    // CHARACTER
    // =========================================================

    public static void SetCharacter(
        int characterIndex)
    {
        CharacterIndex =
            Mathf.Clamp(
                characterIndex,
                0,
                9
            );

        Debug.Log(
            "LOCAL PATINTERO CHARACTER SAVED: " +
            CharacterIndex
        );
    }


    // =========================================================
    // RESET
    // =========================================================

    public static void Reset()
    {
        TeamId = -1;

        CharacterIndex = -1;

        Debug.Log(
            "PATINTERO LOCAL SELECTION RESET"
        );
    }


    // =========================================================
    // CHECKS
    // =========================================================

    public static bool HasTeam()
    {
        return
            TeamId == 0 ||
            TeamId == 1;
    }


    public static bool HasCharacter()
    {
        return
            CharacterIndex >= 0 &&
            CharacterIndex <= 9;
    }
}