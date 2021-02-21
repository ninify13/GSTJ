using UnityEngine;
using Game.Menu;

public class GSTJ_Core : MonoBehaviour
{
    public enum GameMode
    {
        Single,
        Multi,
    }

    public static GameMode SelectedMode = default;

    public static Characters.Character SelectedPlayerCharacter = default;
    public static Characters.Character SelectedNonPlayerCharacter = default;

    public static LevelMeta LevelMeta { get { return m_instance.m_levelMeta; } }

    //for highscores - single player and multiplayer
    public static HighScoreMeta HighScoreList { get { return m_instance.m_highScoreList; } }
    public static HighScoreMeta HighScoreListMP { get { return m_instance.m_highScoreListMP; } }
    //public static Transform HighScoreContainer;

    public static CharacterMetaList CharacterMeta { get { return m_instance.m_characterMeta; } }

    public static int Coins { get { return PlayerPrefs.GetInt(LevelManager.ScoreType.Coin.ToString(), 0); } }    

    static GSTJ_Core m_instance = default;

    [SerializeField] LevelMeta m_levelMeta = default;
    [SerializeField] CharacterMetaList m_characterMeta = default;
    [SerializeField] HighScoreMeta m_highScoreList = default;
    [SerializeField] HighScoreMeta m_highScoreListMP = default;
    public static bool m_ShowFTUE = true;

    void Awake()
    {
        DontDestroyOnLoad(this);

        m_instance = this;
    }

    public static void AddCoins(int amount) 
    { 
        int value = Coins + amount;
        PlayerPrefs.SetInt(PrefsConstants.Coins, value);
    } 
}
