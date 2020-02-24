using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Home : MenuLayout
{
    public Button Button_SinglePlayer;
    public Button Button_MultiPlayer;
    public Button Button_Highscores;
    public Button Button_Exit;


    void Awake()
    {
        Button_SinglePlayer.onClick.AddListener(OnButtonSinglePlayer);
        Button_MultiPlayer.onClick.AddListener(OnButtonMultiPlayer);
        Button_Highscores.onClick.AddListener(OnButtonHighscores);
        Button_Exit.onClick.AddListener(OnButtonExit);
    }

    void OnButtonSinglePlayer()
    {

    }

    void OnButtonMultiPlayer()
    {

    }

    void OnButtonHighscores()
    { 
    
    }

    void OnButtonExit()
    {

    }
}
