using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    public enum PlayerHUD
    {
        Player_01,
        Player_02,
    }

    [SerializeField] HUDElements m_player_01_HUD = default;
    [SerializeField] HUDElements m_player_02_HUD = default;

    [SerializeField] Transform m_waterBar = default;

    [SerializeField] Button m_pauseButton = default;

    [SerializeField] GameObject m_bossDialog = default;
    [SerializeField] GameObject m_bossPanel = default;

    public void EnableHUD(PlayerHUD playerHUD, bool enable)
    {
        switch (playerHUD)
        {
            case PlayerHUD.Player_01:
                m_player_01_HUD.Root.SetActive(enable);
                break;

            case PlayerHUD.Player_02:
                m_player_02_HUD.Root.SetActive(enable);
                break;
        }
    }

    public void EnablePause(bool enable)
    {
        m_pauseButton.gameObject.SetActive(enable);
    }

    public void EnableBossDialog(bool enable)
    {
        float scaleTo = (enable) ? 1.0f : 0.0f;
        float scaleFrom = (enable) ? 0.0f : 1.0f;
        m_bossDialog.SetActive(enable);
        m_bossPanel.SetActive(enable);
        m_bossDialog.transform.DOScale(scaleTo, 0.5f).From(scaleFrom);
    }

    public void SetHUDCount(PlayerHUD playerHUD, LevelManager.ScoreType type, int value)
    {
        switch (playerHUD)
        {
            case PlayerHUD.Player_01:
                switch (type)
                {
                    case LevelManager.ScoreType.Score:
                        m_player_01_HUD.SetScoreCount(value);
                        break;

                    case LevelManager.ScoreType.Fire:
                        m_player_01_HUD.SetFireCount(value);
                        break;

                    case LevelManager.ScoreType.Coin:
                        m_player_01_HUD.SetCoinCount(value);
                        break;
                }
                break;

            case PlayerHUD.Player_02:
                switch (type)
                {
                    case LevelManager.ScoreType.Score:
                        m_player_02_HUD.SetScoreCount(value);
                        break;

                    case LevelManager.ScoreType.Fire:
                        m_player_02_HUD.SetFireCount(value);
                        break;

                    case LevelManager.ScoreType.Coin:
                        m_player_02_HUD.SetCoinCount(value);
                        break;
                }
                break;
        }
    }

    public void SetHUDCollectible(PlayerHUD playerHUD, Sprite sp, int ID)
    {
        switch (playerHUD)
        {
            case PlayerHUD.Player_01:
            m_player_01_HUD.SetCollectibleImage(sp, ID);
            break;

            case PlayerHUD.Player_02:
            m_player_01_HUD.SetCollectibleImage(sp, ID);
            break;
        }
    }

    public void SetProgress(PlayerHUD playerHUD, float normalizedVaue)
    {
        switch(playerHUD)
        {
            case PlayerHUD.Player_01:
                m_player_01_HUD.SetBarValue(normalizedVaue);
                break;

            case PlayerHUD.Player_02:
                m_player_02_HUD.SetBarValue(normalizedVaue);
                break;
        }
    }

    public void SetWaterValue(float availableWater)
    {
        m_waterBar.localScale = new Vector3(m_waterBar.localScale.x,
                                            availableWater,
                                            m_waterBar.localScale.z);
    }
}

[System.Serializable]
public class HUDElements
{
    [SerializeField] GameObject m_root = default;
    public GameObject Root => m_root;

    [SerializeField] Text m_flameCount = default;
    [SerializeField] Text m_coinCount = default;
    [SerializeField] Text m_scoreCount = default;

    [SerializeField] RectTransform m_progressBar = default;

    [SerializeField] Vector2 m_barLimits = default;
    [SerializeField] Image m_collectible01 = default;
    [SerializeField] Image m_collectible02 = default;
    [SerializeField] Image m_collectible03 = default;

    public void SetFireCount(int value)
    {
        m_flameCount.text = value.ToString();
    }

    public void SetCoinCount(int value)
    {
        m_coinCount.text = value.ToString();
    }

    public void SetScoreCount(int value)
    {
        m_scoreCount.text = value.ToString();
    }

    public void SetBarValue(float normalizedValue = 0)
    {
        float startValue = m_barLimits.x;
        float endValue = m_barLimits.y;
        m_progressBar.sizeDelta = new Vector2(startValue, m_progressBar.sizeDelta.y);

        float diff = endValue - startValue;
        float currentValue = diff * normalizedValue;
        m_progressBar.sizeDelta = new Vector2(startValue + currentValue, m_progressBar.sizeDelta.y);
    }

    public void SetCollectibleImage(Sprite sp, int ID)
    {
        //if ID is between 1 and 3, set image accordingly
        switch (ID)
        {
            case 1:
            m_collectible01.gameObject.SetActive(true);
            m_collectible01.sprite = sp;
            break;

            case 2:
            m_collectible02.gameObject.SetActive(true);
            m_collectible02.sprite = sp;
            break;

            case 3:
            m_collectible03.gameObject.SetActive(true);
            m_collectible03.sprite = sp;
            break;

            default:
            Debug.LogWarning("The collectible count was invalid: " + ID.ToString());
            break;
        }
    }
}
