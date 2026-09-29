public enum MultiplayerGameType
{
    None = 0,
    Patintero = 1,
    SekyuBase = 2,
    Bamsak = 3
}

public static class MultiplayerGameSelection
{
    public static MultiplayerGameType SelectedGame =
        MultiplayerGameType.None;


    // =========================================================
    // DISPLAY NAME
    // =========================================================

    public static string GetGameName()
    {
        switch (SelectedGame)
        {
            case MultiplayerGameType.Patintero:
                return "Patintero";

            case MultiplayerGameType.SekyuBase:
                return "Sekyu Base";

            case MultiplayerGameType.Bamsak:
                return "Bamsak";

            default:
                return "Game";
        }
    }


    // =========================================================
    // SCENE NAME
    // =========================================================

    public static string GetSceneName()
    {
        switch (SelectedGame)
        {
            case MultiplayerGameType.Patintero:
                return "Patintero";

            case MultiplayerGameType.SekyuBase:
                return "SekyuBase";

            case MultiplayerGameType.Bamsak:
                return "Bamsak";

            default:
                return "";
        }
    }


    // =========================================================
    // EMPTY ROOM MESSAGE
    // =========================================================

    public static string GetNoRoomsMessage()
    {
        return
            "No " +
            GetGameName() +
            " rooms available.";
    }


    // =========================================================
    // EMPTY SERVER MESSAGE
    // =========================================================

    public static string GetNoServersMessage()
    {
        return
            "No " +
            GetGameName() +
            " servers available.";
    }


    // =========================================================
    // RESET
    // =========================================================

    public static void Reset()
    {
        SelectedGame =
            MultiplayerGameType.None;
    }
}