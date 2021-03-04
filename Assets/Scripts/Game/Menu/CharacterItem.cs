using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Pool;

namespace Game.Menu
{
    public class CharacterItem : PoolItem
    {
        public delegate void OnSelected(CharacterItem characterItem, bool coinUpdateNeeded = false);

        [SerializeField] Button m_selectButton = default;

        [SerializeField] Image m_image = default;

        [SerializeField] TextMeshProUGUI m_name = default;
        [SerializeField] TextMeshProUGUI m_coinValue = default;

        [SerializeField] GameObject m_lockedObject = default;
        [SerializeField] GameObject m_unlockedObject = default;
        [SerializeField] GameObject m_selectedObject = default;

        CharacterMeta m_meta = default;

        OnSelected m_onSelected = default;

        bool m_isLocked = default;

        public override void OnAllocate()
        {
            base.OnAllocate();

            m_selectButton.onClick.AddListener(OnSelect);
        }

        public void Init(CharacterMeta meta, OnSelected onSelected)
        {
            m_image.sprite = meta.Image;
            m_name.text = $"{meta.CharacterName}";

            m_coinValue.text = $"{meta.CoinRequirement}";
            //check if the character is unlocked
            bool isLocked = !(meta.isCharUnlocked);

            m_lockedObject.SetActive(isLocked);
            m_unlockedObject.SetActive(!isLocked);

            m_meta = meta;

            m_onSelected = onSelected;

            m_isLocked = isLocked;

            m_selectedObject.SetActive(false);
        }

        public void SetSelected(bool selected)
        {
            m_selectedObject.SetActive(selected);
        }

        void OnSelect()
        {
            //for detemining if the character can be unlocked or not
            bool isCoinAmountSufficient = false;

            if (m_isLocked)
            {
                //check if this char can be unlocked
                int coins = PlayerPrefs.GetInt(LevelManager.ScoreType.Coin.ToString(), 0);
                isCoinAmountSufficient = (m_meta.CoinRequirement <= coins);

                if (isCoinAmountSufficient == true)
                {
                    //unlock the character
                    m_isLocked = false;
                    m_meta.isCharUnlocked = true;
                    m_lockedObject.SetActive(m_isLocked);
                    m_unlockedObject.SetActive(!m_isLocked);
                    //deduct the required number of coins
                    coins -= m_meta.CoinRequirement;
                    PlayerPrefs.SetInt(LevelManager.ScoreType.Coin.ToString(), coins);
                }
                else //do nothing and return
                    return;
            }
            //select the character and update coins if we unlocked it
            m_onSelected?.Invoke(this, isCoinAmountSufficient);

            GSTJ_Core.SelectedPlayerCharacter = m_meta.CharacterName;

            do
            {
                GSTJ_Core.SelectedNonPlayerCharacter = (Characters.Character)Random.Range(0, (int)Characters.Character.Max);
            }
            while (GSTJ_Core.SelectedNonPlayerCharacter == GSTJ_Core.SelectedPlayerCharacter);

            SetSelected(true);
        }

        public override void OnDeallocate()
        {
            base.OnDeallocate();

            m_selectButton.onClick.RemoveAllListeners();
        }
    }
}