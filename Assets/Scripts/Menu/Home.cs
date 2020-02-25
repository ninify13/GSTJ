using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    void OnDestroy()
    {
        Button_SinglePlayer.onClick.RemoveAllListeners();
        Button_MultiPlayer.onClick.RemoveAllListeners();
        Button_Highscores.onClick.RemoveAllListeners();
        Button_Exit.onClick.RemoveAllListeners();
    }

    void OnButtonSinglePlayer()
    {
        // Switch to SP scene
        //ScaleButtons(0.0f);
        BounceButton(Button_SinglePlayer);
        
    }

    void OnButtonMultiPlayer()
    {
        // Switch to MP scene
        ScaleButtons(0.0f);
    }

    void OnButtonHighscores()
    {
        // Switch to High scores
        ScaleButtons(0.0f);
    }

    void OnButtonExit()
    {
        // Fuck off out of code
        ScaleButtons(0.0f);
    }
    

    // Tweens
    void BounceButton(Button button)
    {
        button.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 1);
    }

    void ScaleButtons(float scaleTo, float duration = 1.0f)
    {
        Button_SinglePlayer.transform.DOScale(scaleTo, duration);
        Button_MultiPlayer.transform.DOScale(scaleTo, duration);
        Button_Highscores.transform.DOScale(scaleTo, duration);
        Button_Exit.transform.DOScale(scaleTo, duration);
    }
}
