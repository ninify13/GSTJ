using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Core.Menu;

namespace Game.Menu
{
    public class Home : MenuLayout
    {
        [SerializeField] Button m_button_SinglePlayer = default;
        [SerializeField] Button m_button_MultiPlayer = default;
        [SerializeField] Button m_button_Highscores = default;
        [SerializeField] Button m_button_Instructions = default;
        [SerializeField] Button m_button_Tutorial = default;
        [SerializeField] Button m_button_Exit = default;

        //check mark for tutorial button
        [SerializeField] private GameObject tutorialCheckMark;
        MenuItem.Menus m_transitionToScene = default;

        protected override void Awake()
        {
            base.Awake();

            m_button_SinglePlayer.onClick.AddListener(OnButtonSinglePlayer);
            m_button_MultiPlayer.onClick.AddListener(OnButtonMultiPlayer);
            m_button_Highscores.onClick.AddListener(OnButtonHighscores);
            m_button_Instructions.onClick.AddListener(OnButtonInstruction);
            m_button_Tutorial.onClick.AddListener(OnButtonTutorial);
            m_button_Exit.onClick.AddListener(OnButtonExit);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //set the state of tutorial check mark
            //if the player has seen FTUE
            if (GSTJ_Core.hasPlayerSeenFTUE == true)
                OnButtonTutorial();

            StartCoroutine(EnterHome());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_button_SinglePlayer.onClick.RemoveAllListeners();
            m_button_MultiPlayer.onClick.RemoveAllListeners();
            m_button_Highscores.onClick.RemoveAllListeners();
            m_button_Instructions.onClick.RemoveAllListeners();
            m_button_Tutorial.onClick.RemoveAllListeners();
            m_button_Exit.onClick.RemoveAllListeners();
        }

        void OnButtonSinglePlayer()
        {
            // Switch to SP scene
            BounceButton(m_button_SinglePlayer).onComplete = ExitScene;
            m_transitionToScene = MenuItem.Menus.Characters;
            GSTJ_Core.SelectedMode = GSTJ_Core.GameMode.Single;
        }

        void OnButtonMultiPlayer()
        {
            // Switch to MP scene
            BounceButton(m_button_MultiPlayer).onComplete = ExitScene;
            m_transitionToScene = MenuItem.Menus.Characters;
            GSTJ_Core.SelectedMode = GSTJ_Core.GameMode.Multi;
        }

        void OnButtonHighscores()
        {
            // Switch to High scores
            BounceButton(m_button_Highscores).onComplete = ExitScene;
            m_transitionToScene = MenuItem.Menus.Scores;
        }
        void OnButtonInstruction()
        {
            BounceButton(m_button_Instructions).onComplete = ExitScene;
            m_transitionToScene = MenuItem.Menus.Instructions;
        }

        void OnButtonTutorial()
        {
            //toggle check mark
            if (tutorialCheckMark != null)
            {
                tutorialCheckMark.gameObject.SetActive(!tutorialCheckMark.gameObject.activeSelf);
                GSTJ_Core.m_ShowFTUE = !GSTJ_Core.m_ShowFTUE;
            }
        }

        void OnButtonExit()
        {
            // Fuck off out of code
            BounceButton(m_button_Exit).onComplete = ExitScene;
        }

        // Transitions
        IEnumerator EnterHome()
        {
            ScaleButtons(1.0f, 0.0f, 0.2f);

            m_button_SinglePlayer.interactable = true;
            m_button_MultiPlayer.interactable = true;
            m_button_Highscores.interactable = true;
            m_button_Instructions.interactable = true;
            m_button_Tutorial.interactable = true;
            m_button_Exit.interactable = true;

            yield return null;
        }

        void ExitScene()
        {
            ScaleButtons(0.0f, 0.2f);
            m_menuManager.SwitchToScreen(m_transitionToScene);
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
            m_button_SinglePlayer.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
            m_button_MultiPlayer.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
            m_button_Highscores.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
            m_button_Instructions.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
            m_button_Exit.transform.DOScale(scaleTo, duration).From(scaleFrom, true);
        }
        //
    }
}