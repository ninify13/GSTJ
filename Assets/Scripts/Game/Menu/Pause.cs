using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField] NewHUDElements[] m_players = default;

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
            if (ID == 1) m_players[0].ResetCollectibleData();
            //et the data
            m_players[0].SetCollectibleData(sp, score, ID);
            break;

            case HUD.PlayerHUD.Player_02:
            //make sure that there are hud elements for player 02 configured
            if (m_players.Length > 1)
            {
                //if it's the first collectible then reset data
                if (ID == 1) m_players[1].ResetCollectibleData();
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
        [SerializeField] Text m_score = default;

        //for setting collectible image/score data
        [SerializeField] Image m_col01 = default;
        [SerializeField] Image m_col02 = default;
        [SerializeField] Image m_col03 = default;

        [SerializeField] Text m_col01Text = default;
        [SerializeField] Text m_col02Text = default;
        [SerializeField] Text m_col03Text = default;

        //for setting score and firecount
        public void SetData(int fireCount, int score)
        {
            m_fireCount.text = fireCount.ToString();
            m_score.text = score.ToString();
        }

        //for setting collectible image and score with ID
        public void SetCollectibleData(Sprite sp, float score, int ID)
        {
            //if ID is between 1 and 3, set image accordingly
            switch (ID)
            {
                case 1:
                m_col01.gameObject.SetActive(true);
                m_col01.sprite = sp;
                m_col01Text.gameObject.SetActive(true);
                m_col01Text.text = score.ToString();
                break;

                case 2:
                m_col02.gameObject.SetActive(true);
                m_col02.sprite = sp;
                m_col02Text.gameObject.SetActive(true);
                m_col02Text.text = score.ToString();
                break;

                case 3:
                m_col03.gameObject.SetActive(true);
                m_col03.sprite = sp;
                m_col03Text.gameObject.SetActive(true);
                m_col03Text.text = score.ToString();
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

        //for clearing collectible data
        public void ResetCollectibleData()
        {
            m_col01.gameObject.SetActive(false);
            m_col02.gameObject.SetActive(false);
            m_col03.gameObject.SetActive(false);
            m_col01Text.gameObject.SetActive(false);
            m_col02Text.gameObject.SetActive(false);
            m_col03Text.gameObject.SetActive(false);
        }
    }
}
