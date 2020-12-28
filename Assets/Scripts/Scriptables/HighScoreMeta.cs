using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HighScores", menuName = "Meta/HighScores", order = 0)]
public class HighScoreMeta : ScriptableObject
{
    public List<HighScoreList> highScores;
    public int maxScoresToBeListed;

    //method to add a highscore, it takes care of the scenario when  
    //list is already full and an existing entry needs to be replaced
    public void AddHighScore(string name, int flameScore, int score)
    {
        //add stuff here
        HighScoreList entry = new HighScoreList();
        entry.playerName = name;
        entry.flameScore = flameScore;
        entry.finalScore = score;

        //check if we have reached max entries
        if (highScores.Count == maxScoresToBeListed)
            //remove the bottom most entry
            highScores.RemoveAt(highScores.Count - 1);
        
        //now let us find a position for this new entry
        int pos = GetIndexFor(score);
        //and insert it in the list
        if (pos < highScores.Count)
            //insert it at the specified position
            highScores.Insert(pos, entry);
        else
            //add it at the end
            highScores.Add(entry);
    }

    public bool IsThisHighScore(int score)
    {
        //check if we have space in the list
        if (highScores.Count < maxScoresToBeListed)
            return true;
        
        //check if the score is greater than bottom most score
        if (score > highScores[highScores.Count - 1].finalScore)
            return true;
        
        //if none of the above is met, return false
        //as there is no space and the score is lower than lowest score
        return false;
    }

    //note that this function doesn't work if the given score is not a highscore
    //use the IsThisHighScore() function to determine that before calling this function
    public int GetIndexFor(int score)
    {
        //first, let's check this score against the whole list
        int index = -1;
        for (int i = 0; i < highScores.Count; i++)
        {
            //if the score is equal to or higher than list score
            if (score >= highScores[i].finalScore)
            {
                //assign that as index
                index = i;
                break;
            }
        }
        //if we found that all the existing scores are higher
        //then we will add this score at the end
        if (index < 0) index = highScores.Count;

        return index;
    }
}

[System.Serializable]
public class HighScoreList
{
    public string playerName;
    public int flameScore;
    public int finalScore;
}
