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

    void Start()
    {
        StartCoroutine(EnterHome());
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
        BounceButton(Button_SinglePlayer).onComplete = ExitScene;        
    }

    void OnButtonMultiPlayer()
    {
        // Switch to MP scene
        BounceButton(Button_MultiPlayer).onComplete = ExitScene;
    }

    void OnButtonHighscores()
    {
        // Switch to High scores
        BounceButton(Button_Highscores).onComplete = ExitScene;
    }

    void OnButtonExit()
    {
        // Fuck off out of code
        BounceButton(Button_Exit).onComplete = ExitScene;
    }
    
    // Transitions
    IEnumerator EnterHome()
    {
        ScaleButtons(0.0f, 0.1f);
        yield return new WaitForSeconds(1.0f);
        ScaleButtons(1.0f, 0.0f, 0.2f);
        yield return null;
    }

    void ExitScene()
    {
        ScaleButtons(0.0f, 0.2f);
    }
    //

    // Tweens
    Tweener BounceButton(Button button)
    {
        button.interactable = false;
        return button.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 1);
    }

    void ScaleButtons(float scaleTo, float scaleFrom = 1.0f, float duration = 1.0f)
    {
        Button_SinglePlayer.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
        Button_MultiPlayer.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
        Button_Highscores.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
        Button_Exit.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
    }
    //
}
