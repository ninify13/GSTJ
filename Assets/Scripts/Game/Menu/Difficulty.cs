using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Core.Menu;

namespace Game.Menu
{
    public class Difficulty : MenuLayout
    {
        [SerializeField] Button[] m_difficultyButtons = default;

        protected override void OnEnable()
        {
            base.OnEnable();

            for (int i = 0; i < GSTJ_Core.LevelMeta.Levels.Count; i++)
            {
                m_difficultyButtons[i].gameObject.GetComponentInChildren<Text>().text = GSTJ_Core.LevelMeta.Levels[i].DifficultyKey;
            }
        }

        public void EnterGame(int i)
        {
            PlayerPrefs.SetInt("LEVEL", i);
            ExitScene();
            SceneManager.LoadScene(SceneConstants.Game);
        }

        void ExitScene()
        {
            m_menuManager.SwitchToScreen(MenuItem.Menus.Game);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            for (int i = 0; i < m_difficultyButtons.Length; i++)
            {
                m_difficultyButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
}