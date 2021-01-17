using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class Pause : MonoBehaviour
{
    //for deciding if this is an in-game pause screen or end screen
    [SerializeField] bool isThisEndLevelScreen = true;
    [SerializeField] AudioSource m_gameEndSound = default;
    [SerializeField] NewHUDElements[] m_players = default;
    [SerializeField] Transform m_mainMenuBTN = default;

    //for showing the collectible data in sequence
    IEnumerator dispSeq = default;
    public void OnEnable()
    {
        //start the display sequence
        dispSeq = ShowDisplaySequence();
        StartCoroutine(dispSeq);

    }
    public void OnDisable()
    {
        StopCoroutine(dispSeq);
        //now hide the collectibles for reveal later
        for (int i=0; i < m_players.Length; i++)
        {
            Transform t = m_players[i].GetCollectibleRoot();
            t.gameObject.SetActive(false);
        }
    }

    //coroutine for displaying the reveal sequence
    private IEnumerator ShowDisplaySequence()
    {
        //disable main menu button at start
        if (m_mainMenuBTN != null)  m_mainMenuBTN.gameObject.SetActive(false);
        //if this is level end screen, start the bgm
        if (isThisEndLevelScreen == true)
        {
            m_gameEndSound.PlayDelayed(0.37f);
        }

        //first reveal the final score for player(s)
        for (int i=0; i < m_players.Length; i++)
        {
            Transform t = m_players[i].GetFinalScoreRoot();
            t.DOScale(1.0f, 0.5f).From(0.0f);
        }
        //wait for the final score(s) to scale up
        yield return new WaitForSeconds(1.0f);

        //now scale up the collectibles, if any
        for (int i=0; i < m_players.Length; i++)
        {
            Transform t = m_players[i].GetCollectibleRoot();
            t.gameObject.SetActive(true);
            t.DOScale(1.0f, 0.5f).From(0.0f);
        }
        //wait for the collectibles to scale up
        yield return new WaitForSeconds(1.5f);

        //for multiplayer, first show the winner if game has ended
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi
            && isThisEndLevelScreen == true)
        {
            Transform winner = default;

            //determine who is the winner
            int p1Score = m_players[0].GetFinalScore();
            int p2Score = m_players[1].GetFinalScore();
            //grab the winner transform
            if (p1Score > p2Score) 
                winner = m_players[0].GetWinnerRoot();
            else 
                winner = m_players[1].GetWinnerRoot();

            //scale the winner transform up
            winner.gameObject.SetActive(true);
            winner.DOScale(1.0f, 0.9f).From(0.0f);
            //wait for scaling up to finish
            yield return new WaitForSeconds(2.0f);
        }

        //now we show the high score tag for all players
        if (isThisEndLevelScreen == true)
        {
            //check if a player has highscore
            for (int i=0; i < m_players.Length; i++)
            {
                if (m_players[i].HighScoreCheck() == true)
                {
                    //generate a random name only for player 02
                    string name = "you";
                    if (i > 0) name = GenerateNewName();
                    
                    //add this score to the list
                    if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
                        GSTJ_Core.HighScoreList.AddHighScore(name, m_players[i].GetFlameScore(),
                                                               m_players[i].GetFinalScore());
                    else //if it's multiplayer mode
                        GSTJ_Core.HighScoreListMP.AddHighScore(name, m_players[i].GetFlameScore(),
                                                               m_players[i].GetFinalScore());
                    //show high score tag
                    Transform t = m_players[i].GetHighScoreTagRoot();
                    t.gameObject.SetActive(true);
                    t.DOScale(1.0f, 0.9f).From(0.0f);
                }
            }
        }
        //enable the main menu button
        if (m_mainMenuBTN != null)  
        {
            m_mainMenuBTN.gameObject.SetActive(true);
            m_mainMenuBTN.DOScale(1.0f, 0.2f).From(0.0f);
        }
        //that's it, reveal sequence is over
    }

    //a small function for generating a 4 letter name
    public string GenerateNewName()
    {
        string name = "";
        name = char.ConvertFromUtf32(Random.Range(97, 123)) +
               char.ConvertFromUtf32(Random.Range(97, 123)) +
               char.ConvertFromUtf32(Random.Range(97, 123)) +
               char.ConvertFromUtf32(Random.Range(97, 123));
        
        //return the randomly generated name
        return name;
    }

    //func for setting score data in the pause screen ui
    //by default the player is assumed to be player 01
    public void SetData(int fireCount, int score, HUD.PlayerHUD player = HUD.PlayerHUD.Player_01)
    {
        switch(player)
        {
            case HUD.PlayerHUD.Player_01:
            m_players[0].SetData(fireCount, score);
            break;

            case HUD.PlayerHUD.Player_02:
            //make sure that there are hud elements for player 02 configured
            if (m_players.Length > 1)
            {
                m_players[1].SetData(fireCount, score);
            }
            else 
                Debug.LogWarning("Trying to set player 02 data when none is configured! Gameobject: " + this.gameObject.name);
            break;
        }
    }

    public void SetCollectibleDataForPlayer(HUD.PlayerHUD player, Sprite sp, float score, int ID)
    {
        switch(player)
        {
            case HUD.PlayerHUD.Player_01:
            //if it's the first collectible then reset data
            if (ID == 1) m_players[0].ResetData();
            //et the data
            m_players[0].SetCollectibleData(sp, score, ID);
            break;

            case HUD.PlayerHUD.Player_02:
            //make sure that there are hud elements for player 02 configured
            if (m_players.Length > 1)
            {
                //if it's the first collectible then reset data
                if (ID == 1) m_players[1].ResetData();
                //set the data
                m_players[1].SetCollectibleData(sp, score, ID);
            }
            else 
                Debug.LogWarning("Trying to set player 02 data when none is configured! Gameobject: " + this.gameObject.name);
            break;
        }
    }

    //only for adding score to an existing collectible
    public void AddCollectibleScoreForPlayer(HUD.PlayerHUD player, float score, int ID)
    {
        switch(player)
        {
            case HUD.PlayerHUD.Player_01:
            m_players[0].AddCollectibleScore(score, ID);
            break;

            case HUD.PlayerHUD.Player_02:
            //make sure that there are hud elements for player 02 configured
            if (m_players.Length > 1)
            {
                m_players[1].AddCollectibleScore(score, ID);
            }
            else 
                Debug.LogWarning("Trying to set player 02 data when none is configured! Gameobject: " + this.gameObject.name);
            break;
        }
    }

    //for showing progress, result, collectible data in single and multiplayer
    [System.Serializable]
    public class NewHUDElements
    {
        [SerializeField] string name;

        [SerializeField] Text m_fireCount = default;
        public int GetFlameScore()
        {
            return int.Parse(m_fireCount.text, System.Globalization.NumberStyles.Integer);
        }
        [SerializeField] Text m_score = default;

        //for setting collectible image/score data
        public int GetFinalScore()
        {
            return int.Parse(m_score.text, System.Globalization.NumberStyles.Integer);
        }
        public Transform GetFinalScoreRoot()
        {
            return m_score.transform;
        }
        [SerializeField] Transform m_collectRoot = default;
        public Transform GetCollectibleRoot()
        {
            return m_collectRoot;
        }
        [SerializeField] Image m_col01 = default;
        [SerializeField] Image m_col02 = default;
        [SerializeField] Image m_col03 = default;

        [SerializeField] Text m_col01Text = default;
        [SerializeField] Text m_col02Text = default;
        [SerializeField] Text m_col03Text = default;

        //the winner and new highscore transforms
        [SerializeField] Transform winner = default;
        public Transform GetWinnerRoot()
        {
            return winner;
        }
        [SerializeField] Transform highScoreTag = default;
        public Transform GetHighScoreTagRoot()
        {
            return highScoreTag;
        }

        //for setting score and firecount
        public void SetData(int fireCount, int score)
        {
            m_fireCount.text = fireCount.ToString();
            m_score.text = score.ToString();
        }

        //for setting collectible image and score with ID
        public void SetCollectibleData(Sprite sp, float score, int ID)
        {
            Image shadow = null;
            //if ID is between 1 and 3, set image accordingly
            switch (ID)
            {
                case 1:
                m_col01.gameObject.SetActive(true);
                m_col01.sprite = sp;
                m_col01Text.gameObject.SetActive(true);
                m_col01Text.text = score.ToString();
                //remove shadow behind the collectible
                shadow = m_col01.transform.parent.gameObject.GetComponent<Image>();
                if (shadow != null) shadow.enabled = false;
                break;

                case 2:
                m_col02.gameObject.SetActive(true);
                m_col02.sprite = sp;
                m_col02Text.gameObject.SetActive(true);
                m_col02Text.text = score.ToString();
                //remove shadow behind the collectible
                shadow = m_col02.transform.parent.gameObject.GetComponent<Image>();
                if (shadow != null) shadow.enabled = false;
                break;

                case 3:
                m_col03.gameObject.SetActive(true);
                m_col03.sprite = sp;
                m_col03Text.gameObject.SetActive(true);
                m_col03Text.text = score.ToString();
                //remove shadow behind the collectible
                shadow = m_col03.transform.parent.gameObject.GetComponent<Image>();
                if (shadow != null) shadow.enabled = false;
                break;

                default:
                Debug.LogWarning("The collectible count was invalid: " + ID.ToString());
                break;
            }
        }

        //for adding only score to collectible
        public void AddCollectibleScore(float score, int ID)
        {
            float newScore = 0.0f;
            //if ID is between 1 and 3, set image accordingly
            switch (ID)
            {
                case 1:
                newScore = float.Parse(m_col01Text.text, System.Globalization.NumberStyles.Float);
                newScore += score;
                m_col01Text.text = newScore.ToString();
                break;

                case 2:
                newScore = float.Parse(m_col02Text.text, System.Globalization.NumberStyles.Float);
                newScore += score;
                m_col02Text.text = newScore.ToString();
                break;

                case 3:
                newScore = float.Parse(m_col03Text.text, System.Globalization.NumberStyles.Float);
                newScore += score;
                m_col03Text.text = newScore.ToString();
                break;

                default:
                Debug.LogWarning("The collectible ID was invalid: " + ID.ToString());
                break;
            }
        }

        //for checking if this player got high score
        public bool HighScoreCheck()
        {
            int finalScore = int.Parse(m_score.text, System.Globalization.NumberStyles.Integer);
            //list is different for single player and multiplayer mode 
            if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
            {
                return GSTJ_Core.HighScoreList.IsThisHighScore(finalScore);
            }
            else //for multiplayer mode
            {
                return GSTJ_Core.HighScoreListMP.IsThisHighScore(finalScore);
            }
        }

        //for clearing collectible data
        public void ResetData()
        {
            m_col01.gameObject.SetActive(false);
            m_col02.gameObject.SetActive(false);
            m_col03.gameObject.SetActive(false);
            m_col01Text.gameObject.SetActive(false);
            m_col02Text.gameObject.SetActive(false);
            m_col03Text.gameObject.SetActive(false);
            m_collectRoot.gameObject.SetActive(false);
            if (winner != null) 
                winner.gameObject.SetActive(false);
            if (highScoreTag != null)
                highScoreTag.gameObject.SetActive(false);
        }
    }
}
