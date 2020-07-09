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
    public Vector2 LevelSpeedMinMax;
    public int LevelTime;

    public int BossTime;
    public int BossFires;

    public Vector2 FireMinMaxPerSet;
    public Vector2 GapBetweenFiresInSet;
    public Vector2 TimeBetweenFireSets;

    public int Coins;

    public float WaterFillRate;
    public float WaterReduceRate;
}
