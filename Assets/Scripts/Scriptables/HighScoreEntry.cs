using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HighScoreEntry : MonoBehaviour
{
    [SerializeField]
    private Text rank;
    [SerializeField]
    private Text playerName;
    [SerializeField]
    private Text flameScore;
    [SerializeField]
    private Text score;

    //function for populating an entry
    public void PopulateEntry(string playerRank, string name, int flames, int finalScore)
    {
        rank.text = playerRank;
        playerName.text = name;
        flameScore.text = flames.ToString();
        score.text = finalScore.ToString();
    }
}