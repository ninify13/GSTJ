using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Core.Menu;
using Core.Pool;

namespace Game.Menu
{
    public class Characters : MenuLayout
    {
        public enum Character
        {
            Humpy,
            Gorgy,
            Aj,
            Adi,
            Dj,
            Max,
        }

        [SerializeField] PoolManager m_poolManager = default;

        [SerializeField] Transform m_contentParent = default;

        [SerializeField] ScrollRect m_scrollRect = default;

        [SerializeField] TextMeshProUGUI m_coinCount = default;

        [SerializeField] Button m_button_Play = default;
        //for setting scroll options at the start
        [SerializeField] Scrollbar m_scrollBar = default;

        PoolItem[] m_characterItems = default;
        CharacterItem m_characterItem = default;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //setting the scroll bar to the left
            m_scrollBar.value = 0;
            m_button_Play.onClick.AddListener(EnterGame);

            EnterScene();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_button_Play.onClick.RemoveAllListeners();

            for (int i = 0; i < (int)Character.Max; i++)
            {
                m_poolManager.ReturnPoolItem(m_characterItems[i]);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void EnterGame()
        {
            ExitScene();
        }

        void EnterScene()
        {
            int coins = PlayerPrefs.GetInt(LevelManager.ScoreType.Coin.ToString(), 0);
            m_coinCount.text = coins.ToString();

            m_characterItems = new PoolItem[(int)Character.Max];
            for (int i = 0; i < (int)Character.Max; i++)
            {
                m_characterItems[i] = m_poolManager.GetPoolItem(PoolType.CharacterItem);
                m_characterItems[i].transform.SetParent(m_contentParent);
                m_characterItems[i].transform.localScale = Vector3.one;
                CharacterItem characterItem = m_characterItems[i].gameObject.GetComponent<CharacterItem>();
                CharacterMeta characterMeta = GSTJ_Core.CharacterMeta.Characters[i];
                characterItem.Init(characterMeta, OnSelectCharacter);
                //for first character, set it selected by default
                if (i == 0)
                {
                    characterItem.SetSelected(true);
                    m_characterItem = characterItem;
                }
            }

            m_scrollRect.horizontalScrollbar.value = 0.0f;

            //ScaleButtons(1.0f, 0.0f, 0.5f);
        }

        void OnSelectCharacter(CharacterItem characterItem, bool coinUpdateNeeded = false)
        {
            if (m_characterItem != null)
                m_characterItem.SetSelected(false);

            //if coins need to be updated, do so now
            if (coinUpdateNeeded == true)
            {
                int coins = PlayerPrefs.GetInt(LevelManager.ScoreType.Coin.ToString(), 0);
                m_coinCount.text = coins.ToString();
            }
            m_characterItem = characterItem;
        }

        void ExitScene()
        {
            //ScaleButtons(0.0f, 1.0f, 0.2f);
            m_menuManager.SwitchToScreen(MenuItem.Menus.Difficulty);
        }

        void ScaleButtons(float scaleTo, float scaleFrom = 1.0f, float duration = 1.0f)
        {
            for (int i = 0; i < m_characterItems.Length; i++)
            {
                m_characterItems[i].transform.DOScale(scaleTo, duration).From(scaleFrom, true);
            }
        }
    }
}