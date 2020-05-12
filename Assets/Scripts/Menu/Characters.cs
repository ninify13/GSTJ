using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class Characters : MenuLayout
{
    public enum Character
    {
        Humpy,
        //Gorgy,
        Aj,
        Adi,
        Dj,
        Max,
    }

    [SerializeField] GameObject[] m_selectionArrows = default;

    [SerializeField] Transform[] m_characters = default;

    [SerializeField] GameObject[] m_lockButtons = default;
    [SerializeField] GameObject[] m_selectImages = default;

    [SerializeField] Text[] m_lockTexts = default;

    [SerializeField] Button[] m_selectButtons = default;
    [SerializeField] Button m_button_Play = default;

    Character m_selectedCharacter = default;
    Character m_nonPlayerCharacter = default;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_button_Play.onClick.AddListener(EnterGame);

        EnterScene();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        m_button_Play.onClick.RemoveAllListeners();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void SelectCharacter(int character)
    {
        m_selectionArrows[(int)m_selectedCharacter].SetActive(false);
        m_selectedCharacter = (Character)character;
        m_selectionArrows[(int)m_selectedCharacter].SetActive(true);

        do
        {
            m_nonPlayerCharacter = (Character)Random.Range(0, (int)Character.Max);
        }
        while (m_nonPlayerCharacter == m_selectedCharacter);
    }

    public void EnterGame()
    {
        GSTJ_Core.SelectedPlayerCharacter = m_selectedCharacter;
        GSTJ_Core.SelectedNonPlayerCharacter = m_nonPlayerCharacter;

        ExitScene();
        SceneManager.LoadScene(SceneConstants.Game);
    }

    void EnterScene()
    {
        // Selection Arrows
        for (int i = 0; i < m_selectionArrows.Length; i++)
        {
            m_selectionArrows[i].SetActive(false);
        }

        // Lock Status
        for (int i = 0; i < GSTJ_Core.CharacterMeta.Characters.Count; i++)
        {
            if (GSTJ_Core.Coins >= GSTJ_Core.CharacterMeta.Characters[i].CoinRequirement)
            {
                m_lockButtons[i].SetActive(false);
                m_selectButtons[i].targetGraphic = m_selectImages[i].GetComponent<Image>();
                m_selectButtons[i].interactable = true;
                m_selectImages[i].gameObject.SetActive(true);
            }
            else
            {
                m_lockButtons[i].SetActive(true);
                m_selectButtons[i].targetGraphic = m_lockButtons[i].GetComponent<Image>();
                m_selectButtons[i].interactable = false;
                m_selectImages[i].gameObject.SetActive(false);
                m_lockTexts[i].text = string.Format(GSTJ_Core.CharacterMeta.UnlockTexts, (GSTJ_Core.CharacterMeta.Characters[i].CoinRequirement).ToString());
            }
        }

        ScaleButtons(1.0f, 0.0f, 0.5f);
    }

    void ExitScene()
    {
        ScaleButtons(0.0f, 1.0f, 0.2f);
        m_menuManager.SwitchToScreen(MenuItem.Menus.Game);
    }

    void ScaleButtons(float scaleTo, float scaleFrom = 1.0f, float duration = 1.0f)
    {
        for (int i = 0; i < m_characters.Length; i++)
        {
            m_characters[i].transform.DOScale(scaleTo, duration).From(scaleFrom, true);
        }
    }
}
