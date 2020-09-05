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
        int random = UnityEngine.Random.Range(0, totalWeight);
        int result = (int)CoinClumpWeights.FindLast(weight => weight.x < random).y;

        if (result == 0)
        {
            result = (int)CoinClumpWeights[0].y;
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
