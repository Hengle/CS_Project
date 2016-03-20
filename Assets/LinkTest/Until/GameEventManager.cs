public static class GameEventManager
{

    public delegate void GameEvent();
    public static event GameEvent GameStart;
    public static event GameEvent GameOver;
    public static event GameEvent GameWin;
    public static event GameEvent GamePaus;
    public static event GameEvent GameMenu;
    public static event GameEvent GamePass;
    public static event GameEvent GameRestart;
    public static event GameEvent TrunRound;
    public static event GameEvent TrunDown;
    public static event GameEvent TrunOver;
    public static event GameEvent Check;
    public static event GameEvent Bomb;

    public static void TriggerGameStart()
    {
        if (GameStart != null)
        {
            GameStart();
        }
    }

    public static void TriggerGameOver()
    {
        if (GameOver != null)
        {
            GameOver();
        }
    }

    public static void TriggerGameWin()
    {
        if (GameWin != null)
        {
            GameWin();
        }
    }

    public static void TriggerGamePause()
    {
        if (GamePaus != null)
        {
            GamePaus();
        }
    }

    public static void TriggerGamePass()
    {
        if (GamePass != null)
        {
            GamePass();
        }
    }

    public static void TriggerGameMenu()
    {
        if (GameMenu != null)
        {
            GameMenu();
        }
    }

    public static void TriggerGameRestart()
    {
        if (GameRestart != null)
        {
            GameRestart();
        }
    }



    public static void TriggeTrunRound()
    {
        if (TrunRound != null)
        {
            TrunRound();
        }
    }


    public static void TriggeTrunDown()
    {
        if (TrunDown != null)
        {
            TrunDown();
        }
    }

    public static void TriggeTrunOver()
    {
        if (TrunOver != null)
        {
            TrunOver();
        }
    }

    public static void TriggeBomb()
    {
        if (Bomb != null)
        {
            Bomb();
        }
    }

    public static void TriggeCheck()
    {
        if (Check != null)
        {
            Check();
        }
    }
}