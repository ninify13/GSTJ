using Core.Menu;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Menu
{
    public class Scores : MenuLayout
    {
        [SerializeField] private Transform scoreContainer;
        [SerializeField] private GameObject sampleScoreEntry;
        protected override void OnEnable()
        {
            base.OnEnable();
            //need to remove existing rows in the table
            //index is starting from 0 as sample row is disabled by default and won't
            //be returned using GetComponentsInChildren
            HighScoreEntry[] curList = scoreContainer.GetComponentsInChildren<HighScoreEntry>();
            for (int i = 0; i < curList.Length; i++)
            {
                //destroy the list items
                Destroy(curList[i].gameObject);
            }
            //now let's get the req num of entries from the high score meta
            int numOfEntries = GSTJ_Core.HighScoreList.highScores.Count;

            //let's instantiate new rows, populate them with data and add them to the table
            for (int i = 0; i < numOfEntries; i++)
            {
                //get listing data from the highscore meta object
                HighScoreList listData = GSTJ_Core.HighScoreList.highScores[i];

                //create a new entry using sample entry
                GameObject entryObj = Instantiate(sampleScoreEntry, 
                                                  sampleScoreEntry.transform.position, 
                                                  sampleScoreEntry.transform.rotation);
                //move it in proper position and make it visible
                entryObj.transform.SetParent(sampleScoreEntry.transform.parent);
                //resetting the scale to make it visible
                entryObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                entryObj.SetActive(true);
                HighScoreEntry entry = entryObj.GetComponent<HighScoreEntry>();

                //let us populate this entry using the data from highscore meta object
                string rank = "";
                if (i<=8) 
                    rank = "0" + (i+1).ToString();
                else 
                    rank = i.ToString();
                entry.PopulateEntry(rank, listData.playerName, listData.flameScore, 
                                                               listData.finalScore);
            }
        }
    }
}
