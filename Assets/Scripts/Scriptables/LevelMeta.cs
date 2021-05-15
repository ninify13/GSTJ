using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelMeta", menuName = "Meta/Levels", order = 0)]
public class LevelMeta : ScriptableObject
{
    public List<LevelMetaObject> Levels;
}

[Serializable]
public class LevelMetaObject
{
    public string DifficultyKey;

    public Vector2 LevelSpeedMinMax;
    public int LevelTime;

    public int BossTime;
    public int BossFires;

    public Vector2 FireMinMaxPerSet;
    public Vector2 GapBetweenFiresInSet;
    public Vector2 TimeBetweenFireSets;

    public int Coins;

    public List<Vector2> CoinClumpWeights;

    public float WaterFillRate;
    public float WaterReduceRate;

    public int GetCoinClumpAmount()
    {
        int totalWeight = GetTotalCoinClumpWeight();
        //we are adding 1 to totalWeight as Random.Range 
        //excludes 2nd parameter in int version by default
        int random = UnityEngine.Random.Range(0, totalWeight+1);
        int result = (int)CoinClumpWeights[0].y;
        for (int i = 0; i < CoinClumpWeights.Count; i++)
        {
            if (random < (int)CoinClumpWeights[i].x)
            {
                result = (int)CoinClumpWeights[i].y;
                break;
            }
            random -= (int)CoinClumpWeights[i].x;
        }
        
        return result;
    }

    int GetTotalCoinClumpWeight()
    {
        int weight = 0;
        for (int i = 0; i < CoinClumpWeights.Count; i++)
        {
            weight += (int)CoinClumpWeights[i].x;
        }
        return weight;
    }
}
